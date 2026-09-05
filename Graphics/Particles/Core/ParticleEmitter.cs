namespace Particle.Core
{
  /// <summary>
  /// 运行时发射器: 持有配置的运行状态, 每帧采样形状并产出粒子生成记录.
  /// <br>槽位采用环形分配 —— CPU 精确掌握每个槽位的死亡时间, 槽位仅在
  /// 旧粒子确认死亡后复用, GPU 侧无需原子回收链表.</br>
  /// </summary>
  public sealed class ParticleEmitter
  {
    /// <summary>发射器配置.</summary>
    public EmitterConfig Config;

    /// <summary>槽位区间起点 (效果池内绝对索引).</summary>
    public int RangeStart;

    /// <summary>当前占用槽位数 (环形写入推进后的实际占用, ≤ 容量).</summary>
    public int Occupancy;

    /// <summary>发射器累积时间 (自效果开始, 含开始延迟).</summary>
    public float LocalTime;

    // —— 内部状态 ——
    private int _ringHead;
    private int _written;
    private float _accumulator;
    private int _burstCursor;
    private Random _random;
    private float _maxLife;

    /// <summary>单帧最大生成数量 (防止极端速率一次性灌爆生成缓冲).</summary>
    public const int MaxSpawnsPerFrame = 256;

    public ParticleEmitter(EmitterConfig config, int rangeStart) => Reset(config, rangeStart);

    /// <summary>重置发射器状态 (随机种子可选固定).</summary>
    public void Reset(EmitterConfig config, int rangeStart, int? seed = null)
    {
      Config = config;
      RangeStart = rangeStart;
      _ringHead = 0;
      _written = 0;
      _accumulator = 0f;
      _burstCursor = 0;
      LocalTime = 0f;
      Occupancy = 0;
      _maxLife = Math.Max(config.LifeMin, config.LifeMax);
      _random = config.Seed != 0 ? new Random(config.Seed) : new Random(seed ?? Environment.TickCount);
    }

    /// <summary>该发射器支持的最大粒子寿命 (秒), 用于效果的自动结束判定.</summary>
    public float MaxLife => _maxLife;

    /// <summary>循环模式下的周期重启: 复位发射计时与突发事件游标, 但保留环形槽位.</summary>
    public void RestartCycle()
    {
      LocalTime = 0f;
      _burstCursor = 0;
      _accumulator = 0f;
    }

    /// <summary>从环形槽位中取出下一个槽位索引 (供代码式发射使用).</summary>
    public int RingTake()
    {
      int slot = _ringHead;
      _ringHead = (_ringHead + 1) % Config.Capacity;
      _written++;
      Occupancy = Math.Min(_written, Config.Capacity);
      return slot;
    }

    /// <summary>
    /// 推进发射器一帧: 处理开始延迟、速率曲线累积与突发事件,
    /// 将本帧生成记录追加到 <paramref name="output"/>.
    /// </summary>
    public void Advance(float dt, float normalizedEffectTime, List<ParticleSpawnRecord> output)
    {
      LocalTime += dt;
      if (LocalTime < Config.StartTime)
        return;

      // —— 持续发射: 速率 × 速率曲线 ——
      float rate = Config.EmissionRate * MathF.Max(0f, Config.RateCurve.Evaluate(normalizedEffectTime));
      if (rate > 0f)
      {
        _accumulator += rate * dt;
        int budget = MaxSpawnsPerFrame;
        while (_accumulator >= 1f && budget-- > 0)
        {
          _accumulator -= 1f;
          Emit(output, 1f, normalizedEffectTime);
        }
        if (budget <= 0)
          _accumulator = MathF.Min(_accumulator, 1f);
      }

      // —— 突发事件 ——
      List<BurstEvent> bursts = Config.Bursts;
      while (_burstCursor < bursts.Count && LocalTime >= bursts[_burstCursor].Time)
      {
        BurstEvent burst = bursts[_burstCursor];
        _burstCursor++;
        int budget = MaxSpawnsPerFrame;
        for (int i = 0; i < burst.Count && budget-- > 0; i++)
          Emit(output, burst.VelocityScale, normalizedEffectTime);
      }
    }

    /// <summary>发射一个粒子: 采样形状与参数, 写入生成记录.</summary>
    public void Emit(List<ParticleSpawnRecord> output, float velocityScale = 1f, float normalizedEffectTime = 0f)
    {
      EmitterConfig c = Config;

      c.Shape.Sample(_random, LocalTime - Config.StartTime, out Vector2 position, out Vector2 direction);

      float speed = MathHelper.Lerp(c.SpeedMin, c.SpeedMax, (float)_random.NextDouble())
                  * MathF.Max(0f, c.SpeedCurve.Evaluate(normalizedEffectTime));
      Vector2 velocity = direction * speed * velocityScale;

      ParticleSpawnRecord record = default;
      record.PosX = position.X;
      record.PosY = position.Y;
      record.VelX = velocity.X;
      record.VelY = velocity.Y;
      record.Size = MathHelper.Lerp(c.SizeMin, c.SizeMax, (float)_random.NextDouble());
      record.Rotation = MathHelper.ToRadians(MathHelper.Lerp(c.RotationMin, c.RotationMax, (float)_random.NextDouble()));
      record.Life = MathHelper.Lerp(c.LifeMin, c.LifeMax, (float)_random.NextDouble());
      record.Slot = RangeStart + _ringHead;
      record.Seed = (float)_random.NextDouble();
      record.Aspect = c.Aspect;
      record.AngularVel = MathHelper.ToRadians(MathHelper.Lerp(c.AngularVelocityMin, c.AngularVelocityMax, (float)_random.NextDouble()));
      record.Tint = MathHelper.Lerp(c.TintMin, c.TintMax, (float)_random.NextDouble());
      record.Stretch = c.StretchedBillboard ? 1f : 0f;

      output.Add(record);

      _ringHead = (_ringHead + 1) % c.Capacity;
      _written++;
      Occupancy = Math.Min(_written, c.Capacity);
    }

    /// <summary>将发射器变换 (效果位置/旋转/缩放) 应用到记录上.</summary>
    public static void TransformRecord(ref ParticleSpawnRecord record, Vector2 position, float rotation, float scale)
    {
      if (rotation == 0f && scale == 1f)
      {
        record.PosX += position.X;
        record.PosY += position.Y;
        return;
      }
      float c = MathF.Cos(rotation), s = MathF.Sin(rotation);
      float x = record.PosX * scale, y = record.PosY * scale;
      record.PosX = x * c - y * s + position.X;
      record.PosY = x * s + y * c + position.Y;
      x = record.VelX * scale;
      y = record.VelY * scale;
      record.VelX = x * c - y * s;
      record.VelY = x * s + y * c;
    }
  }
}
