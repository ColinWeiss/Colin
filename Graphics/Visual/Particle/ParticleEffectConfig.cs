using System.Text.Json.Serialization;

namespace Colin.Core.Graphics.Visual.Particle
{
  /// <summary>混合模式.</summary>
  public enum ParticleBlendMode
  {
    /// <summary>加法混合 —— 刀光、火花、爆炸闪光等发光效果首选.</summary>
    Additive,
    /// <summary>预乘 Alpha 混合 —— 烟雾、碎片等常规效果.</summary>
    AlphaBlend,
    /// <summary>非预乘 Alpha 混合.</summary>
    NonPremultiplied,
    /// <summary>不混合 (覆盖).</summary>
    Opaque
  }

  /// <summary>
  /// 渲染配置: 混合模式、纹理与速度拉伸参数.
  /// </summary>
  [Serializable]
  public class RenderConfig
  {
    /// <summary>混合模式.</summary>
    public ParticleBlendMode Blend = ParticleBlendMode.Additive;

    /// <summary>纹理名称: 程序化内置纹理 ("blade"/"glow"/"spark"/"smoke") 或资产路径 (经 Leemo.Assets 管线加载).</summary>
    public string Texture = "blade";

    /// <summary>速度拉伸系数 (秒): 拉伸长度增量 = |速度| × 该值; 0.02~0.08 适合刀光.</summary>
    public float StretchFactor = 0.045f;

    /// <summary>速度拉伸最大长度 (像素), 防止高速粒子拉出超长条.</summary>
    public float MaxStretchLength = 220f;

    /// <summary>深拷贝.</summary>
    public RenderConfig Clone() => new RenderConfig
    {
      Blend = Blend,
      Texture = Texture,
      StretchFactor = StretchFactor,
      MaxStretchLength = MaxStretchLength
    };
  }

  /// <summary>
  /// 完整粒子效果的可序列化配置: 由多个发射器 (组合模式) 与一份渲染配置构成.
  /// <br>修改任一成员后调用 <see cref="NotifyChanged"/> 以通知运行时 (观察者模式).</br>
  /// </summary>
  [Serializable]
  public class ParticleEffectConfig
  {
    /// <summary>效果名称.</summary>
    public string Name = "未命名效果";

    /// <summary>效果时长 (秒); 持续发射速率曲线以此为横轴归一化.</summary>
    public float Duration = 1f;

    /// <summary>是否循环播放.</summary>
    public bool Looping = false;

    /// <summary>效果级随机种子 (0 = 随机; Reset 时使用).</summary>
    public int Seed = 0;

    /// <summary>发射器列表 (组合模式: 一个效果由多个发射器共同构成).</summary>
    public List<EmitterConfig> Emitters = new List<EmitterConfig>();

    /// <summary>渲染配置.</summary>
    public RenderConfig Render = new RenderConfig();

    /// <summary>配置版本号.</summary>
    [JsonIgnore]
    public int Version;

    /// <summary>配置整体变更事件 (观察者模式通知入口).</summary>
    public event Action<ParticleEffectConfig> Changed;

    /// <summary>触发整体配置变更通知 (会级联刷新版本号).</summary>
    public void NotifyChanged()
    {
      Version++;
      Changed?.Invoke(this);
    }

    /// <summary>订阅配置变更.</summary>
    public void Subscribe(Action<ParticleEffectConfig> observer) => Changed += observer;

    /// <summary>取消订阅配置变更.</summary>
    public void Unsubscribe(Action<ParticleEffectConfig> observer) => Changed -= observer;

    /// <summary>计算效果总槽位 (各发射器容量之和, 决定 GPU 缓冲区大小).</summary>
    public int TotalCapacity()
    {
      int total = 0;
      for (int i = 0; i < Emitters.Count; i++)
        total += Math.Max(1, Emitters[i].Capacity);
      return total;
    }

    /// <summary>深拷贝整份效果配置.</summary>
    public ParticleEffectConfig Clone() => new ParticleEffectConfig
    {
      Name = Name,
      Duration = Duration,
      Looping = Looping,
      Seed = Seed,
      Emitters = Emitters?.Select(e => e.Clone()).ToList() ?? new List<EmitterConfig>(),
      Render = Render?.Clone() ?? new RenderConfig()
    };
  }
}
