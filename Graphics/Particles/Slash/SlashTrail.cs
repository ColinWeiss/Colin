namespace Particle.Slash
{
  /// <summary>
  /// 轨迹历史型刀光 (游戏内剑光): 挥砍时每帧 <see cref="AddPoint"/> 记录剑尖位置,
  /// 条带 Mesh 直接沿轨迹点列摆放 —— 头部 (最新点) 宽而亮, 尾部 (旧点) 收窄渐隐,
  /// 停止挥砍后轨迹随时间自然流逝消散. 即 Unity 拉刀光 Trail 的做法.
  /// </summary>
  public sealed class SlashTrail
  {
    /// <summary>轨迹点保留时长 (秒); 超龄点被淘汰.</summary>
    public float TrailTime = 0.16f;
    /// <summary>最大全宽 (像素, 头部最宽处).</summary>
    public float Width = 30f;
    /// <summary>头部颜色.</summary>
    public Vector4 HeadColor = new Vector4(1.4f, 1.4f, 1.4f, 1f);
    /// <summary>尾部颜色.</summary>
    public Vector4 TailColor = new Vector4(0.35f, 0.6f, 1.0f, 0f);
    /// <summary>纹理名.</summary>
    public string Texture = "blade";
    /// <summary>轨迹点间距下限 (像素) —— 剑尖几乎没动时不重复记录.</summary>
    public float MinPointDistance = 1.5f;
    /// <summary>轨迹是否已空 (渲染端可跳过).</summary>
    public bool IsEmpty => _points.Count < 2;

    private readonly struct TrailPoint
    {
      public readonly Vector2 Position;
      public readonly float Age;
      public TrailPoint(Vector2 position, float age)
      {
        Position = position;
        Age = age;
      }
    }

    private readonly List<TrailPoint> _points = new List<TrailPoint>(64);
    private Vector2 _lastPosition;
    private bool _hasLast;

    // —— 网格构建缓存 ——
    private Vector2[] _points_cache = new Vector2[64];
    private float[] _halfWidths_cache = new float[64];
    private Vector4[] _colors_cache = new Vector4[64];
    private float[] _us_cache = new float[64];

    public SlashTrail()
    {
    }

    /// <summary>清空轨迹 (挥砍开始时).</summary>
    public void Clear()
    {
      _points.Clear();
      _hasLast = false;
    }

    /// <summary>记录一个剑尖位置 (世界坐标, 每帧调用).</summary>
    public void AddPoint(Vector2 position)
    {
      if (_hasLast && Vector2.DistanceSquared(position, _lastPosition) < MinPointDistance * MinPointDistance)
        return;
      _lastPosition = position;
      _hasLast = true;
      _points.Insert(0, new TrailPoint(position, 0f));   // 头部在前.
      if (_points.Count > 64)
        _points.RemoveAt(_points.Count - 1);
    }

    /// <summary>推进轨迹年龄并淘汰超龄点 (由 SlashRenderer.TickAll 驱动).</summary>
    public void Update(float dt)
    {
      for (int i = 0; i < _points.Count; i++)
      {
        TrailPoint point = _points[i];
        _points[i] = new TrailPoint(point.Position, point.Age + dt);
      }
      while (_points.Count > 0 && _points[_points.Count - 1].Age > TrailTime)
        _points.RemoveAt(_points.Count - 1);
    }

    /// <summary>
    /// 构建当前轨迹的条带网格.
    /// </summary>
    /// <returns>三角带段数 (0 = 无需绘制).</returns>
    public int Build(SlashVertex[] vertices, short[] indices)
    {
      int count = _points.Count;
      if (count < 2)
        return 0;

      if (_points_cache.Length < count)
      {
        _points_cache = new Vector2[64];
        _halfWidths_cache = new float[64];
        _colors_cache = new Vector4[64];
        _us_cache = new float[64];
      }

      for (int i = 0; i < count; i++)
      {
        TrailPoint point = _points[i];
        float age01 = Math.Clamp(point.Age / MathF.Max(1e-4f, TrailTime), 0f, 1f);

        _points_cache[i] = point.Position;

        // 等宽轨迹 (战场剑2 刀光): 宽度恒定, 头亮尾隐由颜色/透明度承担.
        _halfWidths_cache[i] = Width * 0.5f;


        // 头亮尾隐: 颜色随轨迹年龄渐变 + 亮度随年龄衰减.
        Vector4 color = Vector4.Lerp(HeadColor, TailColor, i / (float)(count - 1));
        float brightness = 1f - age01 * age01;
        color.X *= brightness;
        color.Y *= brightness;
        color.Z *= brightness;
        color.W *= brightness;
        _colors_cache[i] = color;

        _us_cache[i] = 1f - i / (float)(count - 1);
      }

      return RibbonBuilder.Build(_points_cache, _halfWidths_cache, _colors_cache, _us_cache, vertices, indices);
    }
  }
}
