using System.Runtime.InteropServices;

namespace Colin.Core.Graphics.Visual.Particle
{
  /// <summary>
  /// GPU/CPU 通用的单个粒子状态 (80 字节, 5 × float4, 16 字节对齐).
  /// <br>该结构同时作为 ComputeSharp 缓冲区元素类型与 MonoGame 实例化绘制的顶点流布局,
  /// 字段顺序与 <see cref="InstanceVertexDeclaration"/> 逐字节对应, 修改时必须同步着色器.</br>
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct Particle
  {
    /// <summary>POSITION0: 位置.xy 与 速度.xy (像素/秒).</summary>
    public float PosX, PosY, VelX, VelY;
    /// <summary>COLOR0: 渲染颜色 rgba (由生命周期曲线求值得出).</summary>
    public float R, G, B, A;
    /// <summary>TEXCOORD0: 尺寸(像素), 旋转(弧度), 年龄(秒), 总生命(秒; ≤0 表示已死亡).</summary>
    public float Size, Rotation, Age, Life;
    /// <summary>TEXCOORD1: 随机种子(0~1), 长宽比, 角速度(弧度/秒), 色调系数.</summary>
    public float Seed, Aspect, AngularVel, Tint;
    /// <summary>TEXCOORD2: 基准尺寸(像素, 尺寸曲线的乘算基数), 拉伸模式(0=公告牌, 1=速度拉伸), 保留字段.</summary>
    public float BaseSize, Stretch, Reserved2, Reserved3;

    /// <summary>粒子是否处于存活状态.</summary>
    public readonly bool IsAlive => Life > 0f && Age < Life;
  }

  /// <summary>
  /// 单个粒子的生成记录 (64 字节, 4 × float4).
  /// <br>由 CPU 侧采样发射形状后构建, 经 GPU 缓冲区传入 ComputeSharp 生成着色器;
  /// 颜色不在此处指定 —— 渲染颜色一律由生命周期曲线求值.</br>
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct ParticleSpawnRecord
  {
    /// <summary>0: 初始位置.xy 与 初始速度.xy.</summary>
    public float PosX, PosY, VelX, VelY;
    /// <summary>1: 基准尺寸, 初始旋转(弧度), 总生命(秒), 槽位索引(取整后使用).</summary>
    public float Size, Rotation, Life, Slot;
    /// <summary>2: 随机种子, 长宽比, 角速度, 色调系数.</summary>
    public float Seed, Aspect, AngularVel, Tint;
    /// <summary>3: 拉伸模式(0/1), 保留对齐字段.</summary>
    public float Stretch, Pad1, Pad2, Pad3;
  }

  /// <summary>
  /// 粒子数据纹理的布局常量与渲染端声明.
  /// <br>每粒子占一列纹素 (宽 = 容量), <see cref="DataRows"/> 行 × RGBA32F,
  /// 逐行对应 <see cref="Particle"/> 的 5 个 float4; 渲染端实例流只携带槽位 ID,
  /// 顶点着色器按 ID 做纹素取样.</br>
  /// </summary>
  public static class ParticleLayouts
  {
    /// <summary>数据纹理行数 (每粒子 5 × float4).</summary>
    public const int DataRows = 5;

    /// <summary>ID 实例流顶点声明 (每实例一个槽位 ID).</summary>
    public static readonly VertexDeclaration IdInstanceVertexDeclaration = new VertexDeclaration(
      new VertexElement(0, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1));
  }
}
