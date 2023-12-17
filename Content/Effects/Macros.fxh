#ifdef DX11
#define TECHNIQUE(name, psname ) \
	technique name { pass { PixelShader = compile ps_5_0 psname(); } }
#define PASS(psname ) \
	pass { PixelShader = compile ps_5_0 psname(); }
#define DECLARE_TEXTURE(Name, index) \
    Texture2D<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index)
#define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#else
#define TECHNIQUE(name, psname ) \
	technique name { pass { PixelShader = compile ps_3_0 psname(); } }
#define PASS(psname ) \
	pass { PixelShader = compile ps_3_0 psname(); }
#define DECLARE_TEXTURE(Name, index) \
    sampler2D Name : register(s##index);
#define SAMPLE_TEXTURE(Name, texCoord)  tex2D(Name, texCoord)
#endif

struct PSInput
{
    float4 position : SV_Position;
    float4 color : COLOR0;
    float2 texCoord : TEXCOORD0;
};