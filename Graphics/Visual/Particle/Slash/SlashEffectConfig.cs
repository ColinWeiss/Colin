using System.Text.Json.Serialization;

using Particle.Core;
using CurveKey = Particle.Core.CurveKey;

namespace Colin.Core.Graphics.Visual.Particle.Slash
{
  // =====================================================================
  //  收尾修饰器 (阶段二): 挥动结束后的收尾表现, 一份配置可同时叠加多个.
  //  每个修饰器有独立的时长与时间轴曲线; 全部修饰器播完, 刀光才判定完成.
  // =====================================================================

  /// <summary>收尾修饰器类型标识 (诊断用).</summary>
  public enum SlashFinishKind
  {
    /// <summary>渐隐: 整体透明度随曲线衰减.</summary>
    Fade,
    /// <summary>收拢: 尾端从起始角向刃头收进 (星爆气流斩式).</summary>
    Collapse
  }

  /// <summary>
  /// 收尾修饰器基类: 挥动 (阶段一) 结束后驱动收尾 (阶段二) 的可叠加修饰器.
  /// <br>派生: <see cref="SlashFadeFinish"/> (渐隐) / <see cref="SlashCollapseFinish"/> (收拢) ——
  /// 二者同级、允许同时添加 (边收拢边渐隐). 多态序列化经 "$kind" 判别符.</br>
  /// </summary>
  [JsonPolymorphic(TypeDiscriminatorPropertyName = "$kind")]
  [JsonDerivedType(typeof(SlashFadeFinish), "fade")]
  [JsonDerivedType(typeof(SlashCollapseFinish), "collapse")]
  [Serializable]
  public abstract class SlashFinishConfig
  {
    /// <summary>是否启用.</summary>
    public bool Enabled = true;

    /// <summary>收尾时长 (秒) —— 该修饰器自己的时间轴长度.</summary>
    public float Duration = 0.25f;

    /// <summary>收尾时间轴曲线 (横轴: 收尾时间进度, <b>纵轴: 相对速度</b>) ——
    /// 积分归一化: 时长内必完成, 曲线只塑造快慢节奏 (收拢速度 / 渐隐速度).</summary>
    public FloatCurve Curve = FloatCurve.Constant(1f);

    /// <summary>修饰器类型 (诊断).</summary>
    public abstract SlashFinishKind Kind { get; }

    /// <summary>深拷贝.</summary>
    public abstract SlashFinishConfig Clone();
  }

  /// <summary>渐隐收尾: 整体透明度 × (1 - 曲线(t)). 曲线决定渐隐节奏 (线性淡出 / 先驻留后消失等).</summary>
  [Serializable]
  public sealed class SlashFadeFinish : SlashFinishConfig
  {
    public override SlashFinishKind Kind => SlashFinishKind.Fade;

    public override SlashFinishConfig Clone() => new SlashFadeFinish
    {
      Enabled = Enabled,
      Duration = Duration,
      Curve = Curve?.Clone() ?? FloatCurve.Constant(1f)
    };
  }

  /// <summary>收拢收尾: 尾端角 = 起始角 → (起始角 + 曲线(t) × 全弧) 向刃头收进.
  /// 时长到点收拢必然完成 —— 不存在没收入的顶点 (容灾).</summary>
  [Serializable]
  public sealed class SlashCollapseFinish : SlashFinishConfig
  {
    public override SlashFinishKind Kind => SlashFinishKind.Collapse;

    public override SlashFinishConfig Clone() => new SlashCollapseFinish
    {
      Enabled = Enabled,
      Duration = Duration,
      Curve = Curve?.Clone() ?? FloatCurve.Constant(1f)
    };
  }

  // =====================================================================
  //  纹理层修饰器: 刀光本体的贴图可叠加任意多层, 每层独立纹理与参数,
  //  几何每帧只构建一次, 逐层绘制 (各自采样/色调) —— 叠出更丰富的视觉表现.
  // =====================================================================

  /// <summary>
  /// 刀光纹理层: 一层独立的贴图 pass. UV.x 沿刀光方向 (尾 0 → 头 1), UV.y 沿宽度截面.
  /// </summary>
  [Serializable]
  public class SlashTextureLayer
  {
    /// <summary>是否启用.</summary>
    public bool Enabled = true;

    /// <summary>纹理名: 内置 ("blade"/"glow"/"spark"/"smoke") / "file:路径" / "embed:键" (预制件自带).</summary>
    public string Texture = "blade";

    /// <summary>层颜色调制 (与顶点头尾渐变色相乘后再乘此色).</summary>
    public Vector4 Tint = new Vector4(1f, 1f, 1f, 1f);

    /// <summary>层强度 (整体乘算, 可叠出主次层级).</summary>
    public float Intensity = 1f;

    /// <summary>UV 横向平铺次数 (1 = 整张映射; &gt;1 沿刀光重复).</summary>
    public float UTiling = 1f;

    /// <summary>UV 横向偏移 (静态; 配合滚动做相位).</summary>
    public float UOffset = 0f;

    /// <summary>横向滚动速度 (u/秒) —— 流光沿刀光方向持续流动.</summary>
    public float ScrollSpeed = 0f;

    /// <summary>UV 纵向缩放 (截面收窄 &lt;1 / 扩张 &gt;1, 以中线为中心).</summary>
    public float VScale = 1f;

    /// <summary>深拷贝.</summary>
    public SlashTextureLayer Clone() => new SlashTextureLayer
    {
      Enabled = Enabled,
      Texture = Texture,
      Tint = Tint,
      Intensity = Intensity,
      UTiling = UTiling,
      UOffset = UOffset,
      ScrollSpeed = ScrollSpeed,
      VScale = VScale
    };
  }

  /// <summary>
  /// 刃花配置: 沿刀光前缘喷射的火花粒子层.
  /// <br>刃花与刀光角度<b>绑定</b> —— 运行时由 <see cref="SlashEffect"/> 每帧取弧的当前前缘角度,
  /// 用与 Mesh 相同的整体坐标系 (椭圆缩放/旋转) 求前缘位置与切向, 直接驱动发射,
  /// 因此无论收尾方式、扫速、坐标系怎么改, 刃花始终生在刃头上.</br>
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
    /// <summary>刃花纹理名 (内置 spark/glow 或 "file:路径" / "embed:键").</summary>
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
  /// 刀光预制件配置 (独立大功能, 自包含): 
  /// <br>- 阶段一 <b>挥动</b>: 前缘按 <see cref="SlashArcConfig.SweepCurve"/> 从起始角扫到结束角;</br>
  /// <br>- 阶段二 <b>收尾</b>: <see cref="SlashArcConfig.Finishes"/> 修饰器列表 (渐隐/收拢, 可同时叠加);</br>
  /// <br>- 本体贴图: <see cref="SlashArcConfig.Layers"/> 纹理层列表 (可叠加多层);</br>
  /// <br>- <see cref="EmbeddedTextures"/>: 预制件自带的贴图数据 (PNG base64) —— 层纹理名写 "embed:键"
  /// 即从预制件内部取图, 不依赖任何磁盘路径, 玩家机器上开箱即用.</br>
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

    /// <summary>嵌入纹理表 (键 → PNG base64) —— 预制件自带贴图, "embed:键" 由此解析.</summary>
    public Dictionary<string, string> EmbeddedTextures = new Dictionary<string, string>();

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
      EmbeddedTextures = EmbeddedTextures is null
        ? new Dictionary<string, string>()
        : new Dictionary<string, string>(EmbeddedTextures),
      Looping = Looping,
      RestTime = RestTime,
      Seed = Seed
    };
  }
}
