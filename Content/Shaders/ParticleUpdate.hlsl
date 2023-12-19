Texture2D inTex : register(t0);
RWTexture2D<float4> outTex : register(u0);

[numthreads(32, 3, 1)]
void Main(uint3 DTid : SV_DispatchThreadID)
{
    float2 pos = inTex[int2(DTid.x, 1)].rg;
    float2 vel = inTex[int2(DTid.x, 1)].ba;
    float2 sca = inTex[int2(DTid.x, 2)].rg;
    float rot = inTex[int2(DTid.x, 2)].b;
    float activeTime = inTex[int2(DTid.x, 2)].a;
    outTex[int2(DTid.x, 0)] = inTex[int2(DTid.x, 0)];
    outTex[int2(DTid.x, 1)] = float4(pos.x + vel.x, pos.y + vel.y, vel.x , vel.y);
    outTex[int2(DTid.x, 2)] = float4(sca.x, sca.y, rot, activeTime);
}