namespace Colin.Core.IO
{
  /// <summary>
  /// 查元素类型对应的数组类型牌.
  /// <br>泛型类里没法直接 switch 类型, 建一张静态对照表, 每个封闭类型只算一次.</br>
  /// </summary>
  public static class TagTypeInfo<T> where T : unmanaged
  {
    public static readonly TagType Type = Resolve();
    private static TagType Resolve()
    {
      if (typeof(T) == typeof(byte))
        return TagType.ByteArray;
      if (typeof(T) == typeof(short))
        return TagType.ShortArray;
      if (typeof(T) == typeof(int))
        return TagType.IntArray;
      if (typeof(T) == typeof(long))
        return TagType.LongArray;
      if (typeof(T) == typeof(float))
        return TagType.FloatArray;
      if (typeof(T) == typeof(double))
        return TagType.DoubleArray;
      if (typeof(T) == typeof(bool))
        return TagType.BoolArray;
      throw new NotSupportedException("键值数组不支持的元素类型: " + typeof(T).Name);
    }
  }
}
