using ComputeSharp;

namespace Particle.Compute
{
  using Particle.Core;
  /// <summary>
  /// 粒子生成着色器: 将 CPU 采样好的生成记录写入粒子缓冲区的环形槽位.
  /// <br>Age = -1 作为"本帧新生"哨兵, 由集成着色器识别并归一化.</br>
  /// </summary>
  [ThreadGroupSize(DefaultThreadGroupSizes.X)]
  [GeneratedComputeShaderDescriptor]
  public readonly partial struct ParticleSpawnShader(
      ReadWriteBuffer<Particle> current,
      ReadOnlyBuffer<ParticleSpawnRecord> spawns,
      int count) : IComputeShader
  {
    public void Execute()
    {
      int i = ThreadIds.X;
      if (i >= count)
        return;

      ParticleSpawnRecord record = spawns[i];

      Particle particle = default;
      particle.PosX = record.PosX;
      particle.PosY = record.PosY;
      particle.VelX = record.VelX;
      particle.VelY = record.VelY;
      particle.R = 0f;
      particle.G = 0f;
      particle.B = 0f;
      particle.A = 0f;
      particle.Size = record.Size;
      particle.Rotation = record.Rotation;
      particle.Age = -1f;
      particle.Life = record.Life;
      particle.Seed = record.Seed;
      particle.Aspect = record.Aspect;
      particle.AngularVel = record.AngularVel;
      particle.Tint = record.Tint;
      particle.BaseSize = record.Size;
      particle.Stretch = record.Stretch;

      current[(int)record.Slot] = particle;
    }
  }

  /// <summary>
  /// 粒子集成着色器 (核心模拟): 对每个槽位推进生命、积分运动、按曲线重算外观.
  /// <br>奇偶双缓冲: 读 <paramref name="previous"/> (上一帧状态) 与 <paramref name="current"/>
  /// (本帧新生成记录), 写 <paramref name="current"/>; D3D11 渲染端始终读取上一帧结果, 两侧无写冲突.
  /// 两块缓冲区均以 RW 视图绑定 (RW 缓冲区可读, 避开 SRV 描述符路径).</br>
  /// </summary>
  [ThreadGroupSize(DefaultThreadGroupSizes.X)]
  [GeneratedComputeShaderDescriptor]
  public readonly partial struct ParticleIntegrateShader(
      ReadWriteBuffer<Particle> current,
      ReadWriteBuffer<Particle> previous,
      ReadOnlyBuffer<float4> curveKeys,
      float dt,
      float gravityX,
      float gravityY,
      float drag,
      int rangeStart,
      int colorKeyCount,
      int alphaKeyCount,
      int sizeKeyCount) : IComputeShader
  {
    public void Execute()
    {
      int slot = ThreadIds.X + rangeStart;

      Particle currentParticle = current[slot];

      // —— 本帧新粒子: 归一化年龄并给出 t=0 外观 ——
      if (currentParticle.Age < 0f)
      {
        currentParticle.Age = dt;
        currentParticle.Size = currentParticle.BaseSize;
        float4 firstColor = EvalCurve(0, colorKeyCount, 0f);
        float4 firstAlpha = EvalCurve(colorKeyCount, alphaKeyCount, 0f);
        currentParticle.R = firstColor.Y * currentParticle.Tint;
        currentParticle.G = firstColor.Z * currentParticle.Tint;
        currentParticle.B = firstColor.W * currentParticle.Tint;
        currentParticle.A = firstAlpha.Y;
        current[slot] = currentParticle;
        return;
      }

      // —— 上一帧状态 (奇偶缓冲的读取端) ——
      Particle p = previous[slot];

      if (p.Life <= 0f)
      {
        currentParticle.Life = -1f;
        currentParticle.A = 0f;
        current[slot] = currentParticle;
        return;
      }

      p.Age += dt;
      if (p.Age >= p.Life)
      {
        // 死亡: 标记后渲染端按退化处理.
        p.Life = -1f;
        p.A = 0f;
        current[slot] = p;
        return;
      }

      // —— 运动: 重力 + 线性阻尼 + 位移 ——
      p.VelX += gravityX * dt;
      p.VelY += gravityY * dt;
      float damp = Hlsl.Exp(-drag * dt);
      p.VelX *= damp;
      p.VelY *= damp;
      p.PosX += p.VelX * dt;
      p.PosY += p.VelY * dt;
      p.Rotation += p.AngularVel * dt;

      // —— 外观: 生命周期曲线求值 (颜色 rgb / 透明度 / 尺寸倍率) ——
      float t = p.Age / p.Life;
      float4 color = EvalCurve(0, colorKeyCount, t);
      float4 alpha = EvalCurve(colorKeyCount, alphaKeyCount, t);
      float4 sizeMul = EvalCurve(colorKeyCount + alphaKeyCount, sizeKeyCount, t);
      p.R = color.Y * p.Tint;
      p.G = color.Z * p.Tint;
      p.B = color.W * p.Tint;
      p.A = alpha.Y;
      p.Size = p.BaseSize * sizeMul.Y;

      current[slot] = p;
    }

    /// <summary>
    /// GPU 侧曲线求值 (实例方法, 读取 curveKeys 字段 —— ComputeSharp 不允许缓冲区类型作为方法参数):
    /// 关键帧格式为 (t, v0, v1, v2), 线性插值, 区间外取端点.
    /// </summary>
    private readonly float4 EvalCurve(int offset, int count, float t)
    {
      if (count <= 0)
        return new float4(1f, 1f, 1f, 1f);
      float4 first = curveKeys[offset];
      if (count == 1 || t <= first.X)
        return first;
      for (int k = 0; k < count - 1; k++)
      {
        float4 a = curveKeys[offset + k];
        float4 b = curveKeys[offset + k + 1];
        if (t <= b.X)
        {
          float f = (t - a.X) / Hlsl.Max(b.X - a.X, 1e-6f);
          return a + (b - a) * f;
        }
      }
      return curveKeys[offset + count - 1];
    }
  }

  /// <summary>粒子缓冲区清零着色器 (初始化时将两个奇偶缓冲区全部置为死亡态).</summary>
  [ThreadGroupSize(DefaultThreadGroupSizes.X)]
  [GeneratedComputeShaderDescriptor]
  public readonly partial struct ParticleClearShader(
      ReadWriteBuffer<Particle> current) : IComputeShader
  {
    public void Execute()
    {
      current[ThreadIds.X] = default(Particle);
    }
  }
}
