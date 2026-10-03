namespace Colin.Core.IO
{
  /// <summary>
  /// 键值组: 一串"名字 → 值".
  /// <br>存档写法就是往里塞: <c>data["生命"] = 100;</c>, 读档按名字取: <c>data.GetInt("生命", 100)</c>.</br>
  /// <br>取的时候键不在或者类型对不上就回落到给的那个默认值, 老存档碰新代码、新存档碰老代码都炸不了.</br>
  /// <br>键的插入顺序被记着, 同一份内存多次落盘字节级一致, 存档文件能 diff.</br>
  /// </summary>
  public sealed class TagCompound : Tag
  {
    public override TagType Type => TagType.Compound;

    private readonly Dictionary<string, Tag> _values = new Dictionary<string, Tag>();
    private readonly List<string> _keys = new List<string>();
    public int Count => _keys.Count;
    public IReadOnlyList<string> Keys => _keys;
    /// <summary>按插入顺序过一遍键值, 落盘时用; 只增不删的场景下顺序就是写入顺序.</summary>
    public IEnumerable<KeyValuePair<string, Tag>> Entries
    {
      get
      {
        for (int i = 0; i < _keys.Count; i++)
          yield return new KeyValuePair<string, Tag>(_keys[i], _values[_keys[i]]);
      }
    }
    /// <summary>
    /// 塞一个值(隐式转换会替你把数字、布尔、字符串、数组包成标签), 或者取出来看看.
    /// <br>塞 null 等于删键; 取不存在的键返回 null.</br>
    /// </summary>
    public Tag this[string key]
    {
      get => _values.TryGetValue(key, out Tag value) ? value : null;
      set
      {
        if (value is null)
        {
          Remove(key);
          return;
        }
        if (_values.TryAdd(key, value))
          _keys.Add(key);
        else
          _values[key] = value;
      }
    }
    public bool Has(string key) => _values.ContainsKey(key);
    public bool Remove(string key)
    {
      if (_values.Remove(key) is false)
        return false;
      _keys.Remove(key);
      return true;
    }
    public bool GetBool(string key, bool fallback = false) => this[key] is TagBool value ? value.Value : fallback;
    public byte GetByte(string key, byte fallback = 0) => this[key] is TagByte value ? value.Value : fallback;
    public short GetShort(string key, short fallback = 0) => this[key] is TagShort value ? value.Value : fallback;
    public int GetInt(string key, int fallback = 0) => this[key] is TagInt value ? value.Value : fallback;
    public long GetLong(string key, long fallback = 0) => this[key] is TagLong value ? value.Value : fallback;
    public float GetFloat(string key, float fallback = 0f) => this[key] is TagFloat value ? value.Value : fallback;
    public double GetDouble(string key, double fallback = 0d) => this[key] is TagDouble value ? value.Value : fallback;
    public string GetString(string key, string fallback = "") => this[key] is TagString value ? value.Value : fallback;
    public TagCompound GetCompound(string key) => this[key] as TagCompound;
    public TagSeq GetSeq(string key) => this[key] as TagSeq;
    /// <summary>整段取一个数字数组; 拿到的是档里的原数组, 直接能用, 别担心引用共享(键值组读完就扔).</summary>
    public T[] GetArray<T>(string key) where T : unmanaged => (this[key] as TagArray<T>)?.Value;
  }
}
