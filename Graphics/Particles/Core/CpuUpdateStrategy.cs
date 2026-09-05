namespace Particle.Core
{
  /// <summary>
  /// CPU 回退更新策略 (策略模式的回退实现): 与 GPU 策略保持完全一致的模拟语义,
  /// 用于 ComputeSharp 设备不可用或共享缓冲区创建失败的场合.
  /// <br>每帧将粒子数组上传至动态顶点缓冲区, 渲染路径与 GPU 策略完全相同.</br>
  /// </summary>
  public sealed class CpuUpdateStrategy : IParticleUpdateStrategy
  {
    private Particle[] _state;
    private DynamicVertexBuffer _instanceBuffer;
    private int _drawEnd;

    public string Name => "CPU 回退";
    public bool IsZeroCopy => false;
    public int Capacity => _state?.Length ?? 0;

    public void Initialize(GraphicsDevice device, int capacity)
    {
      capacity = Math.Max(1, capacity);
      _state = new Particle[capacity];
      _drawEnd = 0;
      _instanceBuffer = new DynamicVertexBuffer(device, ParticleLayouts.InstanceVertexDeclaration, capacity, BufferUsage.WriteOnly);
    }

    public void Submit(ParticleSimFrame frame)
    {
      _drawEnd = Math.Max(_drawEnd, frame.DrawEnd);
      float dt = frame.Dt;

      for (int e = 0; e < frame.Emitters.Count; e++)
      {
        EmitterFrame emitter = frame.Emitters[e];

        // —— 应用本帧生成记录 (与 GPU 生成着色器一致: Age = -1 哨兵) ——
        List<ParticleSpawnRecord> spawns = emitter.Spawns;
        for (int i = 0; i < spawns.Count; i++)
        {
          ParticleSpawnRecord s = spawns[i];
          int slot = (int)s.Slot;
          if (slot < 0 || slot >= _state.Length)
            continue;
          Particle p = default;
          p.PosX = s.PosX;
          p.PosY = s.PosY;
          p.VelX = s.VelX;
          p.VelY = s.VelY;
          p.BaseSize = s.Size;
          p.Rotation = s.Rotation;
          p.Life = s.Life;
          p.Age = -1f;
          p.Seed = s.Seed;
          p.Aspect = s.Aspect;
          p.AngularVel = s.AngularVel;
          p.Tint = s.Tint;
          p.Stretch = s.Stretch;
          _state[slot] = p;
        }

        // —— 积分 (与 ComputeSharp 集成着色器逐步对应) ——
        ParticleSimParams par = emitter.Params;
        EmitterConfig config = emitter.Config;
        float damp = MathF.Exp(-par.Drag * dt);
        for (int slot = par.RangeStart; slot < par.RangeEnd; slot++)
        {
          Particle p = _state[slot];

          if (p.Age < 0f)
          {
            // 本帧新粒子: 由生成记录确定初始状态, 首帧不位移.
            p.Age = dt;
            p.Size = p.BaseSize;
            Vector4 firstColor = config.ColorOverLife.Evaluate(0f);
            p.R = firstColor.X * p.Tint;
            p.G = firstColor.Y * p.Tint;
            p.B = firstColor.Z * p.Tint;
            p.A = config.OpacityOverLife.Evaluate(0f);
            _state[slot] = p;
            continue;
          }
          if (p.Life <= 0f)
          {
            p.Life = -1f;
            _state[slot] = p;
            continue;
          }

          p.Age += dt;
          if (p.Age >= p.Life)
          {
            p.Life = -1f;
            p.A = 0f;
            _state[slot] = p;
            continue;
          }

          p.VelX += par.GravityX * dt;
          p.VelY += par.GravityY * dt;
          p.VelX *= damp;
          p.VelY *= damp;
          p.PosX += p.VelX * dt;
          p.PosY += p.VelY * dt;
          p.Rotation += p.AngularVel * dt;

          float t = p.Age / p.Life;
          Vector4 rgb = config.ColorOverLife.Evaluate(t);
          p.R = rgb.X * p.Tint;
          p.G = rgb.Y * p.Tint;
          p.B = rgb.Z * p.Tint;
          p.A = config.OpacityOverLife.Evaluate(t);
          p.Size = p.BaseSize * config.SizeOverLife.Evaluate(t);

          _state[slot] = p;
        }
      }
    }

    public (VertexBuffer Buffer, int InstanceCount) ResolveFrame()
    {
      if (_instanceBuffer is null)
        return (null, 0);
      int count = Math.Min(_drawEnd, _state.Length);
      if (count > 0)
      {
        // 只上传占用前缀; 动态缓冲区使用 Discard 避免同步等待.
        _instanceBuffer.SetData(_state, 0, count, SetDataOptions.Discard);
      }
      return (_instanceBuffer, count);
    }

    public void Dispose()
    {
      _instanceBuffer?.Dispose();
      _instanceBuffer = null;
      _state = null;
    }
  }
}
