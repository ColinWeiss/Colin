using System.Runtime.InteropServices;
using System.Text;

namespace Colin.Core.IO
{
  /// <summary>
  /// 把二进制流整个读成一棵键值组的底层机器.
  /// <br>读法是先整棵收进内存再按键取值, 所以键的顺序、多出来的键、缺掉的键都不碍事.</br>
  /// <br>键名走和写端对偶的内联表: 全文只记一份, 之后全是两字节编号, 读出来零字符串分配.</br>
  /// </summary>
  internal sealed class TagReader
  {
    private readonly Stream _stream;
    private readonly List<string> _names = new List<string>();
    public TagReader(Stream stream) => _stream = stream;
    /// <summary>从流里整棵读出键值组; 根必须是键值组.</summary>
    public TagCompound ReadRoot()
    {
      TagType type = ReadType();
      if (type != TagType.Compound)
        throw new InvalidDataException("档的根不是键值组, 类型牌是: " + type);
      ReadName(); //根名是个摆设, 读掉对齐格式就行
      return ReadCompound();
    }
    private TagType ReadType()
    {
      int value = _stream.ReadByte();
      if (value < 0)
        throw new EndOfStreamException("档在类型牌处就断了, 文件多半被截断。");
      return (TagType)value;
    }
    private string ReadName()
    {
      short id = ReadInt16();
      if (id >= 0)
        return _names[id]; //编号从表里拿, 表里的字符串全程复用, 不产生新分配
      short length = ReadInt16();
      string name = Encoding.UTF8.GetString(ReadExact(length));
      _names.Add(name); //头一回见的名字按出现顺序记进表, 和写端约定对齐
      return name;
    }
    private short ReadInt16()
    {
      int high = _stream.ReadByte();
      int low = _stream.ReadByte();
      if (low < 0)
        throw new EndOfStreamException("档在读键名时断了, 文件多半被截断。");
      return (short)(high | (low << 8));
    }
    private int ReadInt32()
    {
      Span<byte> buffer = stackalloc byte[4];
      ReadExactly(buffer);
      return System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(buffer);
    }
    private long ReadInt64()
    {
      Span<byte> buffer = stackalloc byte[8];
      ReadExactly(buffer);
      return System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(buffer);
    }
    private float ReadSingle() => BitConverter.Int32BitsToSingle(ReadInt32());
    private double ReadDouble() => BitConverter.Int64BitsToDouble(ReadInt64());
    private byte[] ReadExact(int length)
    {
      byte[] bytes = new byte[length];
      ReadExactly(bytes);
      return bytes;
    }
    private void ReadExactly(Span<byte> buffer)
    {
      _stream.ReadExactly(buffer);
    }
    private string ReadString()
    {
      int length = ReadInt32();
      if (length < 0 || length > 64 * 1024 * 1024)
        throw new InvalidDataException("字符串长度离谱(" + length + "), 档多半坏了。");
      return Encoding.UTF8.GetString(ReadExact(length));
    }
    private TagCompound ReadCompound()
    {
      TagCompound compound = new TagCompound();
      while (true)
      {
        TagType entryType = ReadType();
        if (entryType == TagType.End)
          break;
        string key = ReadName();
        Tag value = ReadPayload(entryType);
        if (value is not null)
          compound[key] = value;
      }
      return compound;
    }
    private Tag ReadPayload(TagType type)
    {
      switch (type)
      {
        case TagType.End:
          return null;
        case TagType.Byte:
          int byteValue = _stream.ReadByte();
          if (byteValue < 0)
            throw new EndOfStreamException("档在读值时断了, 文件多半被截断。");
          return new TagByte((byte)byteValue);
        case TagType.Short:
          return new TagShort(ReadInt16());
        case TagType.Int:
          return new TagInt(ReadInt32());
        case TagType.Long:
          return new TagLong(ReadInt64());
        case TagType.Float:
          return new TagFloat(ReadSingle());
        case TagType.Double:
          return new TagDouble(ReadDouble());
        case TagType.Bool:
          int boolValue = _stream.ReadByte();
          if (boolValue < 0)
            throw new EndOfStreamException("档在读值时断了, 文件多半被截断。");
          return new TagBool(boolValue != 0);
        case TagType.String:
          return new TagString(ReadString());
        case TagType.Compound:
          return ReadCompound();
        case TagType.List:
          TagType elementType = ReadType();
          int count = ReadInt32();
          if (count < 0 || count > 64 * 1024 * 1024)
            throw new InvalidDataException("列表长度离谱(" + count + "), 档多半坏了。");
          TagSeq list = new TagSeq(elementType);
          for (int i = 0; i < count; i++)
          {
            Tag item = ReadPayload(elementType);
            if (item is not null)
              list.Items.Add(item);
          }
          return list;
        case TagType.ByteArray:
          return new TagArray<byte>(ReadArray<byte>());
        case TagType.ShortArray:
          return new TagArray<short>(ReadArray<short>());
        case TagType.IntArray:
          return new TagArray<int>(ReadArray<int>());
        case TagType.LongArray:
          return new TagArray<long>(ReadArray<long>());
        case TagType.FloatArray:
          return new TagArray<float>(ReadArray<float>());
        case TagType.DoubleArray:
          return new TagArray<double>(ReadArray<double>());
        case TagType.BoolArray:
          return new TagArray<bool>(ReadArray<bool>());
        default:
          throw new InvalidDataException("档里有认不出的值类型(" + type + "), 存档版本对不上。");
      }
    }
    /// <summary>整段数字数组直接读进新数组, 内存拍平直灌, 不逐元素过手.</summary>
    private T[] ReadArray<T>() where T : unmanaged
    {
      int count = ReadInt32();
      if (count < 0 || count > 256 * 1024 * 1024)
        throw new InvalidDataException("数组长度离谱(" + count + "), 档多半坏了。");
      T[] array = new T[count];
      if (count > 0)
        ReadExactly(MemoryMarshal.AsBytes(array.AsSpan()));
      return array;
    }
  }
}
