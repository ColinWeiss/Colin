using ComputeSharp;
using Particle.Core;

namespace Colin.Core.Graphics.Visual.Particle
{
  /// <summary>
  /// 粒子生成着色器 (纹理粒子): 将 CPU 采样好的生成记录写入数据纹理的对应列
  /// (每粒子一列 × <see cref="ParticleLayouts.DataRows"/> 行纹素).
  /// <br>Age = -1 作为"本帧新生"哨兵, 由集成着色器识别并归一化.</br>
  /// </summary>
  [ThreadGroupSize(DefaultThreadGroupSizes.X)]
  [GeneratedComputeShaderDescriptor]
  public readonly partial struct ParticleSpawnShader(
      ReadWriteTexture2D<float4> current,
      ReadOnlyBuffer<ParticleSpawnRecord> spawns,
      int count) : IComputeShader
  {
    public void Execute()
    {
      int i = ThreadIds.X;
      if (i >= count)
        return;

      ParticleSpawnRecord record = spawns[i];
      int slot = (int)record.Slot;

      current[slot, 0] = new float4(record.PosX, record.PosY, record.VelX, record.VelY);
      current[slot, 1] = new float4(0f, 0f, 0f, 0f);                                    // 颜色由曲线求值.
      current[slot, 2] = new float4(record.Size, record.Rotation, -1f, record.Life);    // Age = -1 哨兵.
      current[slot, 3] = new float4(record.Seed, record.Aspect, record.AngularVel, record.Tint);
      current[slot, 4] = new float4(record.Size, record.Stretch, 0f, 0f);               // BaseSize = Size.
    }
  }

