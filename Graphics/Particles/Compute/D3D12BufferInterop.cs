using System;
using System.Runtime.InteropServices;

namespace Particle.Compute
{
  /// <summary>
  /// D3D12 缓冲区视图创建的原始虚表调用 (与 Colin.Core.Graphics.Bridge.D3D12Interop
  /// 的纹理版本同源同风格): 为共享缓冲区重写 ComputeSharp 包装器的持久 UAV 描述符.
  /// <br>ID3D12Device vtable: CreateUnorderedAccessView = 19 (与 TinterBridge 的实战验证一致).</br>
  /// </summary>
  internal static unsafe class D3D12BufferInterop
  {
    private const int CreateUnorderedAccessViewSlot = 19;

    private const int DXGI_FORMAT_UNKNOWN = 0;
    private const int D3D12_UAV_DIMENSION_BUFFER = 1;

    /// <summary>D3D12_UNORDERED_ACCESS_VIEW_DESC (Buffer 视图) 的手工布局.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct BufferUavDesc
    {
      public int Format;
      public int ViewDimension;
      public ulong FirstElement;
      public uint NumElements;
      public uint StructureByteStride;
      public ulong CounterOffsetInBytes;
    }

    /// <summary>为共享缓冲区写入结构化 UAV 描述符 (ReadWriteBuffer 包装器使用).</summary>
    public static void CreateBufferUav(void* d3d12Device, void* resource, uint numElements, uint structureByteStride, ulong cpuDescriptorHandle)
    {
      BufferUavDesc desc = new()
      {
        Format = DXGI_FORMAT_UNKNOWN,
        ViewDimension = D3D12_UAV_DIMENSION_BUFFER,
        FirstElement = 0,
        NumElements = numElements,
        StructureByteStride = structureByteStride,
        CounterOffsetInBytes = 0
      };

      void** vtbl = *(void***)d3d12Device;
      delegate* unmanaged[MemberFunction]<void*, void*, BufferUavDesc*, ulong, void> createUav =
          (delegate* unmanaged[MemberFunction]<void*, void*, BufferUavDesc*, ulong, void>)vtbl[CreateUnorderedAccessViewSlot];
      createUav(d3d12Device, resource, &desc, cpuDescriptorHandle);
    }
  }
}
