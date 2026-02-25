using System.IO.Compression;
using System.Runtime;          // 引入 Variant 结构体所在的命名空间
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Colin.Core.IO
{
  /// <summary>
  /// 类型标记（用于序列化，保持与原始格式兼容）
  /// </summary>
  public enum StoreBoxTag : byte
  {
    Bool,
    Short,
    Int,
    Long,
    Float,
    Double,
    Char,
    String,
    Box
  }

  /// <summary>
  /// 基于 Variant 实现的键值对容器，值类型无需装箱，性能更高。
  /// </summary>
  public class StoreBox
  {
    // 内部存储：键为字符串，值为 Variant（可容纳值类型和引用类型）
    private Dictionary<string, Variant> _data = new Dictionary<string, Variant>();

    // 文件路径（用于 Save/Load）
    public string RootPath;

    #region 基本操作（与原始 API 完全一致）

    public void Set(string key, object value)
    {
      _data[key] = CreateVariantFromObject(value);
    }

    public object Get(string key)
    {
      if (_data.TryGetValue(key, out Variant var))
      {
        return VariantToObject(var);
      }
      return null;
    }

    public void Add(string key, object value)
    {
      _data.Add(key, CreateVariantFromObject(value));
    }

    public bool Remove(string key) => _data.Remove(key);

    public bool ContainsKey(string key) => _data.ContainsKey(key);

    public void Clear() => _data.Clear();

    public int Count => _data.Count;

    public Dictionary<string, Variant>.KeyCollection Keys => _data.Keys;

    public Dictionary<string, Variant>.ValueCollection Values => _data.Values;

    public object this[string key]
    {
      get => Get(key);
      set => Set(key, value);
    }

    #endregion

    #region 强类型 Get 方法

    public int GetInt(string key) => GetAndCheckType<int>(key, VariantTag.Int32, v => v.GetInt32());
    public long GetLong(string key) => GetAndCheckType<long>(key, VariantTag.Int64, v => v.GetInt64());
    public float GetFloat(string key) => GetAndCheckType<float>(key, VariantTag.Float32, v => v.GetFloat32());
    public double GetDouble(string key) => GetAndCheckType<double>(key, VariantTag.Float64, v => v.GetFloat64());
    public bool GetBool(string key) => GetAndCheckType<bool>(key, VariantTag.Bool, v => v.GetBool());
    public char GetChar(string key) => GetAndCheckType<char>(key, VariantTag.Char, v => v.GetChar());
    public short GetShort(string key) => GetAndCheckType<short>(key, VariantTag.Int16, v => v.GetInt16());
    public string GetString(string key) => GetAndCheckType<string>(key, VariantTag.Any, v => v.Get<string>());
    public StoreBox GetBox(string key) => GetAndCheckType<StoreBox>(key, VariantTag.Any, v => v.Get<StoreBox>());

    private T GetAndCheckType<T>(string key, VariantTag expectedTag, Func<Variant, T> getter)
    {
      if (_data.TryGetValue(key, out Variant var))
      {
        if (var.Tag == expectedTag)
        {
          return getter(var);
        }
        throw new InvalidCastException($"Key '{key}' exists but is of type {var.Tag}, cannot cast to {typeof(T)}.");
      }
      throw new KeyNotFoundException($"Key '{key}' not found.");
    }

    #endregion

    #region TryGet 方法

    public bool TryGetInt(string key, out int value) => TryGetValue(key, VariantTag.Int32, v => v.GetInt32(), out value);
    public bool TryGetLong(string key, out long value) => TryGetValue(key, VariantTag.Int64, v => v.GetInt64(), out value);
    public bool TryGetFloat(string key, out float value) => TryGetValue(key, VariantTag.Float32, v => v.GetFloat32(), out value);
    public bool TryGetDouble(string key, out double value) => TryGetValue(key, VariantTag.Float64, v => v.GetFloat64(), out value);
    public bool TryGetBool(string key, out bool value) => TryGetValue(key, VariantTag.Bool, v => v.GetBool(), out value);
    public bool TryGetChar(string key, out char value) => TryGetValue(key, VariantTag.Char, v => v.GetChar(), out value);
    public bool TryGetShort(string key, out short value) => TryGetValue(key, VariantTag.Int16, v => v.GetInt16(), out value);
    public bool TryGetString(string key, out string value)
    {
      if (_data.TryGetValue(key, out Variant var) && var.Tag == VariantTag.Any)
      {
        value = var.ManagedValue as string;   // 可能为 null
        return value != null || var.ManagedValue == null; // 允许 null 字符串返回 true，但 value 为 null
      }
      value = default;
      return false;
    }
    public bool TryGetBox(string key, out StoreBox value)
    {
      if (_data.TryGetValue(key, out Variant var) && var.Tag == VariantTag.Any)
      {
        value = var.ManagedValue as StoreBox;
        return value != null;
      }
      value = default;
      return false;
    }

    private bool TryGetValue<T>(string key, VariantTag expectedTag, Func<Variant, T> getter, out T value)
    {
      if (_data.TryGetValue(key, out Variant var) && var.Tag == expectedTag)
      {
        value = getter(var);
        return true;
      }
      value = default;
      return false;
    }

    #endregion

    #region 对象 <-> Variant 转换

    private static Variant CreateVariantFromObject(object value)
    {
      if (value == null)
        return Variant.CreateAny(null);

      return value switch
      {
        int i => Variant.CreateInt32(i),
        long l => Variant.CreateInt64(l),
        float f => Variant.CreateFloat32(f),
        double d => Variant.CreateFloat64(d),
        bool b => Variant.CreateBool(b),
        char c => Variant.CreateChar(c),
        short s => Variant.CreateInt16(s),
        string str => Variant.CreateAny(str),
        StoreBox b => Variant.CreateAny(b),
        _ => throw new NotSupportedException($"Type {value.GetType()} is not supported by StoreBox.")
      };
    }

    private static object VariantToObject(Variant var)
    {
      return var.Tag switch
      {
        VariantTag.Int32 => var.GetInt32(),
        VariantTag.Int64 => var.GetInt64(),
        VariantTag.Float32 => var.GetFloat32(),
        VariantTag.Float64 => var.GetFloat64(),
        VariantTag.Bool => var.GetBool(),
        VariantTag.Char => var.GetChar(),
        VariantTag.Int16 => var.GetInt16(),
        VariantTag.Any => var.ManagedValue,
        VariantTag.None => null,
        _ => throw new InvalidOperationException($"Unexpected tag: {var.Tag}")
      };
    }

    #endregion

    #region 序列化（保持原始二进制格式不变）

    public async Task SaveAsync()
    {
      using (var fs = new FileStream(RootPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
      using (var gs = new GZipStream(fs, CompressionLevel.Optimal))
      using (var writer = new BinaryWriter(fs))
      {
        await Task.Run(() => WriteToStream(writer));
      }
    }

    public void Save()
    {
      using (var fs = new FileStream(RootPath, FileMode.Create))
      using (var gs = new GZipStream(fs, CompressionLevel.Optimal))
      using (var writer = new BinaryWriter(gs))
      {
        WriteToStream(writer);
      }
    }

    public void Load()
    {
      if (!File.Exists(RootPath))
        return;

      using (var fs = new FileStream(RootPath, FileMode.Open))
      using (var gs = new GZipStream(fs, CompressionMode.Decompress))
      using (var reader = new BinaryReader(fs))
      {
        ReadFromStream(reader);
      }
    }

    private void WriteToStream(BinaryWriter writer)
    {
      writer.Write(_data.Count);
      foreach (var kv in _data)
      {
        writer.Write(kv.Key);
        WriteValue(writer, kv.Value);
      }
    }

    private void ReadFromStream(BinaryReader reader)
    {
      int count = reader.ReadInt32();
      _data.Clear();
      string key;
      Variant value;
      for (int i = 0; i < count; i++)
      {
        key = reader.ReadString();
        value = ReadValue(reader);
        _data[key] = value;
      }
    }

    private void WriteValue(BinaryWriter writer, Variant value)
    {
      switch (value.Tag)
      {
        case VariantTag.Int32:
          writer.Write((byte)StoreBoxTag.Int);
          writer.Write(value.GetInt32());
          break;
        case VariantTag.Int64:
          writer.Write((byte)StoreBoxTag.Long);
          writer.Write(value.GetInt64());
          break;
        case VariantTag.Float32:
          writer.Write((byte)StoreBoxTag.Float);
          writer.Write(value.GetFloat32());
          break;
        case VariantTag.Float64:
          writer.Write((byte)StoreBoxTag.Double);
          writer.Write(value.GetFloat64());
          break;
        case VariantTag.Bool:
          writer.Write((byte)StoreBoxTag.Bool);
          writer.Write(value.GetBool());
          break;
        case VariantTag.Char:
          writer.Write((byte)StoreBoxTag.Char);
          writer.Write(value.GetChar());
          break;
        case VariantTag.Int16:
          writer.Write((byte)StoreBoxTag.Short);
          writer.Write(value.GetInt16());
          break;
        case VariantTag.Any:
          // 根据实际引用类型写入
          if (value.ManagedValue is string s)
          {
            writer.Write((byte)StoreBoxTag.String);
            writer.Write(s);
          }
          else if (value.ManagedValue is StoreBox box)
          {
            writer.Write((byte)StoreBoxTag.Box);
            box.WriteToStream(writer);  // 递归写入嵌套 Box
          }
          else if (value.ManagedValue == null)
          {
            // 原始格式不支持 null，这里抛出异常以保持原行为
            throw new NotSupportedException("Cannot serialize null value.");
          }
          else
          {
            throw new NotSupportedException($"Type {value.ManagedValue.GetType()} not supported.");
          }
          break;
        default:
          throw new NotSupportedException($"Variant tag {value.Tag} not supported.");
      }
    }

    private Variant ReadValue(BinaryReader reader)
    {
      byte type = reader.ReadByte();
      switch ((StoreBoxTag)type)
      {
        case StoreBoxTag.Char: return Variant.CreateChar(reader.ReadChar());
        case StoreBoxTag.String: return Variant.CreateAny(reader.ReadString());
        case StoreBoxTag.Int: return Variant.CreateInt32(reader.ReadInt32());
        case StoreBoxTag.Long: return Variant.CreateInt64(reader.ReadInt64());
        case StoreBoxTag.Float: return Variant.CreateFloat32(reader.ReadSingle());
        case StoreBoxTag.Double: return Variant.CreateFloat64(reader.ReadDouble());
        case StoreBoxTag.Bool: return Variant.CreateBool(reader.ReadBoolean());
        case StoreBoxTag.Short: return Variant.CreateInt16(reader.ReadInt16());
        case StoreBoxTag.Box:
          var nested = new StoreBox();
          nested.ReadFromStream(reader);
          return Variant.CreateAny(nested);
        default:
          throw new InvalidDataException($"Unknown type code: {type}");
      }
    }

    #endregion
  }
}