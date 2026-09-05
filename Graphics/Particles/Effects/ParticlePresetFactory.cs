using Particle.Core;
using Particle.Slash;
using CurveKey = Particle.Core.CurveKey;
using XnaVector4 = Microsoft.Xna.Framework.Vector4;

namespace Particle.Effects
{
  /// <summary>
  /// 粒子预设工厂 (工厂模式): 按名称创建效果配置的工厂注册表.
  /// <br>内置预设: 刀光 / 爆炸 / 火焰 / 烟雾; 可通过 <see cref="Register"/> 扩展自定义预设.</br>
  /// </summary>
  public static class ParticlePresetFactory
  {
    private static readonly Dictionary<string, Func<ParticleEffectConfig>> Factories = new Dictionary<string, Func<ParticleEffectConfig>>
    {
      ["刀光"] = CreateBladeSlash,
      ["爆炸"] = CreateExplosion,
      ["火焰"] = CreateFire,
      ["烟雾"] = CreateSmoke
    };

    /// <summary>刀光 Mesh (弧形条带) 的默认配置 —— 刀光本体的拉刀光网格参数.</summary>
    public static SlashArcConfig CreateBladeSlashArc() => new SlashArcConfig
    {
      Radius = 120f,
      ArcFrom = -125f,
      ArcTo = 115f,
      SweepTime = 0.20f,
      FadeTime = 0.30f,
      Width = 30f,
      WidthPower = 1.4f,
      Segments = 56,
      HeadColor = new Vector4(1.6f, 1.6f, 1.6f, 1f),
      TailColor = new Vector4(0.25f, 0.55f, 1.2f, 0.06f),
      Retire = SlashRetireMode.Sweep,
      CatchupSpeed = 500f,
      Texture = "blade"
    };

    /// <summary>刀光 Mesh 发射器 (统一进发射器体系: Kind == SlashArc, 粒子层参数不参与模拟).</summary>
    public static EmitterConfig CreateSlashArcEmitter(string name = "刀光本体") => new EmitterConfig
    {
      Name = name,
      Kind = EmitterKind.SlashArc,
      Slash = CreateBladeSlashArc(),
      Capacity = 1,                // 刀光发射器不占用粒子槽位 (仅保留 1 列占位).
      StartTime = 0f
    };

    /// <summary>游戏内剑光轨迹 (SlashTrail) 的默认配置.</summary>
    public static SlashTrail CreateBladeTrail()
    {
      return new SlashTrail
      {
        TrailTime = 0.15f,
        Width = 34f,
        HeadColor = new Vector4(1.5f, 1.5f, 1.5f, 1f),
        TailColor = new Vector4(0.3f, 0.55f, 1.1f, 0f),
        Texture = "blade",
        MinPointDistance = 1.2f
      };
    }

    /// <summary>全部预设名称 (有序).</summary>
    public static IReadOnlyList<string> PresetNames => Factories.Keys.ToList();

    /// <summary>注册自定义预设工厂 (同名覆盖).</summary>
    public static void Register(string name, Func<ParticleEffectConfig> factory) => Factories[name] = factory;

    /// <summary>按名称创建预设配置; 每次调用返回全新配置 (可自由修改/保存).</summary>
    public static ParticleEffectConfig Create(string presetName)
    {
      if (Factories.TryGetValue(presetName, out Func<ParticleEffectConfig> factory))
        return factory();
      throw new KeyNotFoundException($"未注册的粒子预设: {presetName}");
    }

