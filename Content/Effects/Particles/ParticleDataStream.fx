#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

#include "../ColinMacros.fxh"

float4x4 Transform;
int ParticleCountMax;

struct VertexShaderInput
{
    float3 Position : POSITION0; // 四个模板顶点的位置.
    float4 Color : COLOR0;
    float2 WorldPosition : POSITION1;
    float2 Velocity : TEXCOORD1;
    float2 Scale : TEXCOORD2;
    float Rotation : TEXCOORD3;
    float ActiveTime : TEXCOORD4;
    float ID : TEXCOORD5;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 WorldPosition : POSITION1;
    float2 Velocity : TEXCOORD1;
    float2 Scale : TEXCOORD2;
    float Rotation : TEXCOORD3;
    float ActiveTime : TEXCOORD4;
    float ID : TEXCOORD5;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(float4(input.Position + float3(input.ID, 0, 0), 1), Transform);
    output.Color = input.Color;
    output.WorldPosition = input.WorldPosition;
    output.Velocity = input.Velocity;
    output.Scale = input.Scale;
    output.Rotation = input.Rotation;
    output.ActiveTime = input.ActiveTime;
    output.ID = input.ID;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    if (input.Position.y <= 1)
        return input.Color;
    else if (input.Position.y <= 2)
        return float4(input.WorldPosition.x, input.WorldPosition.y, input.Velocity.x, input.Velocity.y);
    else if (input.Position.y <= 3)
        return float4(input.Scale.x, input.Scale.y, input.Rotation, input.ActiveTime);
    else
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