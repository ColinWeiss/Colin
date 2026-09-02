using System;
using ComputeSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CsGraphicsDevice = ComputeSharp.GraphicsDevice;
using GraphicsDevice = Microsoft.Xna.Framework.Graphics.GraphicsDevice;

namespace Colin.Core.Graphics.Bridge;

/// <summary>
/// 一个GPU纹理同时对三方可见，零拷贝：
/// - MonoGame：一个RenderTarget2D包装器（用于向其渲染，或使用SpriteBatch对其进行采样）；
/// - ComputeSharp：一个 ReadWriteTexture2D 包装器（从着色器进行读写）；
/// - 两者都由相同的内存支持：在MonoGame的设备中创建的D3D11纹理
/// （共享 | 共享NTHandle + RT | SRV | UAV绑定标志），已在ComputeSharp上打开
/// 通过ID3D12Device::OpenSharedHandle获取D3D12设备。
/// D3D12端是通过分配一个普通的ComputeSharp纹理来安装的（以获取一个
///（完全注册的包装对象）然后将其原生ID3D12Resource替换为
/// 打开了一个共享对象，重写了其持久化无人机描述符。一切尽在ComputeSharp
/// 轨迹（大小、描述符槽、状态）保持有效。
/// </summary>
public sealed unsafe class SharedTexture : IDisposable
{
  /// <summary>MonoGame-side view. RenderTarget2D (renderable AND sampleable).</summary>
  public RenderTarget2D Wrapper { get; }

  /// <summary>ComputeSharp-side view used as a shader source/target field.</summary>
  public ReadWriteTexture2D<float4> CsTexture { get; }

  private SharedTexture(RenderTarget2D wrapper, ReadWriteTexture2D<float4> csTexture)
  {
    Wrapper = wrapper;
    CsTexture = csTexture;
  }

  public static SharedTexture Create(
      GraphicsDevice mgd,
      SharpDX.Direct3D11.Device d3d11,
      CsGraphicsDevice csDevice,
      void* d3d12Device,
      int width,
      int height)
  {
    // 1. an ordinary ComputeSharp texture: gives us a fully registered wrapper object
    //    whose native resource we replace below
    ReadWriteTexture2D<float4> csTexture = csDevice.AllocateReadWriteTexture2D<float4>(width, height);

    // 2. the shared D3D11 texture, created in MonoGame's own device
    var desc = new SharpDX.Direct3D11.Texture2DDescription
    {
      Width = width,
      Height = height,
      MipLevels = 1,
      ArraySize = 1,
      Format = SharpDX.DXGI.Format.R32G32B32A32_Float,   // matches ReadWriteTexture2D<float4>
      SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),   // no MSAA (required for sharing)
      Usage = SharpDX.Direct3D11.ResourceUsage.Default,
      BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget
                  | SharpDX.Direct3D11.BindFlags.ShaderResource
                  | SharpDX.Direct3D11.BindFlags.UnorderedAccess,
      CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
      OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.Shared
                    | SharpDX.Direct3D11.ResourceOptionFlags.SharedNthandle,
    };

    var d3d11Texture = new SharpDX.Direct3D11.Texture2D(d3d11, desc);

    // 3. export an NT handle and open it on the ComputeSharp device
    IntPtr handle;
    using (var dxgiResource = d3d11Texture.QueryInterface<SharpDX.DXGI.Resource1>())
    {
      handle = dxgiResource.CreateSharedHandle(
          null,
          SharpDX.DXGI.SharedResourceFlags.Read | SharpDX.DXGI.SharedResourceFlags.Write,
          null);
    }

    void* sharedResource = D3D12Interop.OpenSharedHandleForResource(d3d12Device, handle);

    // 4. point the ComputeSharp wrapper's persistent UAV descriptors at the shared resource
    D3D12Interop.CreateTexture2DUav(
        d3d12Device, sharedResource,
        CsReflection.ReadDescriptorHandle(csTexture, "D3D12CpuDescriptorHandle"));
    D3D12Interop.CreateTexture2DUav(
        d3d12Device, sharedResource,
        CsReflection.ReadDescriptorHandle(csTexture, "D3D12CpuDescriptorHandleNonShaderVisible"));

    // 5. swap the native resource inside the wrapper. Resources opened from D3D11 behave
    //    like ALLOW_SIMULTANEOUS_ACCESS on the D3D12 side (implicit promotion/decay), and
    //    with the tracked state at UAV ComputeSharp never emits barriers for it — exactly
    //    what a simultaneous-access resource wants.
    void* oldResource = CsReflection.ReadComPtr(csTexture, "d3D12Resource");
    CsReflection.WriteComPtr(csTexture, "d3D12Resource", sharedResource);
    CsReflection.SetResourceState(csTexture, 8 /* D3D12_RESOURCE_STATE_UNORDERED_ACCESS */);

    if (oldResource != null)
    {
      D3D12Interop.Release(oldResource);   // its only remaining user was the descriptor we rewrote
    }

    // 6. the MonoGame-side wrapper shell (a RenderTarget2D whose native texture is ours)
    var wrapper = new RenderTarget2D(mgd, width, height, false, SurfaceFormat.Vector4, DepthFormat.None);
    CsReflection.SetNativeTexture(wrapper, d3d11Texture);

    return new SharedTexture(wrapper, csTexture);
  }

  public void Dispose()
  {
    Wrapper.Dispose();      // releases the D3D11 texture (we handed ownership over in step 6)
    CsTexture.Dispose();    // releases the opened D3D12 resource
  }
}