    // =====================================================================
    //  刀光 (重点预设)
    //  形状: 扇形弧线切向发射; 渲染: 速度拉伸公告牌 + 加法混合 + 梭形渐变纹理;
    //  参数: 生命极短 (0.05~0.12 秒), 初速高 (900~1300 像素/秒), 颜色高亮, 快速衰减.
    //  编辑器内单独播放时呈弧形扇面; 游戏内由 BladeTrailAPI 沿剑刃轨迹驱动.
    // =====================================================================
    private static ParticleEffectConfig CreateBladeSlash()
    {
      // —— 辉光层: 沿横扫前缘的柔光大粒子, 给光刃提供体积感 ——
      EmitterConfig glow = new EmitterConfig
      {
        Name = "刃辉",
        Capacity = 128,
        EmissionRate = 260f,
        RateCurve = new FloatCurve
        {
          Keys = { new CurveKey(0f, 0.8f), new CurveKey(0.3f, 1f), new CurveKey(0.85f, 0.6f), new CurveKey(1f, 0.2f) }
        },
        Shape = new EmissionShapeConfig
        {
          Shape = EmissionShapeType.Arc,
          Radius = 118f,
          ArcStart = -122f,
          ArcAngle = 16f,             // 前缘角度窗口 (跟随横扫).
          Velocity = VelocityMode.Tangent,
          SweepSpeed = 1200f,         // 与 Mesh 横扫同步: (ArcTo-ArcFrom)/SweepTime ≈ 1075°/s.
          Jitter = 5f
        },
        SpeedMin = 40f,
        SpeedMax = 110f,
        LifeMin = 0.14f,
        LifeMax = 0.26f,
        SizeMin = 16f,
        SizeMax = 26f,
        SizeOverLife = new FloatCurve
        {
          Keys = { new CurveKey(0f, 0.7f), new CurveKey(0.35f, 1f), new CurveKey(1f, 1.5f) }
        },
        ColorOverLife = new ColorCurve
        {
          Keys =
          {
            new ColorKey(0f,   new Vector4(1.00f, 1.00f, 1.00f, 1f)),
            new ColorKey(0.4f, new Vector4(0.55f, 0.85f, 1.35f, 1f)),
            new ColorKey(1f,   new Vector4(0.20f, 0.35f, 0.90f, 1f))
          }
        },
        OpacityOverLife = new FloatCurve
        {
          Keys = { new CurveKey(0f, 0.85f), new CurveKey(0.5f, 0.55f), new CurveKey(1f, 0f) }
        },
        TintMin = 0.85f,
        TintMax = 1.1f,
        Drag = 2.5f,
        StretchedBillboard = false,
        Seed = 0
      };

      // —— 火花层: 沿弧散落的细小拖尾, 增强速度感 ——
      EmitterConfig sparks = new EmitterConfig
      {
        Name = "刃花",
        Capacity = 128,
        EmissionRate = 220f,
        Shape = new EmissionShapeConfig
        {
          Shape = EmissionShapeType.Arc,
          Radius = 120f,
          ArcStart = -125f,
          ArcAngle = 14f,
          Velocity = VelocityMode.Tangent,
          SweepSpeed = 1200f,
          Jitter = 10f
        },
        SpeedMin = 450f,
        SpeedMax = 950f,
        LifeMin = 0.06f,
        LifeMax = 0.16f,
        SizeMin = 1.6f,
        SizeMax = 3f,
        Aspect = 1.6f,
        ColorOverLife = new ColorCurve
        {
          Keys =
          {
            new ColorKey(0f, new Vector4(1.2f, 1.2f, 1.0f, 1f)),
            new ColorKey(1f, new Vector4(0.4f, 0.65f, 1.2f, 1f))
          }
        },
        OpacityOverLife = new FloatCurve
        {
          Keys = { new CurveKey(0f, 1f), new CurveKey(1f, 0f) }
        },
        TintMin = 0.8f,
        TintMax = 1.2f,
        Gravity = new Vector2(0f, 140f),
        Drag = 1.6f,
        StretchedBillboard = true,
        Seed = 0
      };

      return new ParticleEffectConfig
      {
        Name = "刀光",
        Duration = 0.26f,
        Looping = false,
        Emitters = { glow, sparks, CreateSlashArcEmitter() },
        Render = new RenderConfig
        {
          Blend = ParticleBlendMode.Additive,
          Texture = "glow",
          StretchFactor = 0.05f,
          MaxStretchLength = 200f
        }
      };
    }

