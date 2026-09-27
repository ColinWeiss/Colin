namespace Colin.Core.Mathematical
{
  /// <summary>
  /// 类型键快照表: 内部维护一个 <see cref="Dictionary{Type, TValue}"/> 负责按键查取,
  /// 同时维护一个与之同步的值数组作为快照, 供逐帧遍历走索引直取.
  /// <br>解决的问题: <see cref="Dictionary{TKey, TValue}.Values"/> 配合
  /// <see cref="Linq.Enumerable.ElementAt{TSource}(System.Collections.Generic.IEnumerable{TSource}, int)"/>
  /// 的逐帧遍历是 O(n²) 且每次调用分配枚举器; 本类将遍历降为 O(n) 且零分配.</br>
  /// <br>快照的顺序即 Add 的先后顺序 (插入序), <see cref="Remove(Type)"/> 做保序压缩,
  /// 其余元素的相对顺序不变 —— 依赖注册顺序决定更新/渲染层级的场景 (如场景模块列表) 可以放心替换.</br>
  /// <br>性能契约: 按键查取与索引遍历是热路径, O(1) 且不分配;
  /// 结构性修改 (Add/Remove/Clear) 是冷路径, 允许 O(n), 频繁增删的场景不适用本类.</br>
  /// <br>[!] 非线程安全, 仅主线程使用.</br>
  /// </summary>
  /// <typeparam name="TValue">存储的值类型.</typeparam>
  public class TypeSnapshotTable<TValue>
  {
    private readonly Dictionary<Type, TValue> _dictionary = new Dictionary<Type, TValue>();

    private Type[] _keys = Array.Empty<Type>();
    private TValue[] _snapshot = Array.Empty<TValue>();
    private int _count;

    /// <summary>
    /// 当前元素数量, 亦即快照数组的有效长度.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// 按快照索引直取元素, 遍历热路径专用.
    /// </summary>
    public TValue this[int index] => _snapshot[index];

    /// <summary>
    /// 按快照索引直取键.
    /// </summary>
    public Type KeyAt(int index) => _keys[index];

    /// <summary>
    /// 以快照的有效区间敞开连续内存, 供 for-each/ Span 风格的紧致循环.
    /// <br>[!] 快照数组会在结构修改时被重排或换底, 不要持有 Span 跨越任何结构性修改.</br>
    /// </summary>
    public ReadOnlySpan<TValue> AsSpan() => _snapshot.AsSpan(0, _count);

    /// <summary>
    /// 按键取值, 不存在时抛 <see cref="KeyNotFoundException"/>, 与字典语义一致.
    /// </summary>
    public TValue this[Type key] => _dictionary[key];

    /// <summary>
    /// 按键赋值: 已存在则原地替换 (快照槽位同步更新, 不改变顺序), 不存在则等价 <see cref="Add"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set(Type key, TValue value)
    {
      if (_dictionary.ContainsKey(key) is false)
      {
        Add(key, value);
        return;
      }
      _dictionary[key] = value;
      _snapshot[IndexOf(key)] = value;
    }

    /// <summary>
    /// 添加元素; 重复键抛 <see cref="ArgumentException"/>, 与字典语义一致.
    /// </summary>
    public void Add(Type key, TValue value)
    {
      _dictionary.Add(key, value);
      EnsureCapacity(_count + 1);
      _keys[_count] = key;
      _snapshot[_count] = value;
      _count++;
    }

    /// <summary>
    /// 尝试添加; 键已存在时不做任何事并返回 <see langword="false"/>.
    /// </summary>
    public bool TryAdd(Type key, TValue value)
    {
      if (_dictionary.ContainsKey(key))
        return false;
      Add(key, value);
      return true;
    }

    /// <summary>
    /// 按键移除; 做保序压缩, 其余元素在快照中的相对顺序不变.
    /// </summary>
    public bool Remove(Type key)
    {
      int index = IndexOf(key);
      if (index < 0)
        return false;
      _dictionary.Remove(key);
      int tail = _count - index - 1;
      if (tail > 0)
      {
        Array.Copy(_keys, index + 1, _keys, index, tail);
        Array.Copy(_snapshot, index + 1, _snapshot, index, tail);
      }
      _count--;
      _keys[_count] = null;
      _snapshot[_count] = default;
      return true;
    }

    /// <summary>
    /// 清空全部元素; 底层数组保留复用, 不释放.
    /// </summary>
    public void Clear()
    {
      _dictionary.Clear();
      Array.Clear(_keys, 0, _keys.Length);
      Array.Clear(_snapshot, 0, _snapshot.Length);
      _count = 0;
    }

    /// <summary>
    /// 判断键是否存在.
    /// </summary>
    public bool ContainsKey(Type key) => _dictionary.ContainsKey(key);

    /// <summary>
    /// 按键查取.
    /// </summary>
    public bool TryGetValue(Type key, out TValue value) => _dictionary.TryGetValue(key, out value);

    private int IndexOf(Type key)
    {
      for (int index = 0; index < _count; index++)
      {
        if (_keys[index] == key)
          return index;
      }
      return -1;
    }

    private void EnsureCapacity(int min)
    {
      if (min <= _snapshot.Length)
        return;
      int newSize = _snapshot.Length * 2;
      if (newSize < 8)
        newSize = 8;
      if (newSize < min)
        newSize = min;
      Array.Resize(ref _keys, newSize);
      Array.Resize(ref _snapshot, newSize);
    }
  }
}
