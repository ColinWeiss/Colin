Texture2D inTex : register(t0);
RWTexture2D<float4> outTex : register(u0);

[numthreads(16, 4, 1)]
void Main(uint3 DTid : SV_DispatchThreadID)
{
    float2 pos = inTex[int2(DTid.x, 2)].rg;
    float2 vel = inTex[int2(DTid.x, 2)].ba;
    float2 sca = inTex[int2(DTid.x, 3)].rg;
    float2 scaVel = inTex[int2(DTid.x, 3)].ba;
    float rot = inTex[int2(DTid.x, 4)].r;
    float rotVel = inTex[int2(DTid.x, 4)].g;
    float activeTime = inTex[int2(DTid.x, 4)].b;
    float active = inTex[int2(DTid.x, 4)].a;
    outTex[int2(DTid.x, 2)] = float4(pos + vel, vel);
    outTex[int2(DTid.x, 3)] = float4(sca + scaVel, scaVel);
    outTex[int2(DTid.x, 4)] = float4(rot + rotVel, rotVel, activeTime, active);
}