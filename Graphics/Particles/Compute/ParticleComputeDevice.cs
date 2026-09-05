using System;
using Colin.Core.Graphics.Bridge;
using ComputeSharp;
using ComputeSharp.Interop;
using CsGraphicsDevice = ComputeSharp.GraphicsDevice;
using GraphicsDevice = Microsoft.Xna.Framework.Graphics.GraphicsDevice;

namespace Particle.Compute
{
  /// <summary>
  /// 粒子系统专用的 ComputeSharp 计算设备: 选取与 MonoGame D3D11 设备同一物理适配器的
  /// D3D12 设备, 并持有 ComputeSharp 内部完成栅栏的同步句柄.
  /// <br>与 TinterBridge 相互独立 (各自的设备包装与栅栏), 不产生耦合.</br>
  /// </summary>
  public sealed unsafe class ParticleComputeDevice : IDisposable
  {
    private static ParticleComputeDevice _shared;

    /// <summary>ComputeSharp 设备包装.</summary>
    public CsGraphicsDevice Device { get; }

    /// <summary>原始 ID3D12Device 指针 (共享资源打开用).</summary>
    public void* D3d12Device { get; }

    /// <summary>MonoGame 的 D3D11 设备.</summary>
    public SharpDX.Direct3D11.Device D3d11 { get; }

    /// <summary>完成栅栏同步器 (每次派发后记录值, 下一帧开始前等待).</summary>
    internal ComputeFenceSync Fence { get; }

    private ParticleComputeDevice(CsGraphicsDevice device, SharpDX.Direct3D11.Device d3d11, void* d3d12Device, ComputeFenceSync fence)
    {
      Device = device;
      D3d11 = d3d11;
      D3d12Device = d3d12Device;
      Fence = fence;
    }

    /// <summary>
    /// 依据 MonoGame 设备创建计算设备 (每次调用重新选取; 失败抛异常).
    /// </summary>
    public static ParticleComputeDevice Create(GraphicsDevice graphicsDevice)
    {
      SharpDX.Direct3D11.Device d3d11 = (SharpDX.Direct3D11.Device)graphicsDevice.Handle;

      if (d3d11.FeatureLevel < SharpDX.Direct3D.FeatureLevel.Level_11_0)
        throw new NotSupportedException("需要 HiDef 图形配置 (Feature Level 11.0) 才能使用 GPU 粒子策略.");

      // 选取与 MonoGame 同一物理适配器的 ComputeSharp 设备.
      long adapterLuid;
      using (SharpDX.DXGI.Device dxgiDevice = d3d11.QueryInterface<SharpDX.DXGI.Device>())
      using (SharpDX.DXGI.Adapter adapter = dxgiDevice.Adapter)
      {
        adapterLuid = adapter.Description.Luid;
      }

      CsGraphicsDevice selected = null;
      foreach (CsGraphicsDevice candidate in CsGraphicsDevice.EnumerateDevices())
      {
        if (selected is null && CsReflection.LuidToInt64(candidate.Luid) == adapterLuid)
        {
          selected = candidate;
        }
        else
        {
          candidate.Dispose();
        }
      }

      selected ??= CsGraphicsDevice.GetDefault();

      // 校验原始虚表调用机制并取回 D3D12 设备指针.
      Guid iid = D3D12Interop.IID_ID3D12Device;
      void* devicePtr = null;
      InteropServices.GetID3D12Device(selected, &iid, &devicePtr);

      if (D3D12Interop.GetDeviceRemovedReason(devicePtr) != 0)
        throw new InvalidOperationException("D3D12 设备已移除 (GetDeviceRemovedReason 失败).");

      return new ParticleComputeDevice(selected, d3d11, devicePtr, new ComputeFenceSync(selected));
    }

    /// <summary>获取进程级共享计算设备 (惰性创建, 失败抛异常).</summary>
    public static ParticleComputeDevice GetOrCreate(GraphicsDevice graphicsDevice)
    {
      if (_shared is null)
        _shared = Create(graphicsDevice);
      return _shared;
    }

    public void Dispose()
    {
      Fence?.Dispose();
      if (D3d12Device != null)
        D3D12Interop.Release(D3d12Device);
      Device?.Dispose();
    }
  }
}
