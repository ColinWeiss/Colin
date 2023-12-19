Texture2D<float4> SpriteTexture : register(t0);
sampler SpriteTextureSampler : register(s0);

Texture2D<float4> Data : register(t1);
sampler DataSampler : register(s1);

float4x4 Transform;
float ParticleCountMax;

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
    float2 samplePos = float2(input.ID / ParticleCountMax, 0.5);
    float4 pos = Data.SampleLevel(DataSampler, samplePos, 0);
    output.Position = mul(float4(input.Position + float3(pos.rg , 0), 1), Transform);
    output.TexCoord = input.TexCoord;
    output.ID = input.ID;
    output.Color = pos;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile vs_5_0 MainVS();
        PixelShader = compile ps_5_0 MainPS();
    }
};