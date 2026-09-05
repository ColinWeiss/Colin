using System;
using Colin.Core.Graphics.Bridge;
using ComputeSharp;
using CsGraphicsDevice = ComputeSharp.GraphicsDevice;
using GraphicsDevice = Microsoft.Xna.Framework.Graphics.GraphicsDevice;

namespace Particle.Compute
{
  /// <summary>
  /// 一个 GPU 缓冲区同时对三方可见, 零拷贝:
  /// <br>- MonoGame: 一个 VertexBuffer 包装器 (native 已替换为共享缓冲区, 作为实例化绘制的实例流);</br>
  /// <br>- ComputeSharp: 两个 ReadWriteBuffer 包装器 (写视图 UAV / 读视图 UAV —— RW 缓冲区同样可读,
  /// 全程只依赖经实战验证的 UAV 描述符路径);</br>
  /// <br>- 三者背后是同一块在 MonoGame 的 D3D11 设备中创建 (Shared) 的显存, 在 ComputeSharp 的
  /// D3D12 设备上各打开两次 (读写视图独立接口, 释放时引用平衡).</br>
  /// <br>D3D12 侧缓冲区始终处于 COMMON 态 (隐式提升/衰减), ComputeSharp 对缓冲区不插入屏障;
  /// 两侧以 ComputeSharp 完成栅栏同步 (见 GpuUpdateStrategy).</br>
  /// </summary>
  public sealed unsafe class SharedParticleBuffer<T> : IDisposable where T : unmanaged
  {
    /// <summary>最近一次共享缓冲区创建所命中的候选组合描述 (诊断用).</summary>
    public static string LastCreationMode { get; private set; } = "未创建";

    /// <summary>MonoGame 侧实例顶点缓冲 (native 为共享 D3D11 缓冲, 所有权移交于此).</summary>
    public VertexBuffer InstanceBuffer { get; }

    /// <summary>ComputeSharp 侧写视图 (生成/集成着色器写入).</summary>
    public ReadWriteBuffer<T> WriteView { get; }

    /// <summary>ComputeSharp 侧读视图 (集成着色器读取上一帧状态; 仅读取, 不写入).</summary>
    public ReadWriteBuffer<T> ReadView { get; }

    public int Capacity { get; }

    private SharedParticleBuffer(VertexBuffer instanceBuffer, ReadWriteBuffer<T> writeView, ReadWriteBuffer<T> readView, int capacity)
    {
      InstanceBuffer = instanceBuffer;
      WriteView = writeView;
      ReadView = readView;
      Capacity = capacity;
    }

