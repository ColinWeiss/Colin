using System.Runtime.InteropServices;
using System.Text;

namespace Colin.Core.IO
{
  /// <summary>
  /// 把键值组写进二进制流的底层机器.
  /// <br>每个值开头带一个类型牌加一个键名引用; 键名第一次出现记全文, 之后只用两字节编号,
  /// 整份档里键名的开销几乎被压没了.</br>
  /// </summary>
  internal sealed class TagWriter
  {
    private readonly BinaryWriter _writer;
    private readonly Dictionary<string, short> _nameIds = new Dictionary<string, short>();
    public TagWriter(Stream stream)
    {
      //leaveOpen 给上: 键值组写完流还要归调用方管, 不能跟着一起关
      _writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
    }
    /// <summary>把一份键值组整棵写进流, 就是档的根.</summary>
    public void WriteRoot(TagCompound root)
    {
      WriteTyped(TagType.Compound, "Root");
      WritePayload(root);
    }
    /// <summary>写一个值的类型牌和键名引用.</summary>
    private void WriteTyped(TagType type, string key)
    {
      _writer.Write((byte)type);
      WriteName(key);
    }
    private void WriteName(string key)
    {
      if (_nameIds.TryGetValue(key, out short id))
      {
        _writer.Write(id);
        return;
      }
      //头一回见: 写个 -1 标记, 后面跟全文, 读的人照同样顺序把名字表记下来
      _writer.Write((short)-1);
      byte[] bytes = Encoding.UTF8.GetBytes(key);
      _writer.Write((short)bytes.Length);
      _writer.Write(bytes);
      if (_nameIds.Count < short.MaxValue)
        _nameIds[key] = (short)_nameIds.Count;
    }
    private void WritePayload(Tag tag)
    {
      switch (tag.Type)
      {
        case TagType.Byte:
          _writer.Write(((TagByte)tag).Value);
          break;
        case TagType.Short:
          _writer.Write(((TagShort)tag).Value);
          break;
        case TagType.Int:
          _writer.Write(((TagInt)tag).Value);
          break;
        case TagType.Long:
          _writer.Write(((TagLong)tag).Value);
          break;
        case TagType.Float:
          _writer.Write(((TagFloat)tag).Value);
          break;
        case TagType.Double:
          _writer.Write(((TagDouble)tag).Value);
          break;
        case TagType.Bool:
          _writer.Write(((TagBool)tag).Value);
          break;
        case TagType.String:
          WriteString(((TagString)tag).Value);
          break;
        case TagType.Compound:
          WriteCompound((TagCompound)tag);
          break;
        case TagType.List:
          WriteList((TagSeq)tag);
          break;
        case TagType.ByteArray:
          if (tag is TagArray<byte> byteArray)
            WriteArray(byteArray.Value);
          break;
        case TagType.ShortArray:
          if (tag is TagArray<short> shortArray)
            WriteArray(shortArray.Value);
          break;
        case TagType.IntArray:
          if (tag is TagArray<int> intArray)
            WriteArray(intArray.Value);
          break;
        case TagType.LongArray:
          if (tag is TagArray<long> longArray)
            WriteArray(longArray.Value);
          break;
        case TagType.FloatArray:
          if (tag is TagArray<float> floatArray)
            WriteArray(floatArray.Value);
          break;
        case TagType.DoubleArray:
          if (tag is TagArray<double> doubleArray)
            WriteArray(doubleArray.Value);
          break;
        case TagType.BoolArray:
          if (tag is TagArray<bool> boolArray)
            WriteArray(boolArray.Value);
          break;
      }
    }
    private void WriteString(string value)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
      _writer.Write(bytes.Length);
      _writer.Write(bytes);
    }
    private void WriteCompound(TagCompound compound)
    {
      foreach (KeyValuePair<string, Tag> entry in compound.Entries)
      {
        WriteTyped(entry.Value.Type, entry.Key);
        WritePayload(entry.Value);
      }
      _writer.Write((byte)TagType.End);
    }
    private void WriteList(TagSeq list)
    {
      _writer.Write((byte)list.ElementType);
      _writer.Write(list.Items.Count);
      for (int i = 0; i < list.Items.Count; i++)
        WritePayload(list.Items[i]);
    }
    /// <summary>
    /// 数组整块直写: 元素内存布局就是紧凑的, 按 span 拍平进流, 不做任何逐元素操作.
    /// <br>物块存档的热路径全走这里.</br>
    /// </summary>
    private void WriteArray<T>(T[] array) where T : unmanaged
    {
      if (array is null)
      {
        _writer.Write(0);
        return;
      }
      _writer.Write(array.Length);
      if (array.Length == 0)
        return;
      _writer.Write(MemoryMarshal.AsBytes(array.AsSpan()));
    }
  }
}
