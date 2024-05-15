using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Colin.Core.Utilities
{
  /// <summary>
  /// Header of an instance of Reference Type.
  /// </summary>
  [StructLayout(LayoutKind.Explicit)]
  public unsafe struct ObjectHeader
  {
    [FieldOffset(0)]
    public void* m_pEEType;
    [FieldOffset(8)]
    public fixed byte FieldData[1];
  }
  /// <summary>
  /// Fast reflection. Use this struct temporarily.
  /// </summary>
  public unsafe struct UnsafeObject
  {
    public ObjectHeader* Data;
    public UnsafeObject(ObjectHeader* data)
    {
      Data = data;
    }
    public UnsafeObject(void* data) : this((ObjectHeader*)data) { }
    public T GetUmanagedField<T>(uint offset) where T : unmanaged
    {
      return *(T*)(Data->FieldData + offset);
    }
    public ref T GetField<T>(uint offset)
    {
      return ref Unsafe.AsRef<T>(Data->FieldData + offset);
    }
    public ref T GetField<T>(FieldInfo fieldInfo)
    {
      var ptr = fieldInfo.FieldHandle.Value + 12;
      uint length = *(ushort*)ptr;
      uint chunkSize = *(byte*)(ptr + 2);
      return ref GetField<T>(length + (chunkSize << 16));
    }
    public static UnsafeObject As(object obj)
    {
      return new UnsafeObject(*(ObjectHeader**)Unsafe.AsPointer(ref obj));
    }
  }
}
