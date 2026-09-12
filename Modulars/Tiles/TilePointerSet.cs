namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块指针集合.
  /// <br>用于管理物块指针; 运行时作缓存使用.</br>
  /// </summary>
  public class TilePointerSet
  {
    /// <summary>
    /// 排序缓存.
    /// </summary>
    private static int _sortCache = 0;
    /// <summary>
    /// 更新排序缓存.
    /// </summary>
    public static int UpdateSortCache()
    {
      int value = _sortCache;
      _sortCache++;
      return value;
    }

    /// <summary>
    /// 指示对应格的指针集合; 用作缓存.
    /// </summary>
    public Dictionary<Point3, List<TilePointer>> Cache { get; set; } = new Dictionary<Point3, List<TilePointer>>();

    /// <summary>
    /// 向指定格内添加物块指针; 在执行该方法时会对指定格中的指针进行排序.
    /// </summary>
    public bool AddPointer(Point3 wCoord, TilePointer pointer)
    {
      if (Cache.TryGetValue(wCoord, out List<TilePointer> _list) is false)
      {
        _list = new List<TilePointer>();
        Cache[wCoord] = _list;
      }
      _list.Add(pointer);
      _list.Sort();
      // 这里以前用 ForEach 套 IndexOf, 那是平方级的, 单格指针一多每帧都是天价, 换成线性遍历
      for (int i = 0; i < _list.Count; i++)
      {
        TilePointer indexed = _list[i];
        indexed.Index = i;
        _list[i] = indexed;
      }
      return true;
    }

    /// <summary>
    /// 向集合缓存内批量添加指针.
    /// </summary>
    public void AddPointers(ICollection<TilePointer> pointers)
    {
      foreach (var pointer in pointers)
        AddPointer(pointer.PointTo, pointer);
    }

    /// <summary>
    /// 删除物块指针; 在执行该方法时会对指定格中的指针进行排序.
    /// </summary>
    public bool RemovePointer(Point3 wCoord, TilePointer pointer)
    {
      if (Cache.TryGetValue(wCoord, out List<TilePointer> _list) is false)
        return false;
      if (_list.Remove(pointer))
      {
        _list.Sort();
        for (int i = 0; i < _list.Count; i++)
        {
          TilePointer indexed = _list[i];
          indexed.Index = i;
          _list[i] = indexed;
        }
        return true;
      }
      else
        return true;
    }

    /// <summary>
    /// 清空整个指针缓存.
    /// <br>本来想把列表回收进池子, 但世界生成在后台线程也会写这本字典, 活着枚举必撞并发, 直接清最稳.</br>
    /// <br>真正的大头是 AddPointer 里的平方级重排, 那个已经修掉了, 每帧这点列表开销交给 GC 可以接受.</br>
    /// </summary>
    public void ClearToPool()
    {
      Cache.Clear();
    }

    public void ClearPointer(Point3 wCoord)
    {
      if (Cache.ContainsKey(wCoord) is false)
        return;
      Cache.Remove(wCoord);
    }


    /// <summary>
    /// 判断指定格中是否存在指定指针.
    /// </summary>
    public bool HasPointer(Point3 wCoord, TilePointer pointer)
    {
      return Cache[wCoord].Contains(pointer);
    }

    /// <summary>
    /// 判断指定格中是否存在指针.
    /// </summary>
    public bool HasPointer(Point3 wCoord)
    {
      if (Cache.ContainsKey(wCoord))
        return Cache[wCoord]?.Count > 0;
      else
        return false;
    }
  }
}