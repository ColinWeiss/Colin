#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0

float4x4 DataTransform;

Texture2D<float4> Tex : register(t0);
sampler TexSampler : register(s0);

Texture2D<float4> DataRt : register(t1);
sampler DataRtSampler : register(s1);

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 Coord : TEXCOORD0;
    float ID : TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 Coord : TEXCOORD0;
};

VertexShaderOutput MainVS( 
    float3 Position : POSITIONT0,
    float2 Coord : TEXCOORD0,
    float ID : TEXCOORD1
    )
{
    VertexShaderOutput output;
    output.Position = tex2Dlod(DataRtSampler, mul(float4(ID, 0, 0, 0), DataTransform));
    output.Coord = Coord;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return float4( 0 , 0 , 0 , 0 );
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};