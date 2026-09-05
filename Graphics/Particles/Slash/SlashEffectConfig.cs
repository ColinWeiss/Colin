using System.Text.Json.Serialization;

namespace Particle.Slash
{
  /// <summary>
  /// 刃花配置: 沿刀光前缘喷射的火花粒子层.
  /// <br>刃花与刀光角度<b>绑定</b> —— 运行时由 <see cref="SlashEffect"/> 每帧取弧的当前前缘角度,
  /// 用与 Mesh 相同的整体坐标系 (椭圆缩放/旋转) 求前缘位置与切向, 直接驱动发射,
  /// 因此无论收起方式、扫速、坐标系怎么改, 刃花始终生在刃头上.</br>
  /// </summary>
  [Serializable]
  public class SlashSparkConfig
  {
    /// <summary>是否启用刃花层.</summary>
    public bool Enabled = true;
    /// <summary>发射率 (个/秒, 沿前缘).</summary>
    public float Rate = 220f;
    /// <summary>初速下限 (像素/秒, 沿前缘切向).</summary>
    public float SpeedMin = 450f;
    /// <summary>初速上限 (像素/秒).</summary>
    public float SpeedMax = 950f;
    /// <summary>速度散布半角 (度) —— 偏离切向的随机张角.</summary>
    public float SpreadDeg = 14f;
    /// <summary>生命下限 (秒).</summary>
    public float LifeMin = 0.06f;
    /// <summary>生命上限 (秒).</summary>
    public float LifeMax = 0.16f;
    /// <summary>尺寸下限 (像素).</summary>
    public float SizeMin = 1.6f;
    /// <summary>尺寸上限 (像素).</summary>
    public float SizeMax = 3f;
    /// <summary>长宽比.</summary>
    public float Aspect = 1.6f;
    /// <summary>重力 (像素/秒², +Y 向下).</summary>
    public Vector2 Gravity = new Vector2(0f, 140f);
    /// <summary>线性阻尼 (1/秒).</summary>
    public float Drag = 1.6f;
    /// <summary>出生颜色 (白热).</summary>
    public Vector4 StartColor = new Vector4(1.2f, 1.2f, 1.0f, 1f);
    /// <summary>消亡颜色 (冷却偏蓝).</summary>
    public Vector4 EndColor = new Vector4(0.4f, 0.65f, 1.2f, 1f);
    /// <summary>是否随速度拉伸 (高速火花条).</summary>
    public bool Stretched = true;
    /// <summary>火花槽位上限.</summary>
    public int Capacity = 128;
    /// <summary>刃花纹理名 (内置 spark/glow/blade 或 "file:路径").</summary>
    public string Texture = "spark";

    /// <summary>深拷贝.</summary>
    public SlashSparkConfig Clone() => new SlashSparkConfig
    {
      Enabled = Enabled,
      Rate = Rate,
      SpeedMin = SpeedMin,
      SpeedMax = SpeedMax,
      SpreadDeg = SpreadDeg,
      LifeMin = LifeMin,
      LifeMax = LifeMax,
      SizeMin = SizeMin,
      SizeMax = SizeMax,
      Aspect = Aspect,
      Gravity = Gravity,
      Drag = Drag,
      StartColor = StartColor,
      EndColor = EndColor,
      Stretched = Stretched,
      Capacity = Capacity,
      Texture = Texture
    };
  }

  /// <summary>
  /// 刀光效果的完整可序列化配置 (独立大功能, 不依附粒子发射器):
  /// <see cref="Arc"/> 为拉刀光 Mesh 本体, <see cref="Sparks"/> 为与前缘角度绑定的刃花粒子层.
  /// </summary>
  [Serializable]
  public class SlashEffectConfig
  {
    /// <summary>名称 (编辑器/日志显示).</summary>
    public string Name = "刀光";

    /// <summary>刀光 Mesh 本体 (弧形条带) 参数.</summary>
    public SlashArcConfig Arc = new SlashArcConfig();

    /// <summary>刃花层 (沿前缘喷射) 参数.</summary>
    public SlashSparkConfig Sparks = new SlashSparkConfig();

    /// <summary>是否循环挥砍 (游戏内持续技/编辑器预览).</summary>
    public bool Looping = false;

    /// <summary>循环模式: 两次挥砍之间的休止时长 (秒).</summary>
    public float RestTime = 0.35f;

    /// <summary>固定随机种子 (0 = 每次随机).</summary>
    public int Seed = 0;

    /// <summary>配置版本号 (编辑后自增).</summary>
    [JsonIgnore]
    public int Version;

    /// <summary>触发配置变更通知.</summary>
    public event Action<SlashEffectConfig> Changed;

    public void NotifyChanged()
    {
      Version++;
      Changed?.Invoke(this);
    }

    /// <summary>订阅配置变更 (观察者模式).</summary>
    public void Subscribe(Action<SlashEffectConfig> observer) => Changed += observer;

    /// <summary>取消订阅配置变更.</summary>
    public void Unsubscribe(Action<SlashEffectConfig> observer) => Changed -= observer;

    /// <summary>深拷贝.</summary>
    public SlashEffectConfig Clone() => new SlashEffectConfig
    {
      Name = Name,
      Arc = Arc?.Clone() ?? new SlashArcConfig(),
      Sparks = Sparks?.Clone() ?? new SlashSparkConfig(),
      Looping = Looping,
      RestTime = RestTime,
      Seed = Seed
    };
  }
}
