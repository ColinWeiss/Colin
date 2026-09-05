// =====================================================================
// 刀光条带 Mesh 着色器 (MonoGame Effect, DirectX_11):
// 顶点由 RibbonBuilder 直接摆在轨迹/弧线上 (拉刀光, Unity trail 同思路),
// PS 采样梭形渐变纹理 × 顶点色 (头部亮、尾部渐隐在顶点色/UV 中完成).
// =====================================================================

Texture2D<float4> SpriteTexture : register(t0);
sampler SpriteTextureSampler : register(s0);

// 世界坐标 -> 裁剪空间 的完整变换 (View * Projection).
float4x4 Transform;

struct VertexShaderInput
{
    float2 Position : POSITION0;
    float2 Uv : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 Uv : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(float4(input.Position, 0, 1), Transform);
    output.Uv = input.Uv;
    output.Color = input.Color;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 texel = SpriteTexture.Sample(SpriteTextureSampler, input.Uv);
    return texel * input.Color;
}

technique SlashRendering
{
    pass P0
    {
        VertexShader = compile vs_5_0 MainVS();
        PixelShader = compile ps_5_0 MainPS();
    }
};
