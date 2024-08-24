using System.Runtime.Serialization;
using System.Text.Json;

namespace Colin.Core.IO
{
  public class StoreBox : ISerializable
  {
    private Dictionary<string, object> _datas;
    // public Dictionary<string, object> Datas => _datas;

    public object this[int index] => _datas.ElementAt(index).Value;
    public object this[string key]
    {
      get => _datas[key];
      set => _datas[key] = value;
    }

    public int Count => _datas.Count;

    public StoreBox() { _datas = new Dictionary<string, object>(); }
    public StoreBox(Dictionary<string, object> dic) { _datas = dic; }
    public StoreBox(SerializationInfo info, StreamingContext context)
    {
      _datas = (Dictionary<string, object>)info.GetValue("_datas", typeof(Dictionary<string, object>));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("_datas", _datas);
    }

    /// <summary>
    ///  将存储箱中的数据保存为文件.
    ///  <br>[!] 保存的数据将会保存至 <see cref="BasicsDirectory.DataDir"/>.</br>
    /// </summary>
    public void Save(string fileName)
    {
      string _fullPath = Path.Combine(BasicsDirectory.DataDir, fileName);
      using (FileStream fileStream = new FileStream(_fullPath, FileMode.OpenOrCreate))
      {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;
        JsonSerializer.Serialize(fileStream, _datas, options);
      }
    }

    /// <summary>
    ///  将文件的数据读取至箱中.
    /// </summary>
    public void Load(string fileName)
    {
      string _fullPath = Path.Combine(BasicsDirectory.DataDir, fileName);
      using (FileStream fileStream = new FileStream(_fullPath, FileMode.Open))
      {
        _datas = (Dictionary<string, object>)JsonSerializer.Deserialize(fileStream, typeof(Dictionary<string, object>));
      }
    }

    public StoreBox GetBox(string key)
    {
      if (_datas.TryGetValue(key, out object value))
      {
        Dictionary<string, object> dic = JsonSerializer.Deserialize<Dictionary<string, object>>((JsonElement)value);
        return new StoreBox(dic);
      }
      else
        return default;
    }

    public object Get(string key)
    {
      if (_datas.TryGetValue(key, out object value))
        return value;
      else
        return default;
    }

    public byte GetByte(string key) => (byte)Get(key);

    public short GetShort(string key) => (short)Get(key);

    public ushort GetUShort(string key) => (ushort)Get(key);

    public int GetInt(string key) => (int)Get(key);

    public uint GetUInt(string key) => (uint)Get(key);

    public long GetLong(string key) => (long)Get(key);

    public ulong GetULong(string key) => (ulong)Get(key);

    public decimal GetDecimal(string key) => (decimal)Get(key);

    public float GetFloat(string key) => (float)Get(key);

    public double GetDouble(string key) => (double)Get(key);

    public string GetString(string key) => (string)Get(key);

    public char GetChar(string key) => (char)Get(key);

    public void Add(string key, StoreBox box)
    {
      _datas.Add(key, box._datas);
    }

    public void Add(string key, ISerializable value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, byte value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, short value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, ushort value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, int value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, uint value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, long value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, ulong value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, decimal value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, float value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, double value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, string value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

    public void Add(string key, char value)
    {
      if (_datas.ContainsKey(key))
        this[key] = value;
      else
        _datas.Add(key, value);
    }

  }
}