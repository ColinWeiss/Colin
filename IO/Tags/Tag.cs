namespace Colin.Core.IO
{
  /// <summary>
  /// 键值存储里"一个值"的公共底座.
  /// <br>键名字不放在值身上——键值组里靠字典记谁叫什么, 值只管自己是什么.</br>
  /// <br>原始类型到标签的隐式转换统一挂在这: C# 只在源/目标类型本身上找转换算子,
  /// 挂在子类上时 <c>data["键"] = 100</c> 这种写法就够不着了, 所以得在这儿集中安家.</br>
  /// </summary>
  public abstract class Tag
  {
    /// <summary>这个值是哪种类型, 落盘时写进档里当类型牌.</summary>
    public abstract TagType Type { get; }

    public static implicit operator Tag(byte value) => new TagByte(value);
    public static implicit operator Tag(short value) => new TagShort(value);
    public static implicit operator Tag(int value) => new TagInt(value);
    public static implicit operator Tag(long value) => new TagLong(value);
    public static implicit operator Tag(float value) => new TagFloat(value);
    public static implicit operator Tag(double value) => new TagDouble(value);
    public static implicit operator Tag(bool value) => new TagBool(value);
    public static implicit operator Tag(string value) => new TagString(value);
    public static implicit operator Tag(byte[] value) => new TagArray<byte>(value);
    public static implicit operator Tag(short[] value) => new TagArray<short>(value);
    public static implicit operator Tag(int[] value) => new TagArray<int>(value);
    public static implicit operator Tag(long[] value) => new TagArray<long>(value);
    public static implicit operator Tag(float[] value) => new TagArray<float>(value);
    public static implicit operator Tag(double[] value) => new TagArray<double>(value);
    public static implicit operator Tag(bool[] value) => new TagArray<bool>(value);
  }
}
