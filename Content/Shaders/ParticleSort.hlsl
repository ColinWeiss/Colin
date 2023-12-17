Texture2D inTex : register(t0);
RWTexture2D<float4> outTex : register(u0);

[numthreads(16, 4, 1)]
void Main(uint3 DTid : SV_DispatchThreadID)
{
    float4 center = inTex[int2(DTid.x, 3)];
    float4 left = inTex[int2(DTid.x - 1, 3)];
    float4 right = inTex[int2(DTid.x + 1, 0)];
    if (left.a > 0 || DTid.x <= 0)
    {
        outTex[int2(DTid.x, 0)] = inTex[int2(DTid.x, 0)];
        outTex[int2(DTid.x, 1)] = inTex[int2(DTid.x, 1)];
        outTex[int2(DTid.x, 2)] = inTex[int2(DTid.x, 2)];
        outTex[int2(DTid.x, 3)] = inTex[int2(DTid.x, 3)];
    }
    else
    {
        outTex[int2(DTid.x, 0)] = float4(0, 0, 0, 0);
        outTex[int2(DTid.x, 1)] = float4(0, 0, 0, 0);
        outTex[int2(DTid.x, 2)] = float4(0, 0, 0, 0);
        outTex[int2(DTid.x, 3)] = float4(0, 0, 0, 0);
    }
    if (center.a <= 0)
    {
        outTex[int2(DTid.x, 0)] = inTex[int2(DTid.x + 1, 0)];
        outTex[int2(DTid.x, 1)] = inTex[int2(DTid.x + 1, 1)];
        outTex[int2(DTid.x, 2)] = inTex[int2(DTid.x + 1, 2)];
        outTex[int2(DTid.x, 3)] = inTex[int2(DTid.x + 1, 3)];
    }
}