  /// <summary>
  /// 粒子集成着色器 (核心模拟, 纹理粒子版): 对每一列推进生命、积分运动、按曲线重算外观.
  /// <br>奇偶双缓冲: 读 <paramref name="previous"/> (上一帧状态) 与 <paramref name="current"/>
  /// (本帧新生成记录), 写 <paramref name="current"/>; D3D11 渲染端始终读取上一帧结果, 两侧无写冲突.</br>
  /// </summary>
  [ThreadGroupSize(DefaultThreadGroupSizes.X)]
  [GeneratedComputeShaderDescriptor]
  public readonly partial struct ParticleIntegrateShader(
      ReadWriteTexture2D<float4> current,
      ReadWriteTexture2D<float4> previous,
      ReadOnlyBuffer<float4> curveKeys,
      float dt,
      float gravityX,
      float gravityY,
      float drag,
      int rangeStart,
      int colorKeyCount,
      int alphaKeyCount,
      int sizeKeyCount,
      int interpolation) : IComputeShader
  {
    public void Execute()
    {
      int slot = ThreadIds.X + rangeStart;

      float4 head = current[slot, 0];   // Pos.xy, Vel.xy.
      float4 life = current[slot, 2];   // Size, Rotation, Age, Life.

      // —— 本帧新粒子: 归一化年龄并给出 t=0 外观 ——
      if (life.Z < 0f)
      {
        float4 color0 = EvalCurve(0, colorKeyCount, 0f, interpolation);
        float4 alpha0 = EvalCurve(colorKeyCount, alphaKeyCount, 0f, interpolation);
        float4 misc = current[slot, 3];
        float4 extra = current[slot, 4];

        current[slot, 0] = head;
        current[slot, 1] = new float4(
          color0.Y * misc.W,
          color0.Z * misc.W,
          color0.W * misc.W,
          alpha0.Y);
        current[slot, 2] = new float4(extra.X, life.Y, dt, life.W);
        current[slot, 3] = misc;
        current[slot, 4] = extra;
        return;
      }

      // —— 上一帧状态 (奇偶缓冲的读取端) ——
      float4 p0 = previous[slot, 0];
      float4 p2 = previous[slot, 2];
      float4 p3 = previous[slot, 3];
      float4 p4 = previous[slot, 4];

      if (p2.W <= 0f)
      {
        // 已死亡: 标记后渲染端按退化处理.
        current[slot, 2] = new float4(0f, 0f, 0f, -1f);
        current[slot, 1] = new float4(0f, 0f, 0f, 0f);
        return;
      }

      float age = p2.Z + dt;
      if (age >= p2.W)
      {
        current[slot, 2] = new float4(0f, 0f, age, -1f);
        current[slot, 1] = new float4(0f, 0f, 0f, 0f);
        return;
      }

      // —— 运动: 重力 + 线性阻尼 + 位移 ——
      float damp = Hlsl.Exp(-drag * dt);
      float velX = (p0.Z + gravityX * dt) * damp;
      float velY = (p0.W + gravityY * dt) * damp;
      float posX = p0.X + velX * dt;
      float posY = p0.Y + velY * dt;
      float rotation = p2.Y + p3.Z * dt;

      // —— 外观: 生命周期曲线求值 (颜色 rgb / 透明度 / 尺寸倍率) ——
      float t = age / p2.W;
      float4 color = EvalCurve(0, colorKeyCount, t, interpolation);
      float4 alpha = EvalCurve(colorKeyCount, alphaKeyCount, t, interpolation);
      float4 sizeMul = EvalCurve(colorKeyCount + alphaKeyCount, sizeKeyCount, t, interpolation);

      current[slot, 0] = new float4(posX, posY, velX, velY);
      current[slot, 1] = new float4(color.Y * p3.W, color.Z * p3.W, color.W * p3.W, alpha.Y);
      current[slot, 2] = new float4(p4.X * sizeMul.Y, rotation, age, p2.W);
      current[slot, 3] = p3;
      current[slot, 4] = p4;
    }

    /// <summary>
    /// GPU 侧曲线求值 (实例方法, 读取 curveKeys 字段 —— ComputeSharp 不允许缓冲区类型作为方法参数):
    /// 关键帧格式为 (t, v0, v1, v2); 支持 线性/SmoothStep/Catmull-Rom, 与 CPU 公式同源.
    /// </summary>
    private readonly float4 EvalCurve(int offset, int count, float t, int interpolation)
    {
      if (count <= 0)
        return new float4(1f, 1f, 1f, 1f);
      float4 first = curveKeys[offset];
      if (count == 1 || t <= first.X)
        return first;
      if (t >= curveKeys[offset + count - 1].X)
        return curveKeys[offset + count - 1];

      int k = 0;
      for (int i = 0; i < count - 1; i++)
      {
        if (t < curveKeys[offset + i + 1].X)
        {
          k = i;
          break;
        }
      }
      float4 a = curveKeys[offset + k];
      float4 b = curveKeys[offset + k + 1];
      float f = (t - a.X) / Hlsl.Max(b.X - a.X, 1e-6f);

      if (interpolation == 1)
      {
        f = f * f * (3f - 2f * f);
        return a + (b - a) * f;
      }
      if (interpolation == 2)
      {
        // Catmull-Rom: 端点延拓, 对值分量插值 (X 分量为时间, 结果不使用).
        float4 p0 = curveKeys[offset + Hlsl.Max(k - 1, 0)];
        float4 p3 = curveKeys[offset + Hlsl.Min(k + 2, count - 1)];
        return CatmullRom(p0, a, b, p3, f);
      }
      return a + (b - a) * f;
    }

    /// <summary>Catmull-Rom 样条 (与 CPU CurveMath.CatmullRom 同源).</summary>
    private static float4 CatmullRom(float4 p0, float4 p1, float4 p2, float4 p3, float f)
    {
      float f2 = f * f;
      float f3 = f2 * f;
      return 0.5f * ((2f * p1)
        + (-p0 + p2) * f
        + (2f * p0 - 5f * p1 + 4f * p2 - p3) * f2
        + (-p0 + 3f * p1 - 3f * p2 + p3) * f3);
    }
  }

  /// <summary>粒子数据纹理清零着色器 (初始化时将两个奇偶纹理全部置为死亡态).</summary>
  [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
  [GeneratedComputeShaderDescriptor]
  public readonly partial struct ParticleClearShader(
      ReadWriteTexture2D<float4> current) : IComputeShader
  {
    public void Execute()
    {
      current[ThreadIds.X, ThreadIds.Y] = default;
    }
  }
}
