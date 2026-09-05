namespace Particle.Core
{
  /// <summary>发射形状类型.</summary>
  public enum EmissionShapeType
  {
    /// <summary>点发射.</summary>
    Point,
    /// <summary>线段发射 (发射器局部坐标, 相对锚点).</summary>
    Segment,
    /// <summary>圆环发射 (可仅在边缘).</summary>
    Circle,
    /// <summary>圆弧发射 (扇形弧线, 可沿切线方向发射 —— 刀光/挥砍首选).</summary>
    Arc
  }

  /// <summary>速度方向模式.</summary>
  public enum VelocityMode
  {
    /// <summary>沿形状法线向外 (圆/圆弧的径向).</summary>
    Radial,
    /// <summary>沿圆弧切线方向 (挥砍轨迹方向).</summary>
    Tangent,
    /// <summary>固定朝向范围内随机 (0° = +X, 顺时针为正, 与屏幕坐标一致).</summary>
    Direction,
    /// <summary>完全随机.</summary>
    Random
  }

  /// <summary>
  /// 可序列化的发射形状参数: 描述粒子出生点与初始速度方向的采样规则.
  /// <br>所有角度单位为度, 坐标为发射器局部坐标 (发射器变换在运行时叠加).</br>
  /// </summary>
  [Serializable]
  public class EmissionShapeConfig
  {
    /// <summary>形状类型.</summary>
    public EmissionShapeType Shape = EmissionShapeType.Point;

    /// <summary>线段: 起点相对锚点偏移.</summary>
    public Vector2 SegmentFrom = new Vector2(0, 0);
    /// <summary>线段: 终点相对锚点偏移.</summary>
    public Vector2 SegmentTo = new Vector2(40, 0);
    /// <summary>圆/圆弧半径 (像素).</summary>
    public float Radius = 24f;
    /// <summary>圆弧: 弧心角 (度).</summary>
    public float ArcAngle = 90f;
    /// <summary>圆弧: 起始角 (度).</summary>
    public float ArcStart = -45f;
    /// <summary>圆: 是否仅在圆周边缘出生 (否则在圆盘内随机).</summary>
    public bool EdgeOnly = true;

    /// <summary>速度方向模式.</summary>
    public VelocityMode Velocity = VelocityMode.Radial;
    /// <summary>固定朝向: 基准角 (度, 0° = +X).</summary>
    public float DirectionBase = 0f;
    /// <summary>固定朝向: 随机散布半角 (度).</summary>
    public float DirectionSpread = 15f;

    /// <summary>出生点随机抖动半径 (像素).</summary>
    public float Jitter = 0f;

    /// <summary>横扫角速度 (度/秒): 发射基准角随发射器时间推进 —— 弧形刀光的"横扫"来源; 0 = 静止.</summary>
    public float SweepSpeed = 0f;

    /// <summary>深拷贝.</summary>
    public EmissionShapeConfig Clone() => new EmissionShapeConfig
    {
      Shape = Shape,
      SegmentFrom = SegmentFrom,
      SegmentTo = SegmentTo,
      Radius = Radius,
      ArcAngle = ArcAngle,
      ArcStart = ArcStart,
      EdgeOnly = EdgeOnly,
      Velocity = Velocity,
      DirectionBase = DirectionBase,
      DirectionSpread = DirectionSpread,
      Jitter = Jitter,
      SweepSpeed = SweepSpeed
    };

    /// <summary>采样入口 (无时间信息的旧接口, 横扫不生效).</summary>
    public void Sample(Random random, out Vector2 position, out Vector2 direction)
      => Sample(random, 0f, out position, out direction);

    /// <summary>
    /// 采样一个出生点与速度方向 (单位向量), 由运行时发射器调用.
    /// </summary>
    /// <param name="random">随机源.</param>
    /// <param name="elapsedSeconds">发射器已累计时间 (秒) —— 驱动横扫角的推进.</param>
    /// <param name="position">输出: 局部出生点.</param>
    /// <param name="direction">输出: 单位速度方向 (已经按形状规则得出).</param>
    public void Sample(Random random, float elapsedSeconds, out Vector2 position, out Vector2 direction)
    {
      // 横扫: 发射基准角 = 起始角 + 时间 × 角速度; ArcAngle 退化为前缘角度窗口.
      float sweepOffset = SweepSpeed * elapsedSeconds;
      switch (Shape)
      {
        case EmissionShapeType.Segment:
          float f = (float)random.NextDouble();
          position = Vector2.Lerp(SegmentFrom, SegmentTo, f);
          break;

        case EmissionShapeType.Circle:
          float radAngle = (float)(random.NextDouble() * MathHelper.TwoPi);
          float r = EdgeOnly ? Radius : Radius * (float)random.NextDouble();
          position = new Vector2(MathF.Cos(radAngle), MathF.Sin(radAngle)) * r;
          break;

        case EmissionShapeType.Arc:
          float arc = ArcStart + sweepOffset + (float)random.NextDouble() * ArcAngle;
          float arcRad = MathHelper.ToRadians(arc);
          position = new Vector2(MathF.Cos(arcRad), MathF.Sin(arcRad)) * Radius;
          break;

        default:
          position = Vector2.Zero;
          break;
      }

      if (Jitter > 0f)
        position += RandomUnitVector(random) * Jitter * (float)random.NextDouble();

      switch (Velocity)
      {
        case VelocityMode.Tangent:
          // 圆弧切线 = 径向逆时针旋转 90° (弧线挥砍的运动方向).
          float len2 = position.Length();
          direction = len2 > 1e-4f ? new Vector2(-position.Y, position.X) / len2 : Vector2.UnitX;
          break;

        case VelocityMode.Direction:
          float angle = MathHelper.ToRadians(DirectionBase + ((float)random.NextDouble() * 2f - 1f) * DirectionSpread);
          direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
          break;

        case VelocityMode.Random:
          direction = RandomUnitVector(random);
          break;

        default:
          float plen = position.Length();
          direction = plen > 1e-4f ? position / plen : Vector2.UnitX;
          break;
      }
    }

    private static Vector2 RandomUnitVector(Random random)
    {
      float angle = (float)(random.NextDouble() * MathHelper.TwoPi);
      return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }
  }
}
