#define TechniqueFunc(name, psname ) \
	technique name { pass { PixelShader = compile ps_5_0 psname(); } }
#define PassFunc( psname ) \
	pass { PixelShader = compile ps_5_0 psname(); }
#define DeclareTex(Name, index) \
    Texture2D<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index)
#define SampleTex(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#define Sampler( Name ) Name##Sampler
#define VsShaderModel vs_5_0
#define PsShaderModel ps_5_0

struct PixelShaderInput
{
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};