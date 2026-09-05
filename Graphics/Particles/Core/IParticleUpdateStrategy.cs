namespace Particle.Core
{
  /// <summary>单帧内单个发射器的模拟参数与生成记录 (供更新策略消费).</summary>
  public sealed class EmitterFrame
  {
    /// <summary>该发射器在效果池中的槽位区间起点.</summary>
    public int RangeStart;
    /// <summary>该发射器的槽位容量.</summary>
    public int Capacity;
    /// <summary>发射器配置 (CPU 策略直接求值曲线用).</summary>
    public EmitterConfig Config;
    /// <summary>本帧生成的粒子记录.</summary>
    public List<ParticleSpawnRecord> Spawns = new List<ParticleSpawnRecord>();
    /// <summary>模拟参数.</summary>
    public ParticleSimParams Params;
  }

  /// <summary>传递给更新策略的单帧模拟数据.</summary>
  public sealed class ParticleSimFrame
  {
    /// <summary>本帧步长 (秒).</summary>
    public float Dt;
    /// <summary>各发射器的帧数据.</summary>
    public List<EmitterFrame> Emitters = new List<EmitterFrame>();
    /// <summary>实例绘制数量上限 (槽位占用终点, 死槽位由着色器退化处理).</summary>
    public int DrawEnd;
  }

  /// <summary>单发射器模拟参数 (与 ComputeSharp 集成着色器的常量一一对应).</summary>
  public struct ParticleSimParams
  {
    /// <summary>帧步长 (秒).</summary>
    public float Dt;
    /// <summary>重力 X (像素/秒²).</summary>
    public float GravityX;
    /// <summary>重力 Y (像素/秒²).</summary>
    public float GravityY;
    /// <summary>线性阻尼 (1/秒).</summary>
    public float Drag;
    /// <summary>槽位区间起点.</summary>
    public int RangeStart;
    /// <summary>槽位区间终点 (排他).</summary>
    public int RangeEnd;
    /// <summary>颜色曲线关键帧数.</summary>
    public int ColorKeyCount;
    /// <summary>透明度曲线关键帧数.</summary>
    public int AlphaKeyCount;
    /// <summary>尺寸曲线关键帧数.</summary>
    public int SizeKeyCount;
    /// <summary>曲线插值模式 (0 线性 / 1 SmoothStep / 2 Catmull-Rom; 与 CPU 求值同源).</summary>
    public int Interpolation;
  }

  /// <summary>
  /// 粒子更新策略 (策略模式): 封装"粒子状态如何推进一帧"的实现.
  /// <br>所有策略统一输出"粒子数据纹理" —— 每粒子占一列
  /// (<see cref="ParticleLayouts.DataRows"/> 行 × RGBA32F, 逐行对应 <see cref="Particle"/> 的 5 个 float4),
  /// 渲染端顶点着色器按槽位 ID 做纹素取样展开四边形 (老 GpuParticle 的纹理粒子思路,
  /// 零拷贝档借助 D3D11↔D3D12 共享纹理 —— 该路径已被 TinterBridge 实战验证).</br>
  /// </summary>
  public interface IParticleUpdateStrategy : IDisposable
  {
    /// <summary>策略名称 (诊断显示).</summary>
    string Name { get; }

    /// <summary>是否为零拷贝 GPU 路径 (粒子数据全程驻留显存, 渲染直接读取).</summary>
    bool IsZeroCopy { get; }

    /// <summary>策略承载的粒子容量.</summary>
    int Capacity { get; }

    /// <summary>初始化缓冲区资源.</summary>
    /// <param name="device">MonoGame 图形设备.</param>
    /// <param name="capacity">粒子容量.</param>
    void Initialize(GraphicsDevice device, int capacity);

    /// <summary>提交本帧模拟数据并推进粒子状态 (CPU 侧同步完成 / GPU 侧异步派发).</summary>
    void Submit(ParticleSimFrame frame);

    /// <summary>取回本帧渲染数据: 粒子数据纹理与实例数量.</summary>
    (Texture2D DataTexture, int InstanceCount) ResolveFrame();
  }
}
