using System;
using System.Linq;
using System.Reflection;
using ComputeSharp;
using Microsoft.Xna.Framework.Graphics;
using CsGraphicsDevice = ComputeSharp.GraphicsDevice;

namespace Colin.Core.Graphics.Bridge;

/// <summary>
/// Reflection helpers for the internal layouts of ComputeSharp 3.2.0 (verified via runtime
/// field dumps) and MonoGame 3.8.5. The library pins against these versions; a ComputeSharp
/// upgrade requires re-verifying the field names below.
/// </summary>
internal static class CsReflection
{
    private const BindingFlags All = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    // ---- ComputeSharp internals ----

    public static FieldInfo FindField(object obj, string name)
    {
        for (var type = obj.GetType(); type != null && type != typeof(object); type = type.BaseType)
        {
            FieldInfo field = type.GetField(name, All);
            if (field != null)
            {
                return field;
            }
        }

        throw new MissingFieldException(obj.GetType().FullName, name);
    }

    /// <summary>Reads the raw COM pointer out of a ComputeSharp ComPtr field
    /// (e.g. Texture2D&lt;T&gt;.d3D12Resource, GraphicsDevice.d3D12ComputeFence).</summary>
    public static unsafe void* ReadComPtr(object owner, string fieldName)
    {
        object boxed = FindField(owner, fieldName).GetValue(owner)
            ?? throw new InvalidOperationException($"{fieldName} is null");

        FieldInfo ptrField = boxed.GetType().GetField("ptr_", All)
            ?? throw new MissingFieldException(boxed.GetType().FullName, "ptr_");

        return (void*)(IntPtr)Pointer.Unbox(ptrField.GetValue(boxed));
    }

    /// <summary>Replaces the raw COM pointer inside a ComputeSharp ComPtr field.</summary>
    public static unsafe void WriteComPtr(object owner, string fieldName, void* value)
    {
        FieldInfo field = FindField(owner, fieldName);
        object boxed = field.GetValue(owner)
            ?? throw new InvalidOperationException($"{fieldName} is null");

        FieldInfo ptrField = boxed.GetType().GetField("ptr_", All)
            ?? throw new MissingFieldException(boxed.GetType().FullName, "ptr_");

        ptrField.SetValue(boxed, Pointer.Box(value, ptrField.FieldType));
        field.SetValue(owner, boxed);
    }

    /// <summary>Reads a D3D12_CPU_DESCRIPTOR_HANDLE.ptr value (a UIntPtr) out of a
    /// ComputeSharp texture's d3D12ResourceDescriptorHandles struct.</summary>
    public static ulong ReadDescriptorHandle(object csTexture, string handleFieldName)
    {
        object handles = FindField(csTexture, "d3D12ResourceDescriptorHandles").GetValue(csTexture)
            ?? throw new InvalidOperationException("no descriptor handles");

        object handle = FindField(handles, handleFieldName).GetValue(handles);

        FieldInfo[] fields = handle.GetType().GetFields(All);
        FieldInfo ptrField = fields.FirstOrDefault(f => f.Name == "ptr") ?? fields[0];
        object value = ptrField.GetValue(handle);
        return value is UIntPtr u ? (ulong)u : Convert.ToUInt64(value);
    }

    /// <summary>Patches a ComputeSharp texture's tracked resource state (D3D12_RESOURCE_STATES).</summary>
    public static void SetResourceState(object csTexture, int state)
    {
        FieldInfo field = FindField(csTexture, "d3D12ResourceState");
        field.SetValue(csTexture, Enum.ToObject(field.FieldType, state));
    }

    public static long LuidToInt64(Luid luid)
    {
        object boxed = luid;
        uint low = (uint)typeof(Luid).GetField("lowPart", All).GetValue(boxed);
        int high = (int)typeof(Luid).GetField("highPart", All).GetValue(boxed);
        return (long)(((ulong)(uint)high << 32) | low);
    }

    // ---- MonoGame internals ----

    /// <summary>Gets the native SharpDX texture behind a MonoGame Texture2D
    /// (Texture.DirectX.cs private field, stable in the 3.8.x line).</summary>
    public static SharpDX.Direct3D11.Resource GetNativeTexture(Texture2D texture)
    {
        return (SharpDX.Direct3D11.Resource)FindField(texture, "_texture").GetValue(texture)
            ?? throw new InvalidOperationException("texture has no native resource");
    }

    /// <summary>Swaps the native texture inside a MonoGame Texture2D/RenderTarget2D shell
    /// (the internal SetNativeTexture added in 3.8.5; also invalidates cached views).</summary>
    public static void SetNativeTexture(Texture2D shell, SharpDX.Direct3D11.Resource nativeTexture)
    {
        typeof(Texture)
            .GetMethod("SetNativeTexture", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(shell, new object[] { nativeTexture });
    }
}

/// <summary>
/// Waits for ComputeSharp's own internal compute completion fence: each dispatch enqueues
/// a Signal on it with the next fence value, so waiting for a submitted value is exactly
/// "this dispatch has completed on the GPU" — no external synchronization needed.
/// </summary>
internal sealed unsafe class ComputeFenceSync : IDisposable
{
    private void* _fence;
    private readonly IntPtr _waitEvent;

    public ComputeFenceSync(CsGraphicsDevice device)
    {
        _fence = CsReflection.ReadComPtr(device, "d3D12ComputeFence");
        _waitEvent = D3D12Interop.CreateAutoResetEvent();
    }

    /// <summary>The value the NEXT dispatched command list will signal on the fence.</summary>
    public ulong ReadNextValue(CsGraphicsDevice device)
    {
        return Convert.ToUInt64(CsReflection.FindField(device, "nextD3D12ComputeFenceValue").GetValue(device));
    }

    /// <summary>Blocks the CPU until the fence reaches the given value (bounded by a timeout).</summary>
    public void Wait(ulong value)
    {
        D3D12Interop.WaitForFenceValue(_fence, value, _waitEvent);
    }

    public void Dispose()
    {
        // the fence pointer is borrowed from ComputeSharp's ComPtr (no AddRef taken
        // when reading it), so there is nothing to release here
        _fence = null;
    }
}
