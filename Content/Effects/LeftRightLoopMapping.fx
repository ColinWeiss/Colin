#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0

#include "Macros.fxh"

//映射图在底图上的重复次数.
//若映射图本身小于底图且重复绘制后不足以铺满底图,
//则会进行拉伸.
float2 DrawCount;

//映射图在底图上的偏移绘制量.
float2 Offset;

DECLARE_TEXTURE(SpriteTexture, 0);
DECLARE_TEXTURE(MappingTexture, 1);

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 PixelShaderFunction( VertexShaderOutput input ) : COLOR
{
    float2 uv = (input.TextureCoordinates - 0.5) * DrawCount + Offset;
    float4 MappingColor = SAMPLE_TEXTURE(MappingTexture, uv);
    MappingColor = max( 0, sign( -uv.y * (uv.y - 1) ) ) * MappingColor;
    return SAMPLE_TEXTURE(SpriteTexture, input.TextureCoordinates) * MappingColor;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
};