    // =====================================================================
    //  爆炸: 中心闪光 + 径向火花 + 烟团, 单次突发事件.
    // =====================================================================
    private static ParticleEffectConfig CreateExplosion()
    {
      EmitterConfig flash = new EmitterConfig
      {
        Name = "闪光",
        Capacity = 64,
        EmissionRate = 0f,
        Bursts = { new BurstEvent(0f, 10) },
        Shape = new EmissionShapeConfig { Shape = EmissionShapeType.Point },
        SpeedMin = 60f,
        SpeedMax = 160f,
        LifeMin = 0.12f,
        LifeMax = 0.22f,
        SizeMin = 22f,
        SizeMax = 34f,
        ColorOverLife = new ColorCurve
        {
          Keys =
          {
            new ColorKey(0f, new Vector4(1f, 1f, 0.85f, 1f)),
            new ColorKey(0.4f, new Vector4(1f, 0.7f, 0.3f, 1f)),
            new ColorKey(1f, new Vector4(0.6f, 0.25f, 0.1f, 1f))
          }
        },
        OpacityOverLife = new FloatCurve { Keys = { new CurveKey(0f, 1f), new CurveKey(1f, 0f) } },
        SizeOverLife = new FloatCurve { Keys = { new CurveKey(0f, 1f), new CurveKey(1f, 1.8f) } },
        Drag = 4f,
        StretchedBillboard = false,
        Seed = 0
      };

      EmitterConfig debris = new EmitterConfig
      {
        Name = "火花",
        Capacity = 256,
        EmissionRate = 0f,
        Bursts = { new BurstEvent(0f, 48) },
        Shape = new EmissionShapeConfig
        {
          Shape = EmissionShapeType.Circle,
          Radius = 4f,
          EdgeOnly = false,
          Velocity = VelocityMode.Radial
        },
        SpeedMin = 320f,
        SpeedMax = 780f,
        LifeMin = 0.3f,
        LifeMax = 0.7f,
        SizeMin = 2f,
        SizeMax = 4f,
        ColorOverLife = new ColorCurve
        {
          Keys =
          {
            new ColorKey(0f, new Vector4(1f, 0.95f, 0.7f, 1f)),
            new ColorKey(0.5f, new Vector4(1f, 0.55f, 0.2f, 1f)),
            new ColorKey(1f, new Vector4(0.4f, 0.15f, 0.05f, 1f))
          }
        },
        OpacityOverLife = new FloatCurve { Keys = { new CurveKey(0f, 1f), new CurveKey(0.7f, 0.8f), new CurveKey(1f, 0f) } },
        Gravity = new Vector2(0f, 620f),
        Drag = 1.4f,
        StretchedBillboard = true,
        Seed = 0
      };

      EmitterConfig smoke = new EmitterConfig
      {
        Name = "烟团",
        Capacity = 96,
        EmissionRate = 0f,
        Bursts = { new BurstEvent(0.04f, 14) },
        Shape = new EmissionShapeConfig
        {
          Shape = EmissionShapeType.Circle,
          Radius = 8f,
          EdgeOnly = false,
          Velocity = VelocityMode.Radial
        },
        SpeedMin = 40f,
        SpeedMax = 140f,
        LifeMin = 0.7f,
        LifeMax = 1.4f,
        SizeMin = 14f,
        SizeMax = 26f,
        SizeOverLife = new FloatCurve { Keys = { new CurveKey(0f, 0.6f), new CurveKey(1f, 1.6f) } },
        ColorOverLife = new ColorCurve
        {
          Keys =
          {
            new ColorKey(0f, new Vector4(0.45f, 0.42f, 0.4f, 1f)),
            new ColorKey(1f, new Vector4(0.25f, 0.24f, 0.25f, 1f))
          }
        },
        OpacityOverLife = new FloatCurve { Keys = { new CurveKey(0f, 0.9f), new CurveKey(0.6f, 0.5f), new CurveKey(1f, 0f) } },
        TintMin = 0.8f,
        TintMax = 1.1f,
        Drag = 1.8f,
        StretchedBillboard = false,
        Seed = 0
      };

      return new ParticleEffectConfig
      {
        Name = "爆炸",
        Duration = 0.6f,
        Looping = false,
        Emitters = { flash, debris, smoke },
        Render = new RenderConfig
        {
          Blend = ParticleBlendMode.Additive,
          Texture = "glow",
          StretchFactor = 0.03f,
          MaxStretchLength = 120f
        }
      };
    }

