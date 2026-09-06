namespace Colin.Core.Graphics.Visual.Particle.Slash
{
  /// <summary>
  /// 刀光效果实例 (独立大功能): 拉刀光 Mesh 本体 (<see cref="SlashArc"/>) + 与前缘角度绑定的刃花粒子层.
  /// <br>刃花不再是发射器里一份"碰巧同步"的静态弧形发射 —— 每帧由本效果取弧的<b>当前前缘角度</b>,
  /// 经与 Mesh 完全相同的整体坐标系 (椭圆缩放/旋转/位置) 求前缘坐标与切向, 直接驱动发射,
  /// 因此收起方式、扫速、坐标系怎么改, 刃花始终生在刃头上.</br>
  /// <br>提供 <see cref="Play"/> / <see cref="Pause"/> / <see cref="Stop"/> / <see cref="Reset"/>
  /// 完整生命周期; 刃花粒子经内部 <see cref="Colin.Core.Graphics.Visual.Particle.ParticleEffect"/> 承载
  /// (不注册进 ParticleManager, 由本效果统一驱动, 避免双重更新).</br>
  /// </summary>
  public sealed class SlashEffect : IDisposable
  {
    /// <summary>效果配置 (编辑器可直接修改并 NotifyChanged).</summary>
    public SlashEffectConfig Config;

    /// <summary>刀光 Mesh 本体.</summary>
    public SlashArc Arc;

    /// <summary>世界位置 (弧心).</summary>
    public Vector2 Position;
    /// <summary>整体旋转 (弧度, 叠加在配置角度上).</summary>
    public float Rotation;
    /// <summary>整体缩放.</summary>
    public float Scale = 1f;

    /// <summary>是否正在播放.</summary>
    public bool IsPlaying;
    /// <summary>是否暂停.</summary>
    public bool IsPaused;
    /// <summary>是否已挥砍完毕 (非循环; 刃花仍会存续至消亡).</summary>
    public bool IsFinished;

    /// <summary>效果累计时间 (秒, 诊断).</summary>
    public float Time;

    /// <summary>是否可回收 (挥砍完毕且刃花余焰烧完) —— 管理器据此自动清理.</summary>
    public bool CanRecycle => IsFinished && _afterGlow > Config.Sparks.LifeMax + 0.25f;

    /// <summary>当前存活刃花实例数 (诊断).</summary>
    public int SparkCount => _sparks?.InstanceCount ?? 0;

    /// <summary>效果实例标识 (诊断与日志).</summary>
    public int Id { get; } = _nextId++;
    private static int _nextId = 1;

    private Colin.Core.Graphics.Visual.Particle.ParticleEffect _sparks;
    private Colin.Core.Graphics.Visual.Particle.EmitterConfig _sparkEmitter;
    private float _restElapsed;
    private float _sparkCarry;
    private float _afterGlow;
    private readonly List<Colin.Core.Graphics.Visual.Particle.ParticleSpawnInit> _spawnBatch = new List<Colin.Core.Graphics.Visual.Particle.ParticleSpawnInit>(32);
    private Random _random = new Random();

    /// <summary>由管理器或用户创建; 图形设备缺省取粒子管理器/引擎设备.
    /// <br>预制件自带的嵌入纹理 ("embed:键") 在此统一注册 —— 调用侧无需任何额外准备.</br>
    /// </summary>
    public SlashEffect(SlashEffectConfig config, GraphicsDevice device = null)
    {
      Config = config ?? throw new ArgumentNullException(nameof(config));
      device ??= Colin.Core.Graphics.Visual.Particle.ParticleManager.Instance.IsInitialized
        ? Colin.Core.Graphics.Visual.Particle.ParticleManager.Instance.GraphicsDevice
        : CoreInfo.Graphics.GraphicsDevice;

      RegisterEmbeddedTextures(device);
      Arc = new SlashArc(Config.Arc);
      BuildSparks(device);
      Config.Subscribe(OnConfigChanged);
      IsPlaying = true;
    }

    /// <summary>把配置里的嵌入纹理 (PNG base64) 解码注册进共享渲染器缓存 (按 "embed:键" 命名, 幂等).</summary>
    private void RegisterEmbeddedTextures(GraphicsDevice device)
    {
      Colin.Core.Graphics.Visual.Particle.Rendering.ParticleRenderer renderer = Colin.Core.Graphics.Visual.Particle.Rendering.ParticleRenderer.Shared;
      if (renderer is null)
      {
        if (!Colin.Core.Graphics.Visual.Particle.ParticleManager.Instance.IsInitialized)
          Colin.Core.Graphics.Visual.Particle.ParticleManager.Instance.Initialize(device);
        renderer = Colin.Core.Graphics.Visual.Particle.Rendering.ParticleRenderer.Shared;
      }
      if (renderer is null)
        return;

      foreach (KeyValuePair<string, string> pair in ConfigEmbeddedTextures())
      {
        if (string.IsNullOrEmpty(pair.Value))
          continue;
        try
        {
          renderer.LoadTextureBytes("embed:" + pair.Key, Convert.FromBase64String(pair.Value));
        }
        catch (Exception exception)
        {
          Console.WriteLine("Error", $"嵌入纹理解码失败 ({pair.Key}): {exception.Message}");
        }
      }
    }

    /// <summary>嵌入纹理表快照 (避免遍历时与编辑修改竞争).</summary>
    private List<KeyValuePair<string, string>> ConfigEmbeddedTextures() =>
      Config.EmbeddedTextures is null
        ? new List<KeyValuePair<string, string>>()
        : Config.EmbeddedTextures.ToList();

    /// <summary>播放 (恢复).</summary>
    public void Play() { IsPlaying = true; IsPaused = false; IsFinished = false; }

    /// <summary>暂停.</summary>
    public void Pause() => IsPaused = true;

    /// <summary>停止: 复位 Mesh 与刃花, 等待下一次播放.</summary>
    public void Stop()
    {
      IsPlaying = false;
      IsFinished = true;
      Arc.Reset();
      _sparks?.Reset();
      _sparkCarry = 0f;
      _restElapsed = 0f;
      _afterGlow = 0f;
    }

    /// <summary>重置到挥出起点并立即播放 (随机种子可固定).</summary>
    public void Reset(int? seed = null)
    {
      Time = 0f;
      _restElapsed = 0f;
      _sparkCarry = 0f;
      _afterGlow = 0f;
      IsFinished = false;
      IsPaused = false;
      IsPlaying = true;
      Arc.Reset();
      int usedSeed = seed ?? (Config.Seed != 0 ? Config.Seed : _random.Next());
      _random = new Random(usedSeed);
      _sparks?.Reset(usedSeed);
    }

    /// <summary>推进效果一帧 (Mesh 动画 + 刃花发射与模拟). 由管理器/编辑器/引擎挂钩驱动.</summary>
    public void Update(float dt)
    {
      if (!IsPlaying || IsPaused)
        return;

      Time += dt;
      Arc.Position = Position;
      Arc.Rotation = Rotation;
      Arc.Scale = Scale;

      if (!Arc.IsFinished)
      {
        Arc.Update(dt);
      }
      else if (Config.Looping)
      {
        // —— 循环挥砍: 休止 RestTime 后重新挥出 ——
        _restElapsed += dt;
        if (_restElapsed >= MathF.Max(0f, Config.RestTime))
        {
          Arc.Reset();
          _restElapsed = 0f;
        }
      }

      if (!Config.Looping && Arc.IsFinished)
      {
        IsFinished = true;
        _afterGlow += dt;   // 刃花余焰计时 (回收判定).
      }
      else
      {
        IsFinished = false;
        _afterGlow = 0f;
      }

      EmitSparks(dt);
      _sparks?.Update(dt);
    }

    /// <summary>绘制本效果 (刃花粒子 + 刀光 Mesh, 同一相机变换).</summary>
    public void Draw(Matrix transform)
    {
      _sparks?.Draw(transform);
      SlashRenderer.GetOrCreate().DrawOne(Arc, transform);
    }

    // =====================================================================
    //  刃花: 与前缘角度绑定
    // =====================================================================

    /// <summary>沿当前前缘喷射刃花 —— 位置/切向取自弧的实时状态.</summary>
    private void EmitSparks(float dt)
    {
      if (_sparks is null || !Config.Sparks.Enabled || !Arc.IsSweeping)
      {
        _sparkCarry = 0f;
        return;
      }

      SlashSparkConfig sparks = Config.Sparks;
      _sparkCarry += MathF.Max(0f, sparks.Rate) * dt;
      if (_sparkCarry < 1f)
        return;

      float headDeg = Arc.HeadAngleDeg;
      Vector2 head = Arc.PointAt(headDeg);
      Vector2 tangent = Arc.TangentAt(headDeg);       // 指向扫进方向.
      Vector2 normal = new Vector2(-tangent.Y, tangent.X);

      _spawnBatch.Clear();
      while (_sparkCarry >= 1f)
      {
        _sparkCarry -= 1f;

        // 速度: 前缘切向 × 随机速率, 张角 ±SpreadDeg 散布.
        float speed = RandRange(sparks.SpeedMin, sparks.SpeedMax);
        float spread = RandRange(-sparks.SpreadDeg, sparks.SpreadDeg) * MathF.PI / 180f;
        float cos = MathF.Cos(spread), sin = MathF.Sin(spread);
        Vector2 direction = new Vector2(tangent.X * cos - tangent.Y * sin, tangent.X * sin + tangent.Y * cos);

        // 位置: 前缘点 ± 4px 法向散布 (避免完全重叠成一条线).
        Vector2 position = head + normal * RandRange(-4f, 4f);

        _spawnBatch.Add(new Colin.Core.Graphics.Visual.Particle.ParticleSpawnInit
        {
          Position = position,
          Velocity = direction * speed,
          Size = RandRange(sparks.SizeMin, sparks.SizeMax),
          Life = RandRange(sparks.LifeMin, sparks.LifeMax),
          Aspect = sparks.Aspect,
          Rotation = MathF.Atan2(direction.Y, direction.X),
          AngularVel = 0f,
          Tint = RandRange(0.85f, 1.15f),
          Stretch = sparks.Stretched ? 1f : 0f
        });
      }

      _sparks.EmitCustom(_spawnBatch, 0);
    }

    /// <summary>依据刃花配置构建内部粒子效果 (不注册进 ParticleManager).</summary>
    private void BuildSparks(GraphicsDevice device)
    {
      SlashSparkConfig sparks = Config.Sparks;
      _sparkEmitter = new Colin.Core.Graphics.Visual.Particle.EmitterConfig
      {
        Name = "刃花",
        Capacity = Math.Max(1, sparks.Capacity),
        EmissionRate = 0f,                        // 全部经 EmitCustom 驱动 (与前缘绑定).
        Shape = new Colin.Core.Graphics.Visual.Particle.EmissionShapeConfig(),
        SpeedMin = 0f,
        SpeedMax = 0f,
        LifeMin = sparks.LifeMin,
        LifeMax = sparks.LifeMax,
        SizeMin = sparks.SizeMin,
        SizeMax = sparks.SizeMax,
        Aspect = sparks.Aspect,
        Gravity = sparks.Gravity,
        Drag = sparks.Drag,
        StretchedBillboard = sparks.Stretched
      };
      _sparkEmitter.Subscribe(_ => _sparks?.MarkRebuildRequired());

      Colin.Core.Graphics.Visual.Particle.ParticleEffectConfig effectConfig = new Colin.Core.Graphics.Visual.Particle.ParticleEffectConfig
      {
        Name = Config.Name + "·刃花",
        Duration = 3600f,
        Looping = true,                           // 永不自动完成, 由本效果统一停止.
        Emitters = { _sparkEmitter },
        Render = new Colin.Core.Graphics.Visual.Particle.RenderConfig
        {
          Blend = Colin.Core.Graphics.Visual.Particle.ParticleBlendMode.Additive,
          Texture = sparks.Texture,
          StretchFactor = 0.05f,
          MaxStretchLength = 200f
        }
      };
      _sparks = new Colin.Core.Graphics.Visual.Particle.ParticleEffect(effectConfig, device);
      SyncSparkEmitter();
    }

    /// <summary>把刃花配置同步到发射器曲线/渲染参数 (配置变更时调用).</summary>
    private void SyncSparkEmitter()
    {
      if (_sparkEmitter is null)
        return;
      SlashSparkConfig sparks = Config.Sparks;
      _sparkEmitter.Capacity = Math.Max(1, sparks.Capacity);
      _sparkEmitter.LifeMin = sparks.LifeMin;
      _sparkEmitter.LifeMax = sparks.LifeMax;
      _sparkEmitter.SizeMin = sparks.SizeMin;
      _sparkEmitter.SizeMax = sparks.SizeMax;
      _sparkEmitter.Aspect = sparks.Aspect;
      _sparkEmitter.Gravity = sparks.Gravity;
      _sparkEmitter.Drag = sparks.Drag;
      _sparkEmitter.StretchedBillboard = sparks.Stretched;
      _sparkEmitter.ColorOverLife.Keys.Clear();
      _sparkEmitter.ColorOverLife.Keys.Add(new Colin.Core.Graphics.Visual.Particle.ColorKey(0f, sparks.StartColor));
      _sparkEmitter.ColorOverLife.Keys.Add(new Colin.Core.Graphics.Visual.Particle.ColorKey(1f, sparks.EndColor));
      if (_sparks is not null)
        _sparks.Config.Render.Texture = sparks.Texture;
      _sparkEmitter.NotifyChanged();   // 曲线版本自增 → GPU 曲线缓冲刷新; 容量变化走结构重建.
    }

    private void OnConfigChanged(SlashEffectConfig config) => SyncSparkEmitter();

    private float RandRange(float min, float max) => min + (max - min) * (float)_random.NextDouble();

    public void Dispose()
    {
      Config.Unsubscribe(OnConfigChanged);
      _sparks?.Dispose();
      _sparks = null;
      _sparkEmitter = null;
    }
  }
}
