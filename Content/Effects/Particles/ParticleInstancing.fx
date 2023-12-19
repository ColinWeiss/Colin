Texture2D<float4> SpriteTexture : register(t0);
sampler SpriteTextureSampler : register(s0);

Texture2D<float4> Data : register(t1);
sampler2D DataSampler : register(s1);

float4x4 Transform;
float4x4 DataTransform;

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float ID : TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float ID : TEXCOORD1;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;
    half4 pos = tex2Dlod(DataSampler, float4(0.5, 0.5, 0.0 ,0.0));
    output.Position = mul(float4(input.Position + float3(input.ID * 28, pos.y, 0), 1), Transform);
    output.TexCoord = input.TexCoord;
    output.ID = input.ID;
    output.Color = pos;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return float4(input.Color.r, input.Color.g, input.Color.b , 1);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile vs_5_0 MainVS();
        PixelShader = compile ps_5_0 MainPS();
    }
};