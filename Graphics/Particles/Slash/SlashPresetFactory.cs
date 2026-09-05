namespace Particle.Slash
{
  /// <summary>
  /// 刀光预设工厂 (工厂模式): 按名称创建 <see cref="SlashEffectConfig"/>.
  /// <br>刀光是独立大功能 (不依附粒子发射器), 预设与粒子预设 (ParticlePresetFactory) 分离.</br>
  /// </summary>
  public static class SlashPresetFactory
  {
    private static readonly Dictionary<string, Func<SlashEffectConfig>> Factories = new Dictionary<string, Func<SlashEffectConfig>>
    {
      ["标准斩"] = CreateStandard,
      ["快速斩"] = CreateQuick
    };

    /// <summary>全部预设名称 (有序).</summary>
    public static IReadOnlyList<string> PresetNames => Factories.Keys.ToList();

    /// <summary>注册自定义刀光预设 (同名覆盖).</summary>
    public static void Register(string name, Func<SlashEffectConfig> factory) => Factories[name] = factory;

    /// <summary>按名称创建预设配置; 每次调用返回全新配置 (可自由修改/保存).</summary>
    public static SlashEffectConfig Create(string presetName)
    {
      if (Factories.TryGetValue(presetName, out Func<SlashEffectConfig> factory))
        return factory();
      throw new KeyNotFoundException($"未注册的刀光预设: {presetName}");
    }

    /// <summary>标准斩: 中速横扫, 弧带先展开再向刃头收拢, 刃花沿前缘喷射.</summary>
    public static SlashEffectConfig CreateStandard() => new SlashEffectConfig
    {
      Name = "标准斩",
      Arc = new SlashArcConfig
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
      },
      Sparks = new SlashSparkConfig()
    };

    /// <summary>快速斩: 高速窄弧短横扫, 扫速远高于收拢速度, 弧带充分展开后迅速收拢.</summary>
    public static SlashEffectConfig CreateQuick() => new SlashEffectConfig
    {
      Name = "快速斩",
      Arc = new SlashArcConfig
      {
        Radius = 96f,
        ArcFrom = -150f,
        ArcTo = 130f,
        SweepTime = 0.12f,
        FadeTime = 0.18f,
        Width = 22f,
        Segments = 48,
        HeadColor = new Vector4(1.7f, 1.5f, 1.2f, 1f),
        TailColor = new Vector4(1.0f, 0.45f, 0.25f, 0.08f),
        Retire = SlashRetireMode.Sweep,
        CatchupSpeed = 900f,
        Texture = "blade"
      },
      Sparks = new SlashSparkConfig
      {
        Rate = 320f,
        SpeedMin = 520f,
        SpeedMax = 1150f,
        StartColor = new Vector4(1.3f, 1.15f, 0.9f, 1f),
        EndColor = new Vector4(1.0f, 0.5f, 0.25f, 1f)
      }
    };
  }
}
