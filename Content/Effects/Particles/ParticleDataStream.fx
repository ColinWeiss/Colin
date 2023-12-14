struct VertexShaderInput
{
    float3 Position : POSITION0;
    float4 Color : COLOR1;
    float2 WorldPosition : POSITION1;
    float2 Velocity : POSITION2;
    float2 Scale : TEXCOORD1;
    float2 ScaleVel : TEXCOORD2;
    float Rotation : TEXCOORD3;
    float RotationVelocity : TEXCOORD4;
    float ActiveTime : TEXCOORD5;
    int Active : TEXCOORD6;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR1;
    float2 WorldPosition : POSITION1;
    float2 Velocity : POSITION2;
    float2 Scale : TEXCOORD1;
    float2 ScaleVel : TEXCOORD2;
    float Rotation : TEXCOORD3;
    float RotationVelocity : TEXCOORD4;
    float ActiveTime : TEXCOORD5;
    int Active : TEXCOORD6;
};