    /// <summary>
    /// 创建共享缓冲区; 任何一步失败都会抛出异常 (由 GpuUpdateStrategy 捕获并回退 CPU).
    /// </summary>
    /// <param name="mgd">MonoGame 图形设备 (D3D11).</param>
    /// <param name="csDevice">ComputeSharp 图形设备 (D3D12).</param>
    /// <param name="d3d12Device">原始 ID3D12Device 指针.</param>
    /// <param name="capacity">元素容量.</param>
    /// <param name="declaration">MonoGame 实例流顶点声明 (须与 T 布局一致).</param>
    public static SharedParticleBuffer<T> Create(
        GraphicsDevice mgd,
        CsGraphicsDevice csDevice,
        void* d3d12Device,
        int capacity,
        VertexDeclaration declaration)
    {
      int elementSize = sizeof(T);
      int sizeInBytes = elementSize * capacity;

      // 1. 普通 ComputeSharp 缓冲区: 提供已注册的包装器对象 (随后替换其原生资源).
      ReadWriteBuffer<T> csWrite = csDevice.AllocateReadWriteBuffer<T>(capacity);
      ReadWriteBuffer<T> csRead = csDevice.AllocateReadWriteBuffer<T>(capacity);

      // 2. 在 MonoGame 的 D3D11 设备中创建共享缓冲区: 不同驱动对标志组合的接受度不同,
      //    按候选序列探测 (NT 共享 → 传统共享; 仅 VB → 全绑定标志), 首个成功者生效.
      SharpDX.Direct3D11.Device d3d11 = (SharpDX.Direct3D11.Device)mgd.Handle;
      SharpDX.Direct3D11.Buffer d3d11Buffer = null;
      IntPtr sharedHandle = IntPtr.Zero;
      Exception lastError = null;

      // 仅尝试 NT 共享: 传统 (非 NT) 共享句柄虽能被 D3D12 打开, 但首次 GPU 使用会引发设备移除 (实测 AMD 驱动).
      // 第三候选为结构化缓冲变体 (部分驱动只对 StructureByteStride > 0 的缓冲放行 NT 共享).
      (SharpDX.Direct3D11.ResourceOptionFlags optionFlags, SharpDX.Direct3D11.BindFlags bindFlags, int structureStride)[] candidates =
      {
        (SharpDX.Direct3D11.ResourceOptionFlags.Shared | SharpDX.Direct3D11.ResourceOptionFlags.SharedNthandle,
         SharpDX.Direct3D11.BindFlags.VertexBuffer, 0),
        (SharpDX.Direct3D11.ResourceOptionFlags.Shared | SharpDX.Direct3D11.ResourceOptionFlags.SharedNthandle,
         SharpDX.Direct3D11.BindFlags.VertexBuffer | SharpDX.Direct3D11.BindFlags.ShaderResource | SharpDX.Direct3D11.BindFlags.UnorderedAccess, 0),
        (SharpDX.Direct3D11.ResourceOptionFlags.Shared | SharpDX.Direct3D11.ResourceOptionFlags.SharedNthandle
         | SharpDX.Direct3D11.ResourceOptionFlags.BufferStructured,
         SharpDX.Direct3D11.BindFlags.VertexBuffer | SharpDX.Direct3D11.BindFlags.ShaderResource | SharpDX.Direct3D11.BindFlags.UnorderedAccess, elementSize)
      };

      foreach ((SharpDX.Direct3D11.ResourceOptionFlags optionFlags, SharpDX.Direct3D11.BindFlags bindFlags, int structureStride) in candidates)
      {
        try
        {
          var description = new SharpDX.Direct3D11.BufferDescription
          {
            SizeInBytes = sizeInBytes,
            Usage = SharpDX.Direct3D11.ResourceUsage.Default,
            BindFlags = bindFlags,
            CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
            OptionFlags = optionFlags,
            StructureByteStride = structureStride
          };
          SharpDX.Direct3D11.Buffer candidate = new SharpDX.Direct3D11.Buffer(d3d11, description);

          IntPtr handle;
          using (SharpDX.DXGI.Resource1 dxgiResource = candidate.QueryInterface<SharpDX.DXGI.Resource1>())
          {
            handle = dxgiResource.CreateSharedHandle(
                null,
                SharpDX.DXGI.SharedResourceFlags.Read | SharpDX.DXGI.SharedResourceFlags.Write,
                null);
          }

          d3d11Buffer = candidate;
          sharedHandle = handle;
          LastCreationMode = "NT共享 | 绑定:" + bindFlags + " | 结构化:" + (structureStride > 0);
          break;
        }
        catch (Exception exception)
        {
          lastError = exception;
        }
      }

      if (d3d11Buffer is null)
        throw new InvalidOperationException($"共享缓冲区创建失败 (全部候选组合被拒绝): {lastError?.Message}", lastError);

      // 3. 在 D3D12 设备上打开共享句柄两次 (读/写视图独立接口, 释放时引用平衡).
      void* sharedForWrite = D3D12Interop.OpenSharedHandleForResource(d3d12Device, sharedHandle);
      void* sharedForRead = D3D12Interop.OpenSharedHandleForResource(d3d12Device, sharedHandle);

      // 4. 重写两个包装器的持久 UAV 描述符, 使其指向共享资源.
      uint numElements = (uint)capacity;
      uint stride = (uint)elementSize;
      D3D12BufferInterop.CreateBufferUav(
          d3d12Device, sharedForWrite, numElements, stride,
          CsReflection.ReadDescriptorHandle(csWrite, "D3D12CpuDescriptorHandle"));
      D3D12BufferInterop.CreateBufferUav(
          d3d12Device, sharedForRead, numElements, stride,
          CsReflection.ReadDescriptorHandle(csRead, "D3D12CpuDescriptorHandle"));

      // 5. 替换两个包装器的原生资源指针; 旧资源立即释放 (其唯一使用者已被覆写的描述符).
      void* oldWrite = CsReflection.ReadComPtr(csWrite, "d3D12Resource");
      CsReflection.WriteComPtr(csWrite, "d3D12Resource", sharedForWrite);
      if (oldWrite != null)
        D3D12Interop.Release(oldWrite);

      void* oldRead = CsReflection.ReadComPtr(csRead, "d3D12Resource");
      CsReflection.WriteComPtr(csRead, "d3D12Resource", sharedForRead);
      if (oldRead != null)
        D3D12Interop.Release(oldRead);

      // 6. MonoGame 顶点缓冲壳: 创建后用反射替换其内部 SharpDX 缓冲.
      VertexBuffer shell = new VertexBuffer(mgd, declaration, capacity, BufferUsage.WriteOnly);
      System.Reflection.FieldInfo nativeField = typeof(VertexBuffer).GetField(
          "_buffer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
      if (nativeField?.GetValue(shell) is SharpDX.Direct3D11.Buffer placeholder)
        placeholder.Dispose();
      nativeField.SetValue(shell, d3d11Buffer);

      return new SharedParticleBuffer<T>(shell, csWrite, csRead, capacity);
    }

    public void Dispose()
    {
      // 壳缓冲拥有共享 D3D11 缓冲; 两个 ComputeSharp 包装器各自释放自己打开的 D3D12 接口.
      InstanceBuffer.Dispose();
      WriteView.Dispose();
      ReadView.Dispose();
    }
  }
}
