using System.Runtime.InteropServices;

namespace Colin.Core.IO
{
  /// <summary>
  /// 一整段同类型数字的值.
  /// <br>物块存档的命根子: 逐格数据全靠它整存整取, 里面就是裸数组, 落盘时内存直接进流, 不逐格开销.</br>
  /// </summary>
  public sealed class TagArray<T> : Tag where T : unmanaged
  {
    public T[] Value;
    public TagArray() { }
    public TagArray(T[] value) => Value = value;
    public override TagType Type => TagTypeInfo<T>.Type;
    public static implicit operator TagArray<T>(T[] value) => new TagArray<T>(value);
  }
}
