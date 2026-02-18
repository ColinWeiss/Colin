using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Colin.Core.IO
{
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
  public class StoreBox
  {
    private Dictionary<string, object> _data = new Dictionary<string, object>();

    public void Set(string key, object value) => _data[key] = value;
    public object Get(string key) => _data.TryGetValue(key, out var val) ? val : null;

    /// <summary>
    /// 添加键值对, 若键已存在则抛出异常.
    /// </summary>
    public void Add(string key, object value) => _data.Add(key, value);

    public bool Remove(string key) => _data.Remove(key);

    public bool ContainsKey(string key) => _data.ContainsKey(key);

    public void Clear() => _data.Clear();

    public int Count => _data.Count;

    public Dictionary<string, object>.KeyCollection Keys => _data.Keys;

    public Dictionary<string, object>.ValueCollection Values => _data.Values;

    public object this[string key]
    {
      get => Get(key);
      set => Set(key, value);
    }

    public int GetInt(string key) => GetAndCheckType<int>(key);
    public long GetLong(string key) => GetAndCheckType<long>(key);
    public float GetFloat(string key) => GetAndCheckType<float>(key);
    public double GetDouble(string key) => GetAndCheckType<double>(key);
    public bool GetBool(string key) => GetAndCheckType<bool>(key);
    public char GetChar(string key) => GetAndCheckType<char>(key);
    public string GetString(string key) => GetAndCheckType<string>(key);
    public short GetShort(string key) => GetAndCheckType<short>(key);
    public StoreBox GetBox(string key) => GetAndCheckType<StoreBox>(key);

    private T GetAndCheckType<T>(string key)
    {
      if (_data.TryGetValue(key, out var val))
      {
        if (val is T t)
          return t;
        throw new InvalidCastException($"Key '{key}' exists but is of type {val.GetType()}, cannot cast to {typeof(T)}.");
      }
      throw new KeyNotFoundException($"Key '{key}' not found.");
    }

    public bool TryGetInt(string key, out int value) => TryGetValue(key, out value);
    public bool TryGetLong(string key, out long value) => TryGetValue(key, out value);
    public bool TryGetFloat(string key, out float value) => TryGetValue(key, out value);
    public bool TryGetDouble(string key, out double value) => TryGetValue(key, out value);
    public bool TryGetBool(string key, out bool value) => TryGetValue(key, out value);
    public bool TryGetChar(string key, out char value) => TryGetValue(key, out value);
    public bool TryGetString(string key, out string value) => TryGetValue(key, out value);
    public bool TryGetShort(string key, out short value) => TryGetValue(key, out value);
    public bool TryGetBox(string key, out StoreBox value) => TryGetValue(key, out value);

    private bool TryGetValue<T>(string key, out T value)
    {
      if (_data.TryGetValue(key, out var obj) && obj is T t)
      {
        value = t;
        return true;
      }
      value = default(T);
      return false;
    }

    public string RootPath;

    public void Save()
    {
      using (var fs = new FileStream(RootPath, FileMode.Create))
      using (var gs = new GZipStream(fs, CompressionLevel.SmallestSize))
      {
        using (var writer = new BinaryWriter(gs))
        {
          WriteToStream(writer);
        }
      }
    }

    public void Load()
    {
      if (!File.Exists(RootPath))
        return;
      using (var fs = new FileStream(RootPath, FileMode.Open))
      using (var gs = new GZipStream(fs, CompressionMode.Decompress))
      {
        using (var reader = new BinaryReader(gs))
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
      for (int i = 0; i < count; i++)
      {
        string key = reader.ReadString();
        object value = ReadValue(reader);
        _data[key] = value;
      }
    }

    private void WriteValue(BinaryWriter writer, object value)
    {
      switch (value)
      {
        case char c:
          writer.Write((byte)StoreBoxTag.Char);
          writer.Write(c);
          break;
        case float f:
          writer.Write((byte)StoreBoxTag.Float);
          writer.Write(f);
          break;
        case string s:
          writer.Write((byte)StoreBoxTag.String);
          writer.Write(s);
          break;
        case int i:
          writer.Write((byte)StoreBoxTag.Int);
          writer.Write(i);
          break;
        case long l:
          writer.Write((byte)StoreBoxTag.Long);
          writer.Write(l);
          break;
        case double d:
          writer.Write((byte)StoreBoxTag.Double);
          writer.Write(d);
          break;
        case bool b:
          writer.Write((byte)StoreBoxTag.Bool);
          writer.Write(b);
          break;
        case short sh:
          writer.Write((byte)StoreBoxTag.Short);
          writer.Write(sh);
          break;
        case StoreBox box:
          writer.Write((byte)StoreBoxTag.Box);
          box.WriteToStream(writer);
          break;
        default:
          throw new NotSupportedException($"Type {value.GetType()} not supported");
      }
    }

    private object ReadValue(BinaryReader reader)
    {
      byte type = reader.ReadByte();
      switch (type)
      {
        case (byte)StoreBoxTag.Char: return reader.ReadChar();
        case (byte)StoreBoxTag.String: return reader.ReadString();
        case (byte)StoreBoxTag.Int: return reader.ReadInt32();
        case (byte)StoreBoxTag.Long: return reader.ReadInt64();
        case (byte)StoreBoxTag.Float: return reader.ReadSingle();
        case (byte)StoreBoxTag.Double: return reader.ReadDouble();
        case (byte)StoreBoxTag.Bool: return reader.ReadBoolean();
        case (byte)StoreBoxTag.Short: return reader.ReadInt16();
        case (byte)StoreBoxTag.Box:
          {
            var nested = new StoreBox();
            nested.ReadFromStream(reader);
            return nested;
          }
        default:
          throw new InvalidDataException($"Unknown type code: {type}");
      }
    }
  }
}