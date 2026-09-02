using System;
using System.Runtime.InteropServices;

namespace Colin.Core.Graphics.Bridge;

/// <summary>
/// Hand-rolled raw ID3D12 interop (vtable calls) for the bridge: opens shared NT handles
/// on the ComputeSharp device, writes UAV descriptors, and waits on ComputeSharp's
/// internal completion fence.
///
/// All slot numbers are cross-checked against d3d12.h (IUnknown 0..2, ID3D12Object 3..6,
/// then the ID3D12Device method order; ID3D12Fence slots 8..10).
///
/// Proven-dead path, kept here as documentation for future readers: a D3D12-created shared
/// texture (committed with D3D12_HEAP_FLAG_SHARED, any flag/state combination) is rejected
/// with E_INVALIDARG by every D3D11 open path. Only the D3D11-created -> D3D12-opened
/// direction (implemented below) works.
/// </summary>
internal static unsafe class D3D12Interop
{
    // IID_ID3D12Device   = {189819F1-1DB6-4B57-BE54-1821339B85F7}
    public static readonly Guid IID_ID3D12Device = new("189819f1-1db6-4b57-be54-1821339b85f7");

    // IID_ID3D12Resource = {696442BE-A72E-4059-BC79-5B5C98040FAD}
    public static readonly Guid IID_ID3D12Resource = new("696442be-a72e-4059-bc79-5b5c98040fad");

    // ID3D12Device vtable slots (per d3d12.h):
    private const int CreateUnorderedAccessViewSlot = 19;  // after CreateConstantBufferView(17), CreateShaderResourceView(18)
    private const int OpenSharedHandleSlot = 32;            // after CreateSharedHandle(31)
    private const int GetDeviceRemovedReasonSlot = 37;

    // IUnknown / ID3D12Fence vtable slots:
    private const int ReleaseSlot = 2;
    private const int FenceSetEventOnCompletionSlot = 9;     // after GetCompletedValue(8); Signal is 10

    private const int FormatR32G32B32A32Float = 2;        // DXGI_FORMAT_R32G32B32A32_FLOAT
    private const int UavDimensionTexture2D = 4;            // D3D12_UAV_DIMENSION_TEXTURE2D

    [StructLayout(LayoutKind.Sequential)]
    internal struct UavDesc
    {
        public int Format;
        public int ViewDimension;
        public int MipSlice;
        public long Pad0;   // union padding
        public long Pad1;
    }

    /// <summary>Opens a shared NT handle (created by D3D11's IDXGIResource1::CreateSharedHandle)
    /// on the ComputeSharp D3D12 device. This is the industrially-proven D3D11 -> D3D12
    /// texture sharing direction (same one SteamVR's D3D12 backend uses).</summary>
    public static void* OpenSharedHandleForResource(void* d3d12Device, IntPtr ntHandle)
    {
        void** vtbl = *(void***)d3d12Device;
        delegate* unmanaged[MemberFunction]<void*, IntPtr, Guid*, void**, int> openShared =
            (delegate* unmanaged[MemberFunction]<void*, IntPtr, Guid*, void**, int>)vtbl[OpenSharedHandleSlot];

        Guid iid = IID_ID3D12Resource;
        void* resource = null;
        int hr = openShared(d3d12Device, ntHandle, &iid, &resource);
        Marshal.ThrowExceptionForHR(hr);
        return resource;
    }

    /// <summary>Writes a Texture2D UAV over <paramref name="resource"/> into the descriptor
    /// slot identified by <paramref name="cpuDescriptorHandle"/> (a D3D12_CPU_DESCRIPTOR_HANDLE
    /// .ptr value read out of a ComputeSharp texture).</summary>
    public static void CreateTexture2DUav(void* d3d12Device, void* resource, ulong cpuDescriptorHandle)
    {
        UavDesc uav = new()
        {
            Format = FormatR32G32B32A32Float,
            ViewDimension = UavDimensionTexture2D,
            MipSlice = 0,
        };

        void** vtbl = *(void***)d3d12Device;
        delegate* unmanaged[MemberFunction]<void*, void*, void*, UavDesc*, ulong, void> createUav =
            (delegate* unmanaged[MemberFunction]<void*, void*, void*, UavDesc*, ulong, void>)vtbl[CreateUnorderedAccessViewSlot];

        // DestDescriptor is passed by value (a struct with a single SIZE_T member)
        createUav(d3d12Device, resource, null, &uav, cpuDescriptorHandle);
    }

    /// <summary>Calls ID3D12Device::GetDeviceRemovedReason (vtable slot 37, zero parameters).
    /// Used at bridge startup to validate the raw vtable-call machinery.</summary>
    public static int GetDeviceRemovedReason(void* d3d12Device)
    {
        void** vtbl = *(void***)d3d12Device;
        delegate* unmanaged[MemberFunction]<void*, int> getRemoved =
            (delegate* unmanaged[MemberFunction]<void*, int>)vtbl[GetDeviceRemovedReasonSlot];
        return getRemoved(d3d12Device);
    }

    /// <summary>IUnknown::Release (vtable slot 2) on a borrowed interface pointer.</summary>
    public static uint Release(void* comObject)
    {
        void** vtbl = *(void***)comObject;
        delegate* unmanaged[MemberFunction]<void*, uint> release =
            (delegate* unmanaged[MemberFunction]<void*, uint>)vtbl[ReleaseSlot];
        return release(comObject);
    }

    /// <summary>ID3D12Fence::SetEventOnCompletion (vtable slot 9).</summary>
    public static int FenceSetEventOnCompletion(void* fence, ulong value, IntPtr waitEvent)
    {
        void** vtbl = *(void***)fence;
        delegate* unmanaged[MemberFunction]<void*, ulong, IntPtr, int> setEvent =
            (delegate* unmanaged[MemberFunction]<void*, ulong, IntPtr, int>)vtbl[FenceSetEventOnCompletionSlot];
        return setEvent(fence, value, waitEvent);
    }

    [DllImport("kernel32", SetLastError = true)]
    private static extern IntPtr CreateEventW(IntPtr attrs, bool manualReset, bool initialState, char* name);

    [DllImport("kernel32", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

    public static IntPtr CreateAutoResetEvent() => CreateEventW(IntPtr.Zero, false, false, null);

    /// <summary>Waits (with a generous timeout budget) for the given fence value.</summary>
    public static void WaitForFenceValue(void* fence, ulong value, IntPtr waitEvent)
    {
        if (FenceSetEventOnCompletion(fence, value, waitEvent) != 0)
        {
            return; // couldn't arm the wait; proceed
        }

        // 50 x 100ms; give up rather than hang forever
        for (int i = 0; i < 50; i++)
        {
            if (WaitForSingleObject(waitEvent, 100) == 0)
            {
                return;
            }
        }
    }
}
