namespace Particle.Slash
{
  /// <summary>
  /// 弧形刀光实例 (拉刀光 Mesh): 一次完整的挥动明确分为两个阶段 ——
  /// <br><b>阶段一 挥动</b>: 前缘按速度曲线从起始角扫到结束角, 尾端钉在起始角, 弧带随挥动展开;</br>
  /// <br><b>阶段二 收尾</b>: 修饰器列表 (<see cref="SlashFadeFinish"/> 渐隐 /
  /// <see cref="SlashCollapseFinish"/> 收拢) 逐个推进, 可同时叠加, 全部播完即完成.</br>
  /// <br>顶点在"模型空间" (未缩放圆弧, 等宽) 内生成, 椭圆长宽/旋转/位移由相机矩阵承担
  /// (<see cref="TransformMatrix"/>) —— 仿射变换保证弯折再急面片也不自交.</br>
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

    /// <summary>当前前缘 (刃头) 角度 (度) —— 刃花沿此角度绑定发射.</summary>
    public float HeadAngleDeg => _headDeg;
    /// <summary>当前尾端角度 (度).</summary>
    public float TailAngleDeg => _tailDeg;
    /// <summary>前缘是否仍在扫进 (刃花发射门控).</summary>
    public bool IsSweeping => !IsFinished && Progress < 1f;
    /// <summary>第 <paramref name="index"/> 个纹理层的累计横向滚动偏移 (渲染层读取).</summary>
    public float GetLayerScroll(int index) => index >= 0 && index < _layerScrolls.Length ? _layerScrolls[index] : 0f;

    private float _elapsed;
    private float _headDeg;
    private float _tailDeg;

    // —— 收尾修饰器运行时: 启用列表 (随配置版本同步) 与各修饰器的计算结果 ——
    private readonly List<SlashFinishConfig> _activeFinishes = new List<SlashFinishConfig>();
    private int _configStamp = -1;
    private float _fadeScale = 1f;

