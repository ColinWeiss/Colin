namespace Particle.Core
{
  /// <summary>代码式发射的单粒子初始化参数 (绕过发射形状采样, 用于刀光等由逻辑驱动的发射).</summary>
  public struct ParticleSpawnInit
  {
    /// <summary>出生位置 (世界坐标).</summary>
    public Vector2 Position;
    /// <summary>初速度 (像素/秒).</summary>
    public Vector2 Velocity;
    /// <summary>基准尺寸 (像素).</summary>
    public float Size;
    /// <summary>初始旋转 (弧度).</summary>
    public float Rotation;
    /// <summary>总生命 (秒).</summary>
    public float Life;
    /// <summary>长宽比.</summary>
    public float Aspect;
    /// <summary>角速度 (弧度/秒).</summary>
    public float AngularVel;
    /// <summary>色调系数.</summary>
    public float Tint;
    /// <summary>拉伸模式 (0 = 公告牌, 1 = 速度拉伸).</summary>
    public float Stretch;
  }

  /// <summary>
  /// 一个完整的粒子效果实例 (组合模式): 由多个运行时发射器构成, 共享一块粒子槽位池.
  /// <br>提供 <see cref="Play"/> / <see cref="Pause"/> / <see cref="Stop"/> /
  /// <see cref="Update"/> / <see cref="Draw"/> 完整生命周期, 可独立于管理器使用,
  /// 也可交由 <see cref="ParticleManager"/> 统一驱动.</br>
  /// </summary>
  public sealed class ParticleEffect : IDisposable
  {
    /// <summary>效果的配置 (编辑器可直接修改本引用, 观察者模式实时生效).</summary>
    public ParticleEffectConfig Config;

    /// <summary>运行时发射器列表 (与 Config.Emitters 一一对应).</summary>
    public List<ParticleEmitter> Emitters = new List<ParticleEmitter>();

    /// <summary>是否正在播放.</summary>
    public bool IsPlaying;
    /// <summary>是否暂停.</summary>
    public bool IsPaused;
    /// <summary>是否已播放完毕 (非循环效果且所有粒子死亡).</summary>
    public bool IsFinished;

    /// <summary>效果累计时间 (秒).</summary>
    public float Time;

    /// <summary>效果变换: 位置 (世界坐标, 像素).</summary>
    public Vector2 Position;
    /// <summary>效果变换: 旋转 (弧度).</summary>
    public float Rotation;
    /// <summary>效果变换: 缩放.</summary>
    public float Scale = 1f;

    /// <summary>预览专用效果 (编辑器预览用, 不参与游戏场景渲染).</summary>
    public bool PreviewOnly;

    /// <summary>本帧实例绘制数量 (诊断用).</summary>
    public int InstanceCount;

    /// <summary>本效果使用的更新策略 (诊断用).</summary>
    public IParticleUpdateStrategy Strategy => _strategy;

    private IParticleUpdateStrategy _strategy;
    private ParticleSimFrame _frame = new ParticleSimFrame();
    private List<ParticleSpawnInit> _pendingCustom = new List<ParticleSpawnInit>();
    private int _customEmitterIndex;
    private float _maxLife;
    private bool _capacityDirty;
    private Random _seedRandom = new Random();

    /// <summary>效果实例标识 (诊断与日志).</summary>
    public int Id { get; } = _nextId++;
    private static int _nextId = 1;

    /// <summary>由管理器或用户创建: 依据配置构建发射器与更新策略 (初始化失败自动回退 CPU).</summary>
    public ParticleEffect(ParticleEffectConfig config, GraphicsDevice device)
    {
      Config = config;
      _strategy = ParticleManager.Instance.CreateStrategy(config.TotalCapacity());
      BuildEmitters(seed: 0);
      Config.Subscribe(OnConfigChanged);
      IsPlaying = true;
    }

    private void BuildEmitters(int? seed)
    {
      Emitters.Clear();
      int range = 0;
      _maxLife = 0f;
      for (int i = 0; i < Config.Emitters.Count; i++)
      {
        EmitterConfig emitterConfig = Config.Emitters[i];
        ParticleEmitter emitter = new ParticleEmitter(emitterConfig, range);
        emitter.Reset(emitterConfig, range, seed);
        range += Math.Max(1, emitterConfig.Capacity);
        _maxLife = Math.Max(_maxLife, emitter.MaxLife);
        Emitters.Add(emitter);
      }
      _capacityDirty = false;
    }

    private void OnConfigChanged(ParticleEffectConfig config) { }

    /// <summary>播放 (恢复).</summary>
    public void Play() { IsPlaying = true; IsPaused = false; IsFinished = false; }

    /// <summary>暂停.</summary>
    public void Pause() => IsPaused = true;

    /// <summary>重置到初始状态并立即播放 (随机种子可固定).</summary>
    public void Reset(int? seed = null)
    {
      Time = 0f;
      IsFinished = false;
      IsPaused = false;
      IsPlaying = true;
      _pendingCustom.Clear();
      int usedSeed = seed ?? _seedRandom.Next();
      BuildEmitters(Config.Seed != 0 ? Config.Seed : usedSeed);
      _frame.DrawEnd = 0;
    }

    /// <summary>停止并标记完成 (非循环).</summary>
    public void Stop()
    {
      IsPlaying = false;
      IsFinished = true;
    }

    /// <summary>
    /// 推进效果一帧: 更新发射器、生成粒子并提交更新策略.
    /// <br>由 <see cref="ParticleManager.UpdateAll"/> 统一调用, 也可自行驱动 (二选一).</br>
    /// </summary>
    public void Update(float dt)
    {
      if (!IsPlaying || IsPaused)
        return;

      // —— 结构性变更 (容量/发射器增删) 需重建槽位池 ——
      if (_capacityDirty || Emitters.Count != Config.Emitters.Count)
        Rebuild();

      float duration = MathF.Max(1e-4f, Config.Duration);
      Time += dt;

      bool looping = Config.Looping;
      if (looping && Time >= duration)
      {
        Time -= duration;
        for (int i = 0; i < Emitters.Count; i++)
          Emitters[i].RestartCycle();
      }

      float normalized = looping ? (Time / duration) % 1f : Math.Clamp(Time / duration, 0f, 1f);
      bool emitting = looping || Time <= duration;

      _frame.Dt = dt;
      _frame.Emitters.Clear();
      _frame.DrawEnd = 0;

      int customTake = _pendingCustom.Count;

      for (int i = 0; i < Emitters.Count; i++)
      {
        ParticleEmitter emitter = Emitters[i];
        EmitterConfig config = emitter.Config;

        EmitterFrame emitterFrame = new EmitterFrame
        {
          RangeStart = emitter.RangeStart,
          Capacity = config.Capacity,
          Config = config
        };

        if (emitting)
          emitter.Advance(dt, normalized, emitterFrame.Spawns);

        emitterFrame.Params = new ParticleSimParams
        {
          Dt = dt,
          GravityX = config.Gravity.X,
          GravityY = config.Gravity.Y,
          Drag = config.Drag,
          RangeStart = emitter.RangeStart,
          RangeEnd = emitter.RangeStart + Math.Max(1, config.Capacity),
          ColorKeyCount = config.ColorKeyCount,
          AlphaKeyCount = config.AlphaKeyCount,
          SizeKeyCount = config.SizeKeyCount
        };

        // —— 效果变换叠加到配置发射的记录上 (代码式发射的记录视为世界坐标, 不叠加) ——
        List<ParticleSpawnRecord> spawns = emitterFrame.Spawns;
        for (int s = 0; s < spawns.Count; s++)
        {
          ParticleSpawnRecord transformed = spawns[s];
          ParticleEmitter.TransformRecord(ref transformed, Position, Rotation, Scale);
          spawns[s] = transformed;
        }

        _frame.DrawEnd = Math.Max(_frame.DrawEnd, emitter.RangeStart + emitter.Occupancy);
        _frame.Emitters.Add(emitterFrame);
      }

      // —— 代码式发射: 直接写入指定发射器的环形槽位 (世界坐标) ——
      if (customTake > 0)
        FlushCustom(customTake, dt);

      _strategy.Submit(_frame);
      _maxLife = 0f;
      for (int i = 0; i < Emitters.Count; i++)
        _maxLife = Math.Max(_maxLife, Emitters[i].MaxLife);

      InstanceCount = _frame.DrawEnd;

      // —— 非循环效果在发射结束且全部粒子死亡后自动完成 ——
      if (!Config.Looping && Time > duration + _maxLife + 0.1f)
      {
        IsPlaying = false;
        IsFinished = true;
      }
    }

    /// <summary>排队一批代码式发射 (世界坐标, 在下一次 Update 前生效).</summary>
    public void EmitCustom(IReadOnlyList<ParticleSpawnInit> inits, int emitterIndex = 0)
    {
      for (int i = 0; i < inits.Count; i++)
        _pendingCustom.Add(inits[i]);
      _customEmitterIndex = emitterIndex;
    }

    /// <summary>排队一条代码式发射 (世界坐标).</summary>
    public void EmitCustom(in ParticleSpawnInit init, int emitterIndex = 0)
    {
      _pendingCustom.Add(init);
      _customEmitterIndex = emitterIndex;
    }

    private void FlushCustom(int count, float dt)
    {
      int index = Math.Clamp(_customEmitterIndex, 0, Emitters.Count - 1);
      ParticleEmitter emitter = Emitters[index];
      EmitterConfig config = emitter.Config;

      EmitterFrame emitterFrame = new EmitterFrame
      {
        RangeStart = emitter.RangeStart,
        Capacity = config.Capacity,
        Config = config,
        Params = new ParticleSimParams
        {
          Dt = dt,
          GravityX = config.Gravity.X,
          GravityY = config.Gravity.Y,
          Drag = config.Drag,
          RangeStart = emitter.RangeStart,
          RangeEnd = emitter.RangeStart + Math.Max(1, config.Capacity),
          ColorKeyCount = config.ColorKeyCount,
          AlphaKeyCount = config.AlphaKeyCount,
          SizeKeyCount = config.SizeKeyCount
        }
      };

      // —— 将已收集的发射器帧合并, 代码式记录追加到末尾 ——
      // (管理器每帧只调用一次 Update, 此时 _frame.Emitters 已包含该发射器的帧)
      for (int i = 0; i < _frame.Emitters.Count; i++)
      {
        if (_frame.Emitters[i].RangeStart == emitter.RangeStart)
        {
          emitterFrame = _frame.Emitters[i];
          break;
        }
      }

      for (int i = _pendingCustom.Count - count; i < _pendingCustom.Count; i++)
      {
        ParticleSpawnInit init = _pendingCustom[i];

        ParticleSpawnRecord record = default;
        record.PosX = init.Position.X;
        record.PosY = init.Position.Y;
        record.VelX = init.Velocity.X;
        record.VelY = init.Velocity.Y;
        record.Size = init.Size;
        record.Rotation = init.Rotation;
        record.Life = init.Life;
        record.Slot = emitter.RangeStart + emitter.RingTake();
        record.Seed = (float)_seedRandom.NextDouble();
        record.Aspect = init.Aspect;
        record.AngularVel = init.AngularVel;
        record.Tint = init.Tint;
        record.Stretch = init.Stretch;
        emitterFrame.Spawns.Add(record);
      }

      emitter.Occupancy = Math.Min(emitter.Occupancy + count, config.Capacity);
      _frame.DrawEnd = Math.Max(_frame.DrawEnd, emitter.RangeStart + emitter.Occupancy);
      _pendingCustom.Clear();
    }

    /// <summary>
    /// 绘制本效果 (由 <see cref="ParticleManager.RenderAll"/> 统一调用, 也可自行调用).
    /// </summary>
    /// <param name="transform">相机变换矩阵 (世界坐标 → 裁剪空间).</param>
    public void Draw(Matrix transform)
    {
      (VertexBuffer buffer, int instanceCount) = _strategy.ResolveFrame();
      InstanceCount = instanceCount;
      if (buffer is null || instanceCount <= 0)
        return;
      global::Particle.Rendering.ParticleRenderer.Shared?.Draw(buffer, instanceCount, Config.Render, transform);
    }

    private void Rebuild()
    {
      _strategy.Dispose();
      _strategy = ParticleManager.Instance.CreateStrategy(Config.TotalCapacity());
      BuildEmitters(null);
      _capacityDirty = false;
    }

    /// <summary>标记槽位池需要重建 (容量或发射器结构发生变化时由观察者调用).</summary>
    public void MarkRebuildRequired() => _capacityDirty = true;

    public void Dispose()
    {
      Config.Unsubscribe(OnConfigChanged);
      _strategy?.Dispose();
      _strategy = null;
      Emitters.Clear();
    }
  }
}
