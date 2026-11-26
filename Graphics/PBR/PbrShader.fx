// =====================================================================
// PBR 前向渲染着色器 (metallic-roughness / specular-glossiness 双工作流)
// 直接光照 = Cook-Torrance (GGX / Smith / Schlick); 环境项目前为程序化天空近似,
// 后续换预过滤环境图 + BRDF LUT 时只需替换 EnvironmentColor.
// 骨骼蒙皮 = 骨骼矩阵纹理 (每根骨骼 4 行 texel, 行存 XNA 行向量矩阵的 4 行).
// =====================================================================

Texture2D<float4> BaseColorTexture : register(t0);
Texture2D<float4> MetallicRoughnessTexture : register(t1);
Texture2D<float4> NormalTexture : register(t2);
Texture2D<float4> OcclusionTexture : register(t3);
Texture2D<float4> EmissiveTexture : register(t4);
Texture2D<float4> BoneTexture : register(t5);
Texture2D<float4> EnvironmentTexture : register(t6);

SamplerState LinearSampler : register(s0)
{
    Filter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

SamplerState PointSampler : register(s1)
{
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

float4x4 World;
float4x4 ViewProjection;
float4x4 InverseViewProjection;
float3 EyePosition;
float EnvironmentRotation;

float3 DirLight0Direction;
float3 DirLight0Color;
float3 DirLight1Direction;
float3 DirLight1Color;
float3 DirLight2Direction;
float3 DirLight2Color;
float3 EnvironmentTint;
float EnvironmentIntensity;
float HasEnvironmentTexture;
float Exposure;

float4 BaseColorFactor;
float MetallicFactor;
float RoughnessFactor;
float3 SpecularFactor;
float GlossinessFactor;
float3 EmissiveFactor;
float EmissiveStrength;
float NormalScale;
float NormalMapFlipY;
float OcclusionStrength;
float AlphaCutoff;
float AlphaMode;
float Workflow;
float GameLighting;
float BoneCount;

struct VSInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Tangent : TANGENT0;
    float2 TexCoord : TEXCOORD0;
    int4 BlendIndices : BLENDINDICES0;
    float4 BlendWeights : BLENDWEIGHT0;
};

struct VSOutput
{
    float4 Position : SV_Position;
    float3 PositionWS : TEXCOORD0;
    float3 NormalWS : TEXCOORD1;
    float3 TangentWS : TEXCOORD2;
    float3 BitangentWS : TEXCOORD3;
    float2 TexCoord : TEXCOORD4;
};

#define PI 3.14159265359

float3 SRGBToLinear(float3 color)
{
    return pow(max(color, 0.0), 2.2);
}

float3 LinearToSRGB(float3 color)
{
    return pow(max(color, 0.0), 1.0 / 2.2);
}

// ACES filmic 近似 (Narkowicz)
float3 ToneMap(float3 color)
{
    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    return saturate((color * (a * color + b)) / (color * (c * color + d) + e));
}

float3 RotateAroundY(float3 direction, float radians)
{
    float sine = sin(radians);
    float cosine = cos(radians);
    return float3(
        direction.x * cosine + direction.z * sine,
        direction.y,
        -direction.x * sine + direction.z * cosine);
}

float3 EnvironmentColor(float3 direction, float lod)
{
    direction = RotateAroundY(direction, EnvironmentRotation);
    float3 color;
    if (HasEnvironmentTexture > 0.5)
    {
        // 等距柱状贴图: u 沿水平 360 度, v 从上方 0 到下方 1.
        float2 uv;
        uv.x = atan2(direction.z, direction.x) / (2.0 * PI) + 0.5;
        uv.y = 0.5 - asin(clamp(direction.y, -1.0, 1.0)) / PI;
        color = SRGBToLinear(EnvironmentTexture.SampleLevel(LinearSampler, uv, lod).rgb);
    }
    else
    {
        float up = saturate(direction.y * 0.5 + 0.5);
        float3 zenith = float3(0.16, 0.28, 0.52);
        float3 horizon = float3(0.62, 0.68, 0.76);
        color = lerp(horizon, zenith, up);
    }
    return color * EnvironmentTint;
}

float4 FetchBoneRow4(float bone, float row)
{
    float u = (bone + 0.5) / BoneCount;
    float v = (row + 0.5) * 0.25;
    return BoneTexture.SampleLevel(PointSampler, float2(u, v), 0);
}

VSOutput BuildOutput(float4 positionWS, float3 normalWS, float3 tangentWS, float tangentSign, float2 texCoord)
{
    VSOutput output;
    output.Position = mul(positionWS, ViewProjection);
    output.PositionWS = positionWS.xyz;
    output.NormalWS = normalWS;
    output.TangentWS = tangentWS;
    output.BitangentWS = cross(normalWS, tangentWS) * tangentSign;
    output.TexCoord = texCoord;
    return output;
}

void SkinVertex(VSInput input, out float3 positionWS, out float3 normalWS, out float3 tangentWS)
{
    float4x4 skin = float4x4(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    float weightSum = 0.0;

    [unroll]
    for (int i = 0; i < 4; i++)
    {
        float weight = input.BlendWeights[i];
        if (weight <= 0.0)
            continue;
        float bone = (float)input.BlendIndices[i];
        float4 r0 = FetchBoneRow4(bone, 0.0);
        float4 r1 = FetchBoneRow4(bone, 1.0);
        float4 r2 = FetchBoneRow4(bone, 2.0);
        float4 r3 = FetchBoneRow4(bone, 3.0);
        skin += float4x4(r0, r1, r2, r3) * weight;
        weightSum += weight;
    }

    if (weightSum > 0.0)
        skin /= weightSum;
    else
        skin = float4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

    float4 skinned = mul(float4(input.Position.xyz, 1.0), skin);
    positionWS = mul(float4(skinned.xyz, 1.0), World).xyz;
    normalWS = mul(float4(mul(float4(input.Normal, 0.0), skin).xyz, 0.0), World).xyz;
    tangentWS = mul(float4(mul(float4(input.Tangent.xyz, 0.0), skin).xyz, 0.0), World).xyz;
}

VSOutput VSStatic(VSInput input)
{
    float4 positionWS = mul(float4(input.Position.xyz, 1.0), World);
    float3 normalWS = mul(float4(input.Normal, 0.0), World).xyz;
    float3 tangentWS = mul(float4(input.Tangent.xyz, 0.0), World).xyz;
    return BuildOutput(positionWS, normalWS, tangentWS, input.Tangent.w, input.TexCoord);
}

VSOutput VSSkinned(VSInput input)
{
    float3 positionWS;
    float3 normalWS;
    float3 tangentWS;
    SkinVertex(input, positionWS, normalWS, tangentWS);
    return BuildOutput(float4(positionWS, 1.0), normalWS, tangentWS, input.Tangent.w, input.TexCoord);
}

float DistributionGGX(float NdotH, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float d = NdotH * NdotH * (a2 - 1.0) + 1.0;
    return a2 / max(PI * d * d, 0.000001);
}

float GeometrySchlickGGX(float NdotX, float roughness)
{
    float r = roughness + 1.0;
    float k = (r * r) / 8.0;
    return NdotX / max(NdotX * (1.0 - k) + k, 0.000001);
}

float GeometrySmith(float NdotV, float NdotL, float roughness)
{
    return GeometrySchlickGGX(NdotV, roughness) * GeometrySchlickGGX(NdotL, roughness);
}

float3 FresnelSchlick(float VdotH, float3 F0)
{
    return F0 + (1.0 - F0) * pow(saturate(1.0 - VdotH), 5.0);
}

float3 ComputeLight(float3 N, float3 V, float3 diffuseColor, float3 F0, float roughness, float3 lightDirection, float3 lightColor)
{
    float3 L = normalize(-lightDirection);
    float NdotL = saturate(dot(N, L));
    if (NdotL <= 0.0)
        return float3(0, 0, 0);

    float NdotV = max(dot(N, V), 0.0001);
    float3 H = normalize(L + V);
    float NdotH = saturate(dot(N, H));
    float VdotH = saturate(dot(V, H));

    float D = DistributionGGX(NdotH, roughness);
    float G = GeometrySmith(NdotV, NdotL, roughness);
    float3 F = FresnelSchlick(VdotH, F0);

    float3 specular = (D * G * F) / max(4.0 * NdotV * NdotL, 0.0001);
    float3 kD = 1.0 - F;
    return (kD * diffuseColor / PI + specular) * lightColor * NdotL;
}

float4 PSMain(VSOutput input) : COLOR
{
    float4 baseSample = BaseColorTexture.Sample(LinearSampler, input.TexCoord);
    float4 workflowSample = MetallicRoughnessTexture.Sample(LinearSampler, input.TexCoord);

    float3 albedo;
    float3 diffuseColor;
    float3 F0;
    float roughness;
    float alpha;

    if (Workflow < 0.5)
    {
        // metallic-roughness: baseColor 为 sRGB, 金属度/粗糙度在 G/B 通道 (OR M 纹理).
        albedo = BaseColorFactor.rgb * SRGBToLinear(baseSample.rgb);
        float metallic = saturate(MetallicFactor * workflowSample.b);
        roughness = RoughnessFactor * workflowSample.g;
        F0 = lerp(0.04, albedo, metallic);
        diffuseColor = albedo * (1.0 - metallic);
        alpha = BaseColorFactor.a * baseSample.a;
    }
    else
    {
        // specular-glossiness: specular RGB 为 F0 (sRGB), glossiness 在 A 通道.
        albedo = BaseColorFactor.rgb * SRGBToLinear(baseSample.rgb);
        float3 specular = saturate(SpecularFactor * SRGBToLinear(workflowSample.rgb));
        float glossiness = saturate(GlossinessFactor * workflowSample.a);
        roughness = 1.0 - glossiness;
        F0 = specular;
        diffuseColor = albedo * (1.0 - max(max(specular.r, specular.g), specular.b));
        alpha = BaseColorFactor.a * baseSample.a;
    }

    roughness = clamp(roughness, 0.04, 1.0);

    if (AlphaMode < 0.5)
        alpha = 1.0;
    else if (AlphaMode < 1.5)
    {
        clip(alpha - AlphaCutoff);
        alpha = 1.0;
    }

    float3 N = normalize(input.NormalWS);
    float3 T = normalize(input.TangentWS - N * dot(N, input.TangentWS));
    float3 B = normalize(input.BitangentWS);

    if (GameLighting > 0.5)
    {
        // 交给游戏原有 2D 光照: 只输出材质底色 + 环境反射, Ecs 的 Light 后处理会统一乘上来.
        float3 view = normalize(EyePosition - input.PositionWS);
        float3 reflected = reflect(-view, N);
        float3 reflection = EnvironmentColor(reflected, roughness * 8.0) * F0 * (1.0 - roughness);
        return float4(LinearToSRGB(albedo + reflection * EnvironmentIntensity), alpha);
    }

    float3 tangentNormal = NormalTexture.Sample(LinearSampler, input.TexCoord).xyz * 2.0 - 1.0;
    // glTF 法线贴图为 OpenGL 约定 (+Y 朝图像上方), 而 D3D 采样时 +V 朝图像下方, 故翻转绿通道.
    tangentNormal.y *= NormalMapFlipY;
    tangentNormal.xy *= NormalScale;
    N = normalize(T * tangentNormal.x + B * tangentNormal.y + N * tangentNormal.z);

    float3 V = normalize(EyePosition - input.PositionWS);

    float3 color = float3(0, 0, 0);
    color += ComputeLight(N, V, diffuseColor, F0, roughness, DirLight0Direction, DirLight0Color);
    color += ComputeLight(N, V, diffuseColor, F0, roughness, DirLight1Direction, DirLight1Color);
    color += ComputeLight(N, V, diffuseColor, F0, roughness, DirLight2Direction, DirLight2Color);

    float occlusion = lerp(1.0, OcclusionTexture.Sample(LinearSampler, input.TexCoord).r, OcclusionStrength);
    float3 reflection = reflect(-V, N);
    float3 ambientDiffuse = EnvironmentColor(N, 5.0) * diffuseColor;
    float3 ambientSpecular = EnvironmentColor(reflection, roughness * 8.0) * F0 * (1.0 - roughness);
    color += (ambientDiffuse + ambientSpecular) * occlusion * EnvironmentIntensity;

    float3 emissive = EmissiveFactor * EmissiveStrength * SRGBToLinear(EmissiveTexture.Sample(LinearSampler, input.TexCoord).rgb);
    color += emissive;

    color = ToneMap(color * Exposure);
    return float4(LinearToSRGB(color), alpha);
}

struct SkyVSOutput
{
    float4 Position : SV_Position;
    float3 Direction : TEXCOORD0;
};

SkyVSOutput VSSky(uint vertexId : SV_VertexID)
{
    // 全屏三角形: (-1,-1), (3,-1), (-1,3)
    float2 uv = float2((vertexId << 1) & 2, vertexId & 2);
    float2 ndc = uv * 2.0 - 1.0;

    SkyVSOutput output;
    output.Position = float4(ndc, 1.0, 1.0);
    float4 world = mul(float4(ndc, 1.0, 1.0), InverseViewProjection);
    output.Direction = world.xyz / world.w - EyePosition;
    return output;
}

float4 PSSky(SkyVSOutput input) : COLOR
{
    float3 direction = normalize(input.Direction);
    float3 color = ToneMap(EnvironmentColor(direction, 0.0) * Exposure);
    return float4(LinearToSRGB(color), 1.0);
}

technique Skybox
{
    pass P0
    {
        VertexShader = compile vs_5_0 VSSky();
        PixelShader = compile ps_5_0 PSSky();
    }
};

technique Static
{
    pass P0
    {
        VertexShader = compile vs_5_0 VSStatic();
        PixelShader = compile ps_5_0 PSMain();
    }
};

technique Skinned
{
    pass P0
    {
        VertexShader = compile vs_5_0 VSSkinned();
        PixelShader = compile ps_5_0 PSMain();
    }
};
