Texture2D inTex : register(t0);
RWTexture2D<float4> outTex : register(u0);

[numthreads(16, 16, 1)]
void Main(uint3 DTid : SV_DispatchThreadID)
{
    if (DTid.x - 1 <= 0)
        return;
    if (outTex[uint2(DTid.x - 1, 0)].a <= 0)
    {
        outTex[int2(DTid.x - 1, 0)] = inTex[int2(DTid.x, 0)];
        outTex[int2(DTid.x - 1, 1)] = inTex[int2(DTid.x, 1)];
        outTex[int2(DTid.x - 1, 2)] = inTex[int2(DTid.x, 2)];
        outTex[int2(DTid.x - 1, 3)] = inTex[int2(DTid.x, 3)];
    }
}