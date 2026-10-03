namespace Colin.Core.IO
{
  /// <summary>
  /// 键值组里每种值的小类型牌.
  /// <br>存档里每个值开头都写一个这个, 读的人一看牌就知道后面跟着的是什么, 所以叫自描述.</br>
  /// </summary>
  public enum TagType : byte
  {
    /// <summary>键值组的收尾标记, 没有载荷.</summary>
    End = 0,
    Byte = 1,
    Short = 2,
    Int = 3,
    Long = 4,
    Float = 5,
    Double = 6,
    Bool = 7,
    String = 8,
    ByteArray = 9,
    ShortArray = 10,
    IntArray = 11,
    LongArray = 12,
    FloatArray = 13,
    DoubleArray = 14,
    BoolArray = 15,
    /// <summary>嵌套键值组, 里面又是一串带名字的值.</summary>
    Compound = 16,
    /// <summary>一串同类型的值, 元素不带头, 靠顺序.</summary>
    List = 17,
  }
}