    // =====================================================================
    //  火焰: 循环上升火舌, 颜色由白热向暗红过渡, 尺寸随生命膨胀.
    // =====================================================================
    private static ParticleEffectConfig CreateFire()
    {
      EmitterConfig flame = new EmitterConfig
      {
        Name = "火舌",
        Capacity = 256,
        EmissionRate = 150f,
        Shape = new EmissionShapeConfig
        {
          Shape = EmissionShapeType.Segment,
          SegmentFrom = new Vector2(-14f, 0f),
          SegmentTo = new Vector2(14f, 0f),
          Velocity = VelocityMode.Direction,
          DirectionBase = -90f,
          DirectionSpread = 22f,
          Jitter = 3f
        },
        SpeedMin = 60f,
        SpeedMax = 130f,
        LifeMin = 0.4f,
        LifeMax = 0.8f,
        SizeMin = 7f,
        SizeMax = 13f,
        SizeOverLife = new FloatCurve { Keys = { new CurveKey(0f, 0.55f), new CurveKey(0.35f, 1f), new CurveKey(1f, 1.5f) } },
        ColorOverLife = new ColorCurve
        {
          Keys =
          {
            new ColorKey(0f,    new Vector4(1.00f, 0.98f, 0.82f, 1f)),
            new ColorKey(0.25f, new Vector4(1.00f, 0.75f, 0.28f, 1f)),
            new ColorKey(0.60f, new Vector4(0.90f, 0.35f, 0.10f, 1f)),
            new ColorKey(1f,    new Vector4(0.35f, 0.08f, 0.05f, 1f))
          }
        },
        OpacityOverLife = new FloatCurve { Keys = { new CurveKey(0f, 0.9f), new CurveKey(0.5f, 0.75f), new CurveKey(1f, 0f) } },
        TintMin = 0.85f,
        TintMax = 1.1f,
        Gravity = new Vector2(0f, -70f),
        Drag = 0.9f,
        StretchedBillboard = false,
        Seed = 0
      };

      return new ParticleEffectConfig
      {
        Name = "火焰",
        Duration = 1f,
        Looping = true,
        Emitters = { flame },
        Render = new RenderConfig
        {
          Blend = ParticleBlendMode.Additive,
          Texture = "glow",
          StretchFactor = 0.02f,
          MaxStretchLength = 60f
        }
      };
    }

    // =====================================================================
    //  烟雾: 循环上升灰烟, Alpha 混合, 先入后出.
    // =====================================================================
    private static ParticleEffectConfig CreateSmoke()
    {
      EmitterConfig puff = new EmitterConfig
      {
        Name = "烟团",
        Capacity = 128,
        EmissionRate = 36f,
        Shape = new EmissionShapeConfig
        {
          Shape = EmissionShapeType.Segment,
          SegmentFrom = new Vector2(-10f, 0f),
          SegmentTo = new Vector2(10f, 0f),
          Velocity = VelocityMode.Direction,
          DirectionBase = -90f,
          DirectionSpread = 18f,
          Jitter = 5f
        },
        SpeedMin = 18f,
        SpeedMax = 42f,
        LifeMin = 1.2f,
        LifeMax = 2.2f,
        SizeMin = 12f,
        SizeMax = 22f,
        SizeOverLife = new FloatCurve { Keys = { new CurveKey(0f, 0.5f), new CurveKey(1f, 1.7f) } },
        ColorOverLife = new ColorCurve
        {
          Keys =
          {
            new ColorKey(0f, new Vector4(0.62f, 0.62f, 0.64f, 1f)),
            new ColorKey(1f, new Vector4(0.35f, 0.35f, 0.38f, 1f))
          }
        },
        OpacityOverLife = new FloatCurve
        {
          Keys = { new CurveKey(0f, 0f), new CurveKey(0.2f, 0.42f), new CurveKey(0.7f, 0.3f), new CurveKey(1f, 0f) }
        },
        TintMin = 0.85f,
        TintMax = 1.1f,
        Gravity = new Vector2(6f, -26f),
        Drag = 0.6f,
        AngularVelocityMin = -30f,
        AngularVelocityMax = 30f,
        StretchedBillboard = false,
        Seed = 0
      };

      return new ParticleEffectConfig
      {
        Name = "烟雾",
        Duration = 1f,
        Looping = true,
        Emitters = { puff },
        Render = new RenderConfig
        {
          Blend = ParticleBlendMode.AlphaBlend,
          Texture = "smoke",
          StretchFactor = 0.02f,
          MaxStretchLength = 40f
        }
      };
    }
  }
}
