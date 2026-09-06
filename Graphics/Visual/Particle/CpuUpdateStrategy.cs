namespace Colin.Core.Graphics.Visual.Particle
{
  /// <summary>
  /// CPU 回退更新策略 (策略模式的回退实现): 与 GPU 策略保持完全一致的模拟语义,
  /// 用于 ComputeSharp 设备不可用的场合.
  /// <br>每帧将粒子数组打包为数据纹理并上传, 渲染路径与 GPU 策略完全一致
  /// (顶点着色器按槽位 ID 取样).</br>
  /// </summary>
  public sealed class CpuUpdateStrategy : IParticleUpdateStrategy
  {
    private Particle[] _state;
    private Texture2D _dataTexture;
    private Vector4[] _textureData;
    private int _drawEnd;

    public string Name => "CPU 回退";
    public bool IsZeroCopy => false;
    public int Capacity => _state?.Length ?? 0;

    public void Initialize(GraphicsDevice device, int capacity)
    {
      capacity = Math.Max(1, capacity);
      _state = new Particle[capacity];
      _drawEnd = 0;
      _textureData = new Vector4[capacity * ParticleLayouts.DataRows];
      _dataTexture = new Texture2D(device, capacity, ParticleLayouts.DataRows, false, SurfaceFormat.Vector4);
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

    public (Texture2D DataTexture, int InstanceCount) ResolveFrame()
    {
      int count = Math.Min(_drawEnd, _state.Length);
      if (count > 0 && _dataTexture is not null)
      {
        // —— 打包粒子数组 → 数据纹理 (只传占用前缀列) ——
        for (int i = 0; i < count; i++)
        {
          Particle p = _state[i];
          int b = i * ParticleLayouts.DataRows;
          _textureData[b + 0] = new Vector4(p.PosX, p.PosY, p.VelX, p.VelY);
          _textureData[b + 1] = new Vector4(p.R, p.G, p.B, p.A);
          _textureData[b + 2] = new Vector4(p.Size, p.Rotation, p.Age, p.Life);
          _textureData[b + 3] = new Vector4(p.Seed, p.Aspect, p.AngularVel, p.Tint);
          _textureData[b + 4] = new Vector4(p.BaseSize, p.Stretch, p.Reserved2, p.Reserved3);
        }
        _dataTexture.SetData(0, new Rectangle(0, 0, count, ParticleLayouts.DataRows),
          _textureData, 0, count * ParticleLayouts.DataRows);
      }
      return (_dataTexture, count);
    }

    public void Dispose()
    {
      _dataTexture?.Dispose();
      _dataTexture = null;
      _state = null;
      _textureData = null;
    }
  }
}
