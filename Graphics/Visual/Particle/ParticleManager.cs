using System.Threading;

namespace Colin.Core.Graphics.Visual.Particle
{
  /// <summary>
  /// 粒子系统管理器 (单例, 线程安全): 统一持有活跃效果, 驱动每帧更新与渲染.
  /// <br>策略选择: 优先使用 ComputeSharp GPU 零拷贝策略, 设备/共享缓冲区创建失败时
  /// 自动回退 CPU 策略 (两者渲染路径一致).</br>
  /// <br>更新与渲染既可交由引擎挂钩 (EcsAdvancedRenderSystem) 自动驱动, 也可手动调用.</br>
  /// </summary>
  public sealed class ParticleManager
  {
    private static ParticleManager _instance;
    private static readonly object _lock = new object();

    /// <summary>全局实例 (首次访问时惰性创建).</summary>
    public static ParticleManager Instance
    {
      get
      {
        if (_instance is null)
        {
          lock (_lock)
          {
            _instance ??= new ParticleManager();
          }
        }
        return _instance;
      }
    }

    /// <summary>MonoGame 图形设备 (初始化后可用).</summary>
    public GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>共享渲染器 (空时不渲染).</summary>
    public Colin.Core.Graphics.Visual.Particle.Rendering.ParticleRenderer Renderer { get; private set; }

    /// <summary>初始化是否已完成.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>最近一次策略创建的实际路径 (诊断用).</summary>
    public string StrategyPath { get; private set; } = "未初始化";

    /// <summary>活跃效果快照 (诊断与遍历).</summary>
    public IReadOnlyList<ParticleEffect> ActiveEffects => _effects;

    /// <summary>当前活跃效果数.</summary>
    public int EffectCount => _effects.Count;

    /// <summary>全系统粒子总量上限 (活跃效果容量之和).</summary>
    public int TotalCapacity
    {
      get
      {
        int total = 0;
        for (int i = 0; i < _effects.Count; i++)
          total += _effects[i].Config.TotalCapacity();
        return total;
      }
    }

    private readonly List<ParticleEffect> _effects = new List<ParticleEffect>();
    private long _lastUpdateFrame = -1;

    private ParticleManager() { }

    /// <summary>
    /// 初始化管理器 (渲染器与共享资源); 幂等, 可在任意时刻显式调用,
    /// 亦会在首次播放时自动执行.
    /// </summary>
    public void Initialize(GraphicsDevice device)
    {
      lock (_lock)
      {
        if (IsInitialized)
          return;
        GraphicsDevice = device ?? throw new ArgumentNullException(nameof(device));
        Renderer ??= new Colin.Core.Graphics.Visual.Particle.Rendering.ParticleRenderer(device);
        IsInitialized = true;
        Console.WriteLine("Remind", $"粒子系统初始化完成.");
      }
    }

    /// <summary>
    /// 创建一条已初始化的更新策略: GPU 零拷贝优先, 设备丢失/共享缓冲失败等一律回退 CPU.
    /// </summary>
    /// <param name="capacity">粒子容量 (槽位总数).</param>
    public IParticleUpdateStrategy CreateStrategy(int capacity)
    {
      if (!IsInitialized)
        throw new InvalidOperationException("ParticleManager 尚未初始化, 请先调用 Initialize.");
      lock (_lock)
      {
        try
        {
          Colin.Core.Graphics.Visual.Particle.GpuUpdateStrategy gpu = new Colin.Core.Graphics.Visual.Particle.GpuUpdateStrategy();
          gpu.Initialize(GraphicsDevice, capacity);
          StrategyPath = gpu.Name;
          return gpu;
        }
        catch (Exception exception)
        {
          Console.WriteLine("Error", "GPU 粒子策略创建失败, 回退 CPU: " + exception.Message);
          CpuUpdateStrategy cpu = new CpuUpdateStrategy();
          cpu.Initialize(GraphicsDevice, capacity);
          StrategyPath = cpu.Name;
          return cpu;
        }
      }
    }

    /// <summary>
    /// 依据配置播放一个粒子效果 (工厂可由 ParticlePresetFactory 提供配置).
    /// </summary>
    /// <param name="config">效果配置 (管理器不克隆 —— 编辑器场景下保持引用以支持实时修改).</param>
    /// <param name="position">效果位置 (世界坐标, 像素).</param>
    /// <param name="rotation">效果旋转 (弧度).</param>
    /// <param name="scale">效果缩放.</param>
    /// <returns>效果实例句柄; Stop/Reset/变换更新均作用于该实例.</returns>
    public ParticleEffect Play(ParticleEffectConfig config, Vector2 position, float rotation = 0f, float scale = 1f)
    {
      if (!IsInitialized)
        Initialize(CoreInfo.Graphics.GraphicsDevice);
      lock (_lock)
      {
        ParticleEffect effect = new ParticleEffect(config, GraphicsDevice)
        {
          Position = position,
          Rotation = rotation,
          Scale = scale
        };
        // 发射器容量变化需要重建槽位池 —— 订阅各发射器配置的变更.
        for (int i = 0; i < config.Emitters.Count; i++)
        {
          EmitterConfig emitter = config.Emitters[i];
          emitter.Subscribe(_ => effect.MarkRebuildRequired());
        }
        _effects.Add(effect);
        return effect;
      }
    }

    /// <summary>停止并移除一个效果实例.</summary>
    public void Stop(ParticleEffect effect)
    {
      if (effect is null)
        return;
      lock (_lock)
      {
        effect.Stop();
        _effects.Remove(effect);
        effect.Dispose();
      }
    }

    /// <summary>停止全部效果.</summary>
    public void StopAll()
    {
      lock (_lock)
      {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
          _effects[i].Stop();
          _effects[i].Dispose();
        }
        _effects.Clear();
      }
    }

    /// <summary>
    /// 推进全部活跃效果一帧 (帧内守卫: 同一游戏帧多次调用只生效一次).
    /// </summary>
    public void UpdateAll(float dt)
    {
      if (!IsInitialized)
        return;
      if (Interlocked.Read(ref _lastUpdateFrame) == Time.FrameCount)
        return;
      Interlocked.Exchange(ref _lastUpdateFrame, Time.FrameCount);

      lock (_lock)
      {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
          ParticleEffect effect = _effects[i];
          effect.Update(dt);
          // 非循环且播完的效果自动回收.
          if (effect.IsFinished && !effect.PreviewOnly)
          {
            _effects.RemoveAt(i);
            effect.Dispose();
          }
        }
      }
    }

    /// <summary>以指定相机变换渲染全部非预览效果 (可在不同相机下多次调用).</summary>
    public void RenderAll(Matrix transform)
    {
      if (!IsInitialized)
        return;
      lock (_lock)
      {
        for (int i = 0; i < _effects.Count; i++)
        {
          ParticleEffect effect = _effects[i];
          if (effect.PreviewOnly)
            continue;
          effect.Draw(transform);
        }
      }
    }

    /// <summary>一步完成更新与渲染 (引擎挂钩的惯用入口).</summary>
    public void TickAndRender(float dt, Matrix cameraTransform)
    {
      UpdateAll(dt);
      RenderAll(cameraTransform);
    }
  }
}
