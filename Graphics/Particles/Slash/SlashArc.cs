namespace Particle.Slash
{
  /// <summary>
  /// 弧形刀光实例 (拉刀光 Mesh): 一条圆弧条带, 挥出阶段从起始角横扫到结束角
  /// (角度范围逐渐展开 —— 前缘即扫动方向), 随后整体渐隐.
  /// <br>顶点直接摆在弧线上 (RibbonBuilder), 月牙宽度 + 头亮尾隐的顶点色随构建计算.</br>
  /// </summary>
  public sealed class SlashArc
  {
    /// <summary>配置 (编辑器可直接修改并 NotifyChanged).</summary>
    public SlashArcConfig Config;

    /// <summary>世界位置 (弧心).</summary>
    public Vector2 Position;
    /// <summary>整体旋转 (弧度, 叠加在配置角度上).</summary>
    public float Rotation;
    /// <summary>整体缩放.</summary>
    public float Scale = 1f;
    /// <summary>是否播放完毕.</summary>
    public bool IsFinished;

    /// <summary>挥扫进度 (0~1, 诊断).</summary>
    public float Progress { get; private set; }

    private float _elapsed;

    // —— 网格构建缓存 (避免每帧分配) ——
    private Vector2[] _points = new Vector2[0];
    private float[] _halfWidths = new float[0];
    private Vector4[] _colors = new Vector4[0];
    private float[] _us = new float[0];

    public SlashArc(SlashArcConfig config)
    {
      Config = config;
    }

    /// <summary>重置到挥出起点.</summary>
    public void Reset()
    {
      _elapsed = 0f;
      Progress = 0f;
      IsFinished = false;
    }

    /// <summary>推进动画 (由 SlashRenderer.TickAll 或调用方驱动).</summary>
    public void Update(float dt)
    {
      if (IsFinished)
        return;
      _elapsed += dt;
      float sweep = MathF.Max(1e-4f, Config.SweepTime);
      Progress = Math.Clamp(_elapsed / sweep, 0f, 1f);
      if (_elapsed >= sweep + Config.FadeTime)
        IsFinished = true;
    }

    /// <summary>
    /// 构建当前姿态的条带网格.
    /// </summary>
    /// <returns>三角带段数 (0 = 无需绘制).</returns>
    public int Build(SlashVertex[] vertices, short[] indices)
    {
      SlashArcConfig config = Config;
      int segments = Math.Max(2, config.Segments);

      // —— 弧角范围: 挥出阶段按缓动展开; 扫完后保持全弧 ——
      float eased = EaseOutCubic(Progress);
      float fromDeg = config.ArcFrom;
      float toDeg = config.ArcTo;
      if (config.Reversed)
        (fromDeg, toDeg) = (toDeg, fromDeg);
      float currentTo = MathHelper.Lerp(fromDeg, toDeg, eased);

      float fade = 1f;
      float sweep = MathF.Max(1e-4f, config.SweepTime);
      if (_elapsed > sweep)
      {
        float fadeT = Math.Clamp((_elapsed - sweep) / MathF.Max(1e-4f, config.FadeTime), 0f, 1f);
        fade = 1f - fadeT * fadeT;   // 平方渐隐.
      }

      Vector2 center = Vector2.Zero;
      int count = segments + 1;

      // —— 逐点摆放 (i=0 为扫动前缘/头部); 缓存数组随段数扩容 ——
      if (_points.Length < count)
      {
        _points = new Vector2[count];
        _halfWidths = new float[count];
        _colors = new Vector4[count];
        _us = new float[count];
      }

      float rotation = Rotation;
      for (int i = 0; i < count; i++)
      {
        float t = i / (float)segments;              // 0 = 头部 (前缘), 1 = 尾部.
        float angleDeg = MathHelper.Lerp(currentTo, fromDeg, t);
        float angleRad = MathHelper.ToRadians(angleDeg) + rotation;
        float radius = config.Radius * Scale;

        _points[i] = center + new Vector2(MathF.Cos(angleRad), MathF.Sin(angleRad)) * radius;

        // 月牙宽度: 沿弧长两端尖、中段宽 (指数可调).
        float widthProfile = MathF.Sin(MathHelper.Pi * MathF.Pow(1f - t, config.WidthPower));
        _halfWidths[i] = config.Width * 0.5f * widthProfile * Scale;

        // 顶点色: 头部亮白 → 尾部青蓝渐隐; 整体乘挥扫后的全局渐隐.
        Vector4 color = Vector4.Lerp(config.HeadColor, config.TailColor, t);
        color.W *= fade;
        _colors[i] = color;

        _us[i] = 1f - t;   // u: 1 = 头 (纹理亮端), 0 = 尾.
      }

      return RibbonBuilder.Build(_points, _halfWidths, _colors, _us, vertices, indices);
    }

    /// <summary>缓动: 快出缓收的横扫手感.</summary>
    private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - Math.Clamp(t, 0f, 1f), 3f);
  }
}
