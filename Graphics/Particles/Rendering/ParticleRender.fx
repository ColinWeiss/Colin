// =====================================================================
// 粒子渲染着色器 (MonoGame Effect, mgfxc /Profile:DirectX_11 编译).
// 双流实例化绘制: 流 0 = 四边形角点模板 (每顶点), 流 1 = 粒子数据 (每实例一个 Particle, 80 字节).
// 支持两种渲染模式: 普通公告牌 (自旋转) 与 速度拉伸公告牌 (刀光等高速条状效果).
// =====================================================================

Texture2D<float4> SpriteTexture : register(t0);
sampler SpriteTextureSampler : register(s0);

// 世界坐标 -> 裁剪空间 的完整变换 (View * Projection).
float4x4 Transform;
// 速度拉伸系数 (秒): 拉伸长度增量 = |速度| * StretchFactor.
float StretchFactor;
// 速度拉伸最大长度 (像素).
float MaxStretchLength;

// ---- 流 0: 四边形角点 (三角带 4 顶点, ±0.5) ----
struct QuadVertex
{
    float2 Corner : POSITION0;
    float2 Uv : TEXCOORD0;
};

// ---- 流 1: 粒子实例数据 (与 Particle.Core.Particle 逐字节对应) ----
struct ParticleInstance
{
    float4 PosVel : COLOR0;             // 位置.xy, 速度.xy
    float4 Color : COLOR1;              // 渲染颜色 rgba
    float4 SizeRotationAgeLife : TEXCOORD1;  // 尺寸, 旋转(弧度), 年龄, 总生命(<=0 死亡)
    float4 Misc : TEXCOORD2;            // 种子, 长宽比, 角速度, 色调
    float4 Extra : TEXCOORD3;           // 基准尺寸, 拉伸模式(0/1), 保留...
};

struct VertexShaderInput
{
    float2 Corner : POSITION0;
    float2 Uv : TEXCOORD0;
    float4 PosVel : COLOR0;
    float4 Color : COLOR1;
    float4 SizeRotationAgeLife : TEXCOORD1;
    float4 Misc : TEXCOORD2;
    float4 Extra : TEXCOORD3;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 Uv : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float life = input.SizeRotationAgeLife.w;
    if (life <= 0)
    {
        // 死亡槽位: 退化四边形 (全部顶点重合于裁剪空间外).
        output.Position = float4(0, 0, -2, 0);
        output.Uv = float2(0, 0);
        output.Color = float4(0, 0, 0, 0);
        return output;
    }

    float2 position = input.PosVel.xy;
    float2 velocity = input.PosVel.zw;
    float size = input.SizeRotationAgeLife.x;
    float rotation = input.SizeRotationAgeLife.y;
    float aspect = max(input.Misc.y, 0.0001);
    float stretched = input.Extra.y;
    float2 worldPosition;

    if (stretched > 0.5)
    {
        // —— 速度拉伸: 沿速度方向展开, 长度 = 尺寸 * 长宽比 + |速度| * 系数 (封顶) ——
        float speed = length(velocity);
        float2 dir = speed > 0.0001 ? velocity / speed : float2(1, 0);
        float stretchLength = min(speed * StretchFactor, MaxStretchLength);
        float length = size * max(aspect, 1) + stretchLength;
        float thickness = size;

        float2 perp = float2(-dir.y, dir.x);
        worldPosition = position
            + dir * (input.Corner.x * length)
            + perp * (input.Corner.y * thickness);
    }
    else
    {
        // —— 普通公告牌: 按粒子自身旋转角展开 ——
        float c = cos(rotation);
        float s = sin(rotation);
        float2 scaled = float2(input.Corner.x * size * aspect, input.Corner.y * size);
        worldPosition = position + float2(scaled.x * c - scaled.y * s, scaled.x * s + scaled.y * c);
    }

    output.Position = mul(float4(worldPosition, 0, 1), Transform);
    output.Uv = input.Uv;
    output.Color = input.Color;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 texel = SpriteTexture.Sample(SpriteTextureSampler, input.Uv);
    return texel * input.Color;
}

technique ParticleRendering
{
    pass P0
    {
        VertexShader = compile vs_5_0 MainVS();
        PixelShader = compile ps_5_0 MainPS();
    }
};