    // —— 纹理层横向滚动状态 (逐层累计 ScrollSpeed) ——
    private float[] _layerScrolls = Array.Empty<float>();

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
      _fadeScale = 1f;
      Array.Clear(_layerScrolls, 0, _layerScrolls.Length);
    }

    /// <summary>同步启用的收尾修饰器 (配置版本变更或首次更新时重建;
    /// 旧配置无修饰器时按 Retire/FadeTime/CatchupTime 合成兜底).</summary>
    private void SyncFinishes()
    {
      int stamp = Config.Version;
      if (_configStamp == stamp)
        return;
      _configStamp = stamp;

      _activeFinishes.Clear();
      if (Config.Finishes is not null)
        foreach (SlashFinishConfig finish in Config.Finishes)
          if (finish is not null && finish.Enabled)
            _activeFinishes.Add(finish);

      if (_activeFinishes.Count == 0)
      {
        if (Config.Retire == SlashRetireMode.Sweep)
          _activeFinishes.Add(new SlashCollapseFinish { Duration = MathF.Max(1e-3f, Config.CatchupTime) });
        else
          _activeFinishes.Add(new SlashFadeFinish { Duration = MathF.Max(1e-3f, Config.FadeTime) });
      }
    }

    /// <summary>推进动画 (由 SlashRenderer.TickAll 或调用方驱动).
    /// <br><b>一次完整的挥动 = 阶段一 挥动 + 阶段二 收尾</b>:</br>
    /// <br>① 挥动: 前缘按速度曲线从起始角扫到结束角, 尾端钉在起始角 (弧带随挥动展开);</br>
    /// <br>② 收尾: 收尾修饰器列表逐个推进 (渐隐/收拢, 可同时叠加, 各自时长与曲线),
    /// 全部播完刀光才判定完成 —— 收拢到点必然收完, 不存在没收入的顶点.</br>
    /// </summary>
    public void Update(float dt)
    {
      if (IsFinished)
        return;

      SyncFinishes();

      SlashArcConfig config = Config;
      float fromDeg = config.ArcFrom;
      float toDeg = config.ArcTo;
      if (config.Reversed)
        (fromDeg, toDeg) = (toDeg, fromDeg);

      float sweep = MathF.Max(1e-4f, config.SweepTime);
      _elapsed += dt;

      if (_elapsed <= sweep)
      {
        // —— 阶段一 挥动: 前缘按速度曲线扫进; 尾端钉在起始角 ——
        float sweepT = Math.Clamp(_elapsed / sweep, 0f, 1f);
        Progress = sweepT;
        float progress = Math.Clamp(config.SweepCurve?.Evaluate(sweepT) ?? sweepT, 0f, 1f);
        _headDeg = MathHelper.Lerp(fromDeg, toDeg, progress);
        _tailDeg = fromDeg;
        _fadeScale = 1f;
      }
      else
      {
        // —— 阶段二 收尾: 各修饰器按自己的时长/曲线推进 (收拢改写尾端, 渐隐乘算透明度) ——
        Progress = 1f;
        _headDeg = toDeg;
        _tailDeg = fromDeg;
        _fadeScale = 1f;

        float finishT = _elapsed - sweep;
        bool allDone = true;
        for (int i = 0; i < _activeFinishes.Count; i++)
        {
          SlashFinishConfig finish = _activeFinishes[i];
          float t = Math.Clamp(finishT / MathF.Max(1e-3f, finish.Duration), 0f, 1f);
          if (t < 1f)
            allDone = false;
          float curved = Math.Clamp(finish.Curve?.Evaluate(t) ?? t, 0f, 1f);
          if (finish is SlashCollapseFinish)
            _tailDeg = MathHelper.Lerp(fromDeg, toDeg, curved);
          else if (finish is SlashFadeFinish)
            _fadeScale *= 1f - curved;
        }

        if (allDone)
        {
          IsFinished = true;
          return;
        }
      }

      // —— 纹理层横向滚动 (流光) ——
      List<SlashTextureLayer> layers = Config.Layers;
      if (layers is not null && layers.Count > 0)
      {
        if (_layerScrolls.Length < layers.Count)
          _layerScrolls = new float[layers.Count];
        for (int i = 0; i < layers.Count; i++)
          if (layers[i] is not null && layers[i].Enabled)
            _layerScrolls[i] += layers[i].ScrollSpeed * dt;
      }
    }

    /// <summary>
    /// 构建当前姿态的条带网格.
    /// <br>顶点在"模型空间"内生成: 未缩放的标准圆弧, 宽度沿半径方向且沿弧长恒定 ——
    /// 圆弧的法线恒为径向, 弯折再急内缘也不会交叉; 椭圆长宽 (ScaleX/ScaleY)、
    /// 整体旋转与位移全部由 <see cref="TransformMatrix"/> (相机矩阵) 承担,
    /// 缩放表现即相机俯拍 —— 仿射变换保证面片永不自交重叠.</br>
    /// </summary>
    /// <returns>三角带段数 (0 = 无需绘制).</returns>
    public int Build(SlashVertex[] vertices, short[] indices)
    {
      // —— 容灾: 已完成的刀光绝不绘制 —— 收尾模型保证修饰器到点必完成, 任何路径下都不会留下冻住的顶点. ——
      if (IsFinished)
        return 0;

      SlashArcConfig config = Config;
      int segments = Math.Max(2, config.Segments);

      // —— 弧带覆盖 [尾端, 前缘]: 两端角度均由 Update 阶段状态给出
      // (挥动期尾端钉在起始角; 收尾期收拢修饰器驱动尾端) ——
      float tailDeg = _tailDeg;
      float spanDeg = _headDeg - tailDeg;
      if (MathF.Abs(spanDeg) < 1f)
        return 0;

      int count = Math.Max(2, (int)MathF.Ceiling(segments * MathF.Abs(spanDeg) / 360f)) + 1;
      EnsureCapacity(count);

      // —— 全局透明度: 渐隐修饰器的乘算结果 (Update 阶段计算) ——
      float globalFade = _fadeScale;

      // —— 模型空间: 标准圆弧 + 整体面片等宽 (宽度超过直径的极端配置兜底) ——
      float radius = MathF.Max(1f, config.Radius * Scale);
      float halfWidth = MathF.Min(config.Width * 0.5f, radius * 0.95f);

      for (int i = 0; i < count; i++)
      {
        float t = i / (float)(count - 1);       // 0 = 头部 (前缘), 1 = 尾端.
        float angleDeg = MathHelper.Lerp(_headDeg, tailDeg, t);

        // 顶点色渐隐: Fade 模式用全局淡出; Sweep 模式尾端恒定渐隐 (静态色渐变).
        float fade = config.Retire == SlashRetireMode.Sweep
          ? MathHelper.Lerp(1f, 0.35f, t)
          : globalFade;
        PlacePoint(i, angleDeg, config, t, Math.Clamp(fade, 0f, 1f), radius, halfWidth);
      }

      return RibbonBuilder.Build(_points, _halfWidths, _colors, _us, vertices, indices);
    }

    /// <summary>摆放单个弧点 (模型空间圆弧 + 整体面片等宽 + 头亮尾隐顶点色; 变换交给相机矩阵).</summary>
    private void PlacePoint(int i, float angleDeg, SlashArcConfig config, float t, float fade, float radius, float halfWidth)
    {
      float angleRad = MathHelper.ToRadians(angleDeg);
      _points[i] = new Vector2(MathF.Cos(angleRad), MathF.Sin(angleRad)) * radius;

      // 等宽: 宽度参数控制整个刀光面片的宽度, 沿弧长恒定; 渐隐完全由顶点色承担.
      _halfWidths[i] = halfWidth;

      // 顶点色: 头部亮白 → 尾部青蓝渐隐.
      Vector4 color = Vector4.Lerp(config.HeadColor, config.TailColor, t);
      color.X *= fade;
      color.Y *= fade;
      color.Z *= fade;
      color.W *= fade;
      _colors[i] = color;

      _us[i] = 1f - t;   // u: 1 = 头 (纹理亮端), 0 = 尾.
    }

    /// <summary>
    /// 刀光的"相机矩阵" (模型空间 → 世界): 椭圆长宽缩放 (ScaleX/ScaleY)、
    /// 整体旋转 (实例 + 配置) 与弧心位移. Mesh 顶点在模型空间内生成后经此矩阵成像,
    /// 因此缩放由"相机"控制, 面片在模型空间内永远不折叠.
    /// </summary>
    public Matrix TransformMatrix()
    {
      SlashArcConfig config = Config;
      return Matrix.CreateScale(config.ScaleX, config.ScaleY, 1f)
        * Matrix.CreateRotationZ(Rotation + MathHelper.ToRadians(config.RotationDeg))
        * Matrix.CreateTranslation(Position.X, Position.Y, 0f);
    }

    /// <summary>
    /// 计算给定角度处的弧上世界坐标 —— 即模型空间圆弧点经 <see cref="TransformMatrix"/>
    /// (相机矩阵) 成像的结果 (X/Y 椭圆缩放 + 整体旋转 + 位置). 刃花沿前缘绑定发射时
    /// 用同一变换, 保证与 Mesh 完全贴合.
    /// </summary>
    public Vector2 PointAt(float angleDeg)
    {
      SlashArcConfig config = Config;
      float angleRad = MathHelper.ToRadians(angleDeg);
      float radius = config.Radius * Scale;

      // 整体坐标系: X/Y 独立缩放 (弧线椭圆化, 决定"长宽").
      Vector2 arcPoint = new Vector2(
        MathF.Cos(angleRad) * radius * config.ScaleX,
        MathF.Sin(angleRad) * radius * config.ScaleY);

      return RotateByConfig(arcPoint) + Position;
    }

    /// <summary>
    /// 给定角度处沿弧的切线方向 (单位向量, 指向角度增大的一侧, 即扫进方向).
    /// 椭圆参数曲线的导数方向, 与 PointAt 使用同一整体坐标系.
    /// </summary>
    public Vector2 TangentAt(float angleDeg)
    {
      SlashArcConfig config = Config;
      float angleRad = MathHelper.ToRadians(angleDeg);
      Vector2 tangent = new Vector2(
        -MathF.Sin(angleRad) * config.ScaleX,
        MathF.Cos(angleRad) * config.ScaleY);
      tangent = RotateByConfig(tangent);
      float length = tangent.Length();
      return length > 1e-5f ? tangent / length : Vector2.UnitX;
    }

    /// <summary>叠加整体旋转 (实例旋转 + 配置旋转).</summary>
    private Vector2 RotateByConfig(Vector2 value)
    {
      float rotation = Rotation + MathHelper.ToRadians(Config.RotationDeg);
      if (rotation == 0f)
        return value;
      float cos = MathF.Cos(rotation), sin = MathF.Sin(rotation);
      return new Vector2(
        value.X * cos - value.Y * sin,
        value.X * sin + value.Y * cos);
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
  }
}
