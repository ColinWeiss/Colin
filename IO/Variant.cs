using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Colin.Core.IO
{
  /// <summary>
  /// 表示支持的分支类型标记。
  /// </summary>
  public enum VariantTag : byte
  {
    None = 0,
    Int32 = 1,
    Int64 = 2,
    Float32 = 3,
    Float64 = 4,
    Bool = 5,
    Char = 6,
    Byte = 7,
    SByte = 8,
    Int16 = 9,
    UInt16 = 10,
    UInt32 = 11,
    UInt64 = 12,
    Any = 255
  }

  /// <summary>
  /// 用于在内存中重叠存储非托管值的固定大小缓冲区。
  /// </summary>
  [StructLayout(LayoutKind.Explicit, Size = 8)]
  internal struct VariantStorage { }

  /// <summary>
  /// 一个高性能的联合类型结构体，通过手动布局最小化内存占用。
  /// 该结构体在 x64 下通常占用 24 字节。
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct Variant
  {
    public VariantTag Tag;
    internal VariantStorage UnmanagedValue;
    public object? ManagedValue;

    #region 内部辅助方法

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static VariantStorage CreateStorage<T>(T value) where T : unmanaged
    {
      VariantStorage storage = default;
      Unsafe.As<VariantStorage, T>(ref storage) = value;
      return storage;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T GetUnmanaged<T>(VariantTag expected) where T : struct
    {
      if (Tag != expected) ThrowMismatch(expected.ToString());
      return Unsafe.As<VariantStorage, T>(ref UnmanagedValue);
    }

    private static void ThrowMismatch(string expected) =>
        throw new InvalidOperationException($"Variant 包含的类型不是 {expected}");

    #endregion

    #region 获取方法

    public int GetInt32() => GetUnmanaged<int>(VariantTag.Int32);
    public long GetInt64() => GetUnmanaged<long>(VariantTag.Int64);
    public float GetFloat32() => GetUnmanaged<float>(VariantTag.Float32);
    public double GetFloat64() => GetUnmanaged<double>(VariantTag.Float64);
    public bool GetBool() => GetUnmanaged<bool>(VariantTag.Bool);
    public byte GetByte() => GetUnmanaged<byte>(VariantTag.Byte);
    public sbyte GetSByte() => GetUnmanaged<sbyte>(VariantTag.SByte);
    public short GetInt16() => GetUnmanaged<short>(VariantTag.Int16);
    public ushort GetUInt16() => GetUnmanaged<ushort>(VariantTag.UInt16);
    public uint GetUInt32() => GetUnmanaged<uint>(VariantTag.UInt32);
    public ulong GetUInt64() => GetUnmanaged<ulong>(VariantTag.UInt64);
    public char GetChar() => GetUnmanaged<char>(VariantTag.Char);

    /// <summary>
    /// 获取存储的引用类型对象。
    /// </summary>
    public object? GetAny() => Tag == VariantTag.Any ? ManagedValue : throw new InvalidOperationException("Variant 不包含对象类型");

    /// <summary>
    /// 获取指定类型的引用类型对象。
    /// </summary>
    public T Get<T>() where T : class
    {
      if (Tag == VariantTag.Any && ManagedValue is T value) return value;
      throw new InvalidOperationException($"Variant 无法转换为类型 {typeof(T).Name}");
    }

    #endregion

    #region 静态构造工厂

    public static Variant None => default;
    public static Variant CreateInt32(int x) => new Variant { Tag = VariantTag.Int32, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateInt64(long x) => new Variant { Tag = VariantTag.Int64, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateFloat32(float x) => new Variant { Tag = VariantTag.Float32, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateFloat64(double x) => new Variant { Tag = VariantTag.Float64, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateBool(bool x) => new Variant { Tag = VariantTag.Bool, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateByte(byte x) => new Variant { Tag = VariantTag.Byte, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateSByte(sbyte x) => new Variant { Tag = VariantTag.SByte, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateInt16(short x) => new Variant { Tag = VariantTag.Int16, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateUInt16(ushort x) => new Variant { Tag = VariantTag.UInt16, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateUInt32(uint x) => new Variant { Tag = VariantTag.UInt32, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateUInt64(ulong x) => new Variant { Tag = VariantTag.UInt64, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateChar(char x) => new Variant { Tag = VariantTag.Char, UnmanagedValue = CreateStorage(x) };
    public static Variant CreateAny(object x) => new Variant { Tag = VariantTag.Any, ManagedValue = x };

    #endregion

    #region 基础重写

    public override string ToString()
    {
      return Tag switch
      {
        VariantTag.Int32 => Unsafe.As<VariantStorage, int>(ref UnmanagedValue).ToString(),
        VariantTag.Int64 => Unsafe.As<VariantStorage, long>(ref UnmanagedValue).ToString(),
        VariantTag.Float32 => Unsafe.As<VariantStorage, float>(ref UnmanagedValue).ToString(),
        VariantTag.Float64 => Unsafe.As<VariantStorage, double>(ref UnmanagedValue).ToString(),
        VariantTag.Bool => Unsafe.As<VariantStorage, bool>(ref UnmanagedValue).ToString(),
        VariantTag.Char => Unsafe.As<VariantStorage, char>(ref UnmanagedValue).ToString(),
        VariantTag.Byte => Unsafe.As<VariantStorage, byte>(ref UnmanagedValue).ToString(),
        VariantTag.SByte => Unsafe.As<VariantStorage, sbyte>(ref UnmanagedValue).ToString(),
        VariantTag.Int16 => Unsafe.As<VariantStorage, short>(ref UnmanagedValue).ToString(),
        VariantTag.UInt16 => Unsafe.As<VariantStorage, ushort>(ref UnmanagedValue).ToString(),
        VariantTag.UInt32 => Unsafe.As<VariantStorage, uint>(ref UnmanagedValue).ToString(),
        VariantTag.UInt64 => Unsafe.As<VariantStorage, ulong>(ref UnmanagedValue).ToString(),
        VariantTag.Any => ManagedValue?.ToString() ?? "null",
        VariantTag.None => "None",
        _ => "Invalid"
      };
    }

    #endregion
  }
}