#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#include "Macros.fxh"

float2 screenSize;

DECLARE_TEXTURE(SpriteTexture, 0);

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS( VertexShaderOutput input ) : COLOR
{
    float2 texelSize = 1.0 / screenSize;
    float4 finalColor = 0.0;
    // 高斯模糊采样过程
    for (int i = -2; i <= 4; ++i)
    {
        float2 offset = texelSize * float2( 1, 0 ) * i;
        finalColor += SAMPLE_TEXTURE(SpriteTexture, input.TextureCoordinates + offset);
    }
    
    for (int j = -2; j <= 4; ++j)
    {
        float2 offset = texelSize * float2( 0, 1 ) * j;
        finalColor += SAMPLE_TEXTURE(SpriteTexture, input.TextureCoordinates + offset);
    }
    
    return finalColor / (8 * 2 + 1);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};