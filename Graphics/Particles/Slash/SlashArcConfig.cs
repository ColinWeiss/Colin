using System.Text.Json.Serialization;

namespace Particle.Slash
{
  /// <summary>刀光的收起方式.</summary>
  public enum SlashRetireMode
  {
    /// <summary>渐隐: 弧带展开到结束角后停住, 整体淡出.</summary>
    Fade,
    /// <summary>横扫: 整条弧随前缘持续旋转跟进 (尾部跟随), 弧上各点被扫过后依尾迹寿命消亡.</summary>
    Sweep
  }

  /// <summary>
  /// 可序列化的弧形刀光 (Slash Mesh) 配置: 一条圆弧形条带 Mesh,
  /// 随时间从起始角横扫到结束角 (星爆气流斩式), 挥出后整体渐隐.
  /// <br>宽度沿弧长呈月牙分布 (两端尖、中间宽), 颜色头部亮、尾部渐隐.</br>
  /// </summary>
  [Serializable]
  public class SlashArcConfig
  {
    /// <summary>弧线半径 (像素).</summary>
    public float Radius = 110f;
    /// <summary>起始角 (度, 屏幕角: 0°=+X, Y 向下).</summary>
    public float ArcFrom = -130f;
    /// <summary>结束角 (度).</summary>
    public float ArcTo = 110f;
    /// <summary>挥扫时长 (秒) —— 从起始角扫到结束角.</summary>
    public float SweepTime = 0.22f;
    /// <summary>渐隐时长 (秒) —— 扫完后整体淡出.</summary>
    public float FadeTime = 0.28f;
    /// <summary>最大全宽 (像素; 月牙中段最宽处, 两侧顶点各承担一半).</summary>
    public float Width = 26f;
    /// <summary>宽度分布指数 (≥1): 越大月牙越向中段集中.</summary>
    public float WidthPower = 1.5f;
    /// <summary>弧线采样段数 (顶点密度).</summary>
    public int Segments = 48;

    /// <summary>收起方式: Fade = 扫完后整体渐隐; Sweep = 整条弧随前缘横扫跟进, 尾部连续消亡 (星爆气流斩式).</summary>
    public SlashRetireMode Retire = SlashRetireMode.Sweep;
    /// <summary>Sweep 模式: 尾端向刃头收拢的速度 (度/秒) —— 低于前缘扫速则弧带先展开再收拢,
    /// 高于扫速则呈短彗尾跟随前缘.</summary>
    public float CatchupSpeed = 480f;

    /// <summary>头部颜色 (扫动前缘).</summary>
    public Vector4 HeadColor = new Vector4(1.5f, 1.5f, 1.5f, 1f);
    /// <summary>尾部颜色 (挥扫起点一侧).</summary>
    public Vector4 TailColor = new Vector4(0.3f, 0.65f, 1.1f, 0.1f);
    /// <summary>纹理名 (内置 blade/glow 或资产路径).</summary>
    public string Texture = "blade";
    /// <summary>是否反向挥扫 (从 ArcTo 扫向 ArcFrom).</summary>
    public bool Reversed = false;

    /// <summary>整体坐标: 横向缩放 (X) —— 拉长/压扁刀光的水平跨度.</summary>
    public float ScaleX = 1f;
    /// <summary>整体坐标: 纵向缩放 (Y) —— 拉长/压扁刀光的垂直跨度 (X≠Y 即椭圆弧).</summary>
    public float ScaleY = 1f;
    /// <summary>整体旋转 (度) —— 整条刀光绕弧心的旋转角.</summary>
    public float RotationDeg = 0f;
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
      SweepTime = SweepTime,
      FadeTime = FadeTime,
      Width = Width,
      WidthPower = WidthPower,
      Segments = Segments,
      HeadColor = HeadColor,
      TailColor = TailColor,
      Texture = Texture,
      Reversed = Reversed,
      ScaleX = ScaleX,
      ScaleY = ScaleY,
      RotationDeg = RotationDeg,
      Retire = Retire,
      CatchupSpeed = CatchupSpeed
    };
  }
}
