namespace Particle.Slash
{
  /// <summary>
  /// 弧形刀光实例 (拉刀光 Mesh): 等宽弯曲长方形条带, 顶点直接摆在弧线上.
  /// <br>两种收起方式 (<see cref="SlashRetireMode"/>):</br>
  /// <br>- <b>Fade</b>: 弧带自起始角展开至结束角, 停住后整体淡出;</br>
  /// <br>- <b>Sweep</b>: 前缘横扫, 尾端以可调的收拢速度持续<b>向刃头收去</b> ——
  /// 弧带逐渐收拢、最终在刃头处消散 (星爆气流斩式).</br>
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
    private float _headDeg;
    private float _tailDeg;
    private bool _tailInitialized;

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
      _tailInitialized = false;
    }

    /// <summary>推进动画 (由 SlashRenderer.TickAll 或调用方驱动). 尾端收拢为状态积分, 只在此处推进.</summary>
    public void Update(float dt)
    {
      if (IsFinished)
        return;

      SlashArcConfig config = Config;
      float fromDeg = config.ArcFrom;
      float toDeg = config.ArcTo;
      if (config.Reversed)
        (fromDeg, toDeg) = (toDeg, fromDeg);

      float sweep = MathF.Max(1e-4f, config.SweepTime);
      _elapsed += dt;
      Progress = Math.Clamp(_elapsed / sweep, 0f, 1f);

      // —— 前缘: 解析推进 (Sweep 匀速 / Fade 缓动) ——
      float headPrev = _headDeg;
      float eased = config.Retire == SlashRetireMode.Sweep
        ? Math.Clamp(_elapsed / sweep, 0f, 1f)
        : EaseOutCubic(_elapsed / sweep);
      _headDeg = MathHelper.Lerp(fromDeg, toDeg, eased);

      // —— 尾端: 以收拢速度向"前缘已扫过的路径"收去 (目标 = 前缘上一帧的位置).
      // 目标取上一帧而非当前帧: 收拢速度再快, 本帧刚扫出的弧段也保留到下一帧 ——
      // 弧带跨度恒 ≥ 单帧扫角, 永不塌成零宽 (否则收拢快于扫速时整条刀光不可见). ——
      if (!_tailInitialized)
      {
        _tailDeg = fromDeg;
        _tailInitialized = true;
      }
      float gap = headPrev - _tailDeg;
      float step = MathF.Max(0f, config.CatchupSpeed) * dt * MathF.Sign(gap);
      if (MathF.Abs(step) >= MathF.Abs(gap))
        _tailDeg = headPrev;
      else
        _tailDeg += step;

      // —— 结束: 渐隐按时间; 横扫按前缘到位且完全收拢 (CatchupSpeed=0 时以渐隐时长兜底) ——
      if (config.Retire == SlashRetireMode.Sweep)
      {
        if (Progress >= 1f && (MathF.Abs(_headDeg - _tailDeg) < 0.5f || _elapsed >= sweep + MathF.Max(1e-3f, config.FadeTime)))
          IsFinished = true;
      }
      else if (_elapsed >= sweep + MathF.Max(1e-3f, config.FadeTime))
      {
        IsFinished = true;
      }
    }

    /// <summary>
    /// 构建当前姿态的条带网格.
    /// </summary>
    /// <returns>三角带段数 (0 = 无需绘制).</returns>
    public int Build(SlashVertex[] vertices, short[] indices)
    {
      SlashArcConfig config = Config;
      int segments = Math.Max(2, config.Segments);

      float fromDeg = config.ArcFrom;
      float toDeg = config.ArcTo;
      if (config.Reversed)
        (fromDeg, toDeg) = (toDeg, fromDeg);

      // —— 弧带覆盖 [尾端, 前缘]; Fade 模式下尾端恒为起始角 ——
      float tailDeg = config.Retire == SlashRetireMode.Sweep ? _tailDeg : fromDeg;
      float spanDeg = _headDeg - tailDeg;
      if (MathF.Abs(spanDeg) < 1f)
        return 0;

      int count = Math.Max(2, (int)MathF.Ceiling(segments * MathF.Abs(spanDeg) / 360f)) + 1;
      EnsureCapacity(count);

      float globalFade = 1f;
      float sweep = MathF.Max(1e-4f, config.SweepTime);
      if (config.Retire == SlashRetireMode.Fade && _elapsed > sweep)
      {
        float fadeT = Math.Clamp((_elapsed - sweep) / MathF.Max(1e-4f, config.FadeTime), 0f, 1f);
        globalFade = 1f - fadeT * fadeT;
        if (globalFade <= 0f)
        {
          IsFinished = true;
          return 0;
        }
      }

      for (int i = 0; i < count; i++)
      {
        float t = i / (float)(count - 1);       // 0 = 头部 (前缘), 1 = 尾端.
        float angleDeg = MathHelper.Lerp(_headDeg, tailDeg, t);

        // 顶点色渐隐: Fade 模式用全局淡出; Sweep 模式尾端恒定渐隐 (静态色渐变).
        float fade = config.Retire == SlashRetireMode.Sweep
          ? MathHelper.Lerp(1f, 0.35f, t)
          : globalFade;
        PlacePoint(i, angleDeg, config, t, Math.Clamp(fade, 0f, 1f));
      }

      return RibbonBuilder.Build(_points, _halfWidths, _colors, _us, vertices, indices);
    }

    /// <summary>摆放单个弧点 (坐标系变换 + 等宽 + 头亮尾隐顶点色 × 存活系数).</summary>
    private void PlacePoint(int i, float angleDeg, SlashArcConfig config, float t, float fade)
    {
      float angleRad = MathHelper.ToRadians(angleDeg);
      float radius = config.Radius * Scale;

      // 整体坐标系: X/Y 独立缩放 (弧线椭圆化, 决定"长宽").
      Vector2 arcPoint = new Vector2(
        MathF.Cos(angleRad) * radius * config.ScaleX,
        MathF.Sin(angleRad) * radius * config.ScaleY);

      // 整体旋转.
      float rotation = Rotation + MathHelper.ToRadians(config.RotationDeg);
      if (rotation != 0f)
      {
        float cos = MathF.Cos(rotation), sin = MathF.Sin(rotation);
        arcPoint = new Vector2(
          arcPoint.X * cos - arcPoint.Y * sin,
          arcPoint.X * sin + arcPoint.Y * cos);
      }
      _points[i] = arcPoint + Position;

      // 等宽弯曲长方形: 宽度沿弧长恒定, 渐隐完全由顶点色承担.
      _halfWidths[i] = config.Width * 0.5f * Scale;

      // 顶点色: 头部亮白 → 尾部青蓝渐隐.
      Vector4 color = Vector4.Lerp(config.HeadColor, config.TailColor, t);
      color.X *= fade;
      color.Y *= fade;
      color.Z *= fade;
      color.W *= fade;
      _colors[i] = color;

      _us[i] = 1f - t;   // u: 1 = 头 (纹理亮端), 0 = 尾.
    }

    private void EnsureCapacity(int count)
    {
      if (_points.Length < count)
      {
        _points = new Vector2[count];
        _halfWidths = new float[count];
        _colors = new Vector4[count];
        _us = new float[count];
      }
    }

    /// <summary>缓动: 快出缓收的展开手感 (仅渐隐模式的展开阶段用).</summary>
    private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - Math.Clamp(t, 0f, 1f), 3f);
  }
}
