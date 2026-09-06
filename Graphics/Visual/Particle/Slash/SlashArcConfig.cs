using System.Text.Json.Serialization;

using Particle.Core;
using CurveKey = Particle.Core.CurveKey;

namespace Colin.Core.Graphics.Visual.Particle.Slash
{
  /// <summary>旧版收起方式 (已被收尾修饰器列表取代; 仅用于加载旧 JSON 时合成修饰器).</summary>
  public enum SlashRetireMode
  {
    /// <summary>渐隐.</summary>
    Fade,
    /// <summary>收拢.</summary>
    Sweep
  }

  /// <summary>
  /// 刀光本体 (弧形条带 Mesh) 配置. 一次完整的挥动明确分为两个阶段:
  /// <br>- <b>阶段一 挥动</b>: 前缘按 <see cref="SweepCurve"/> 速度曲线从 <see cref="ArcFrom"/>
  /// 扫到 <see cref="ArcTo"/>, 历时 <see cref="SweepTime"/>; 尾端钉在起始角, 弧带随挥动展开;</br>
  /// <br>- <b>阶段二 收尾</b>: <see cref="Finishes"/> 修饰器列表 (渐隐/收拢, 可同时叠加),
  /// 各自拥有独立时长与时间轴曲线; 全部播完刀光才判定完成.</br>
  /// <br>本体可叠加 <see cref="Layers"/> 纹理层 (每层独立纹理/色调/UV 变换).</br>
  /// </summary>
  [Serializable]
  public class SlashArcConfig
  {
    // ---- 几何 ----

    /// <summary>弧线半径 (像素).</summary>
    public float Radius = 110f;
    /// <summary>起始角 (度, 屏幕角: 0°=+X, Y 向下).</summary>
    public float ArcFrom = -130f;
    /// <summary>结束角 (度).</summary>
    public float ArcTo = 110f;
    /// <summary>面片全宽 (像素): 模型空间内沿弧长恒定的整体条带宽度 (椭圆长宽由相机矩阵承担).</summary>
    public float Width = 26f;
    /// <summary>弧线采样段数 (顶点密度).</summary>
    public int Segments = 48;
    /// <summary>是否反向挥扫 (从 ArcTo 扫向 ArcFrom).</summary>
    public bool Reversed = false;

    /// <summary>头部颜色 (扫动前缘).</summary>
    public Vector4 HeadColor = new Vector4(1.5f, 1.5f, 1.5f, 1f);
    /// <summary>尾部颜色 (挥扫起点一侧).</summary>
    public Vector4 TailColor = new Vector4(0.3f, 0.65f, 1.1f, 0.1f);

    /// <summary>整体坐标: 横向缩放 (X) —— 相机矩阵承担的椭圆长轴.</summary>
    public float ScaleX = 1f;
    /// <summary>整体坐标: 纵向缩放 (Y) —— 相机矩阵承担的椭圆短轴 (X≠Y 即椭圆弧).</summary>
    public float ScaleY = 1f;
    /// <summary>整体旋转 (度) —— 整条刀光绕弧心的旋转角.</summary>
    public float RotationDeg = 0f;

    // ---- 阶段一: 挥动 ----

    /// <summary>挥扫时长 (秒) —— 前缘从起始角扫到结束角.</summary>
    public float SweepTime = 0.20f;
    /// <summary>挥扫速度曲线 (横轴: 挥扫时间进度, <b>纵轴: 相对速度</b>, 1 ≈ 平均速度) ——
    /// 运行时积分归一化: 挥扫必在时长内完成, 曲线只塑造快慢节奏; 曲线水平段 = 原地停顿.</summary>
    public FloatCurve SweepCurve = FloatCurve.Constant(1f);

    // ---- 阶段二: 收尾 (修饰器, 可同时叠加) ----

    /// <summary>收尾修饰器列表: 渐隐 (SlashFadeFinish) / 收拢 (SlashCollapseFinish), 可同时叠加,
    /// 各自拥有独立时长与时间轴曲线; 全部播完刀光才判定完成.</summary>
    public List<SlashFinishConfig> Finishes = new List<SlashFinishConfig>();

    // ---- 纹理层 (可叠加) ----

    /// <summary>本体纹理层列表: 每层独立纹理/色调/UV 变换, 逐层绘制叠加;
    /// 为空时回退到旧字段 <see cref="Texture"/> 单层绘制.</summary>
    public List<SlashTextureLayer> Layers = new List<SlashTextureLayer>();

    // ---- 旧字段兼容 (新配置不再使用; 加载旧 JSON 且无修饰器/无层时兜底) ----

    /// <summary>[旧] 收起方式 —— Finishes 为空时据此合成收尾修饰器.</summary>
    public SlashRetireMode Retire = SlashRetireMode.Sweep;
    /// <summary>[旧] 渐隐时长.</summary>
    public float FadeTime = 0.28f;
    /// <summary>[旧] 收拢时长.</summary>
    public float CatchupTime = 0.25f;
    /// <summary>[旧] 单纹理名 —— Layers 为空时的兜底纹理.</summary>
    public string Texture = "blade";
    /// <summary>[旧] 未使用 (历史序列化兼容).</summary>
    public float WidthPower = 1.5f;

    /// <summary>配置版本号 (编辑后自增).</summary>
    [JsonIgnore]
    public int Version;

    /// <summary>触发配置变更通知.</summary>
    public event Action<SlashArcConfig> Changed;

    public void NotifyChanged()
    {
      Version++;
      Changed?.Invoke(this);
    }

    /// <summary>订阅配置变更.</summary>
    public void Subscribe(Action<SlashArcConfig> observer) => Changed += observer;

    /// <summary>取消订阅配置变更.</summary>
    public void Unsubscribe(Action<SlashArcConfig> observer) => Changed -= observer;

    /// <summary>深拷贝.</summary>
    public SlashArcConfig Clone() => new SlashArcConfig
    {
      Radius = Radius,
      ArcFrom = ArcFrom,
      ArcTo = ArcTo,
      Width = Width,
      Segments = Segments,
      Reversed = Reversed,
      HeadColor = HeadColor,
      TailColor = TailColor,
      ScaleX = ScaleX,
      ScaleY = ScaleY,
      RotationDeg = RotationDeg,
      SweepTime = SweepTime,
      SweepCurve = SweepCurve?.Clone() ?? FloatCurve.Constant(1f),
      Finishes = Finishes?.Select(f => f?.Clone()).ToList() ?? new List<SlashFinishConfig>(),
      Layers = Layers?.Select(l => l?.Clone()).ToList() ?? new List<SlashTextureLayer>(),
      Retire = Retire,
      FadeTime = FadeTime,
      CatchupTime = CatchupTime,
      Texture = Texture,
      WidthPower = WidthPower
    };
  }
}
