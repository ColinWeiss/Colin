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
      if (Cache.ContainsKey(wCoord) is false)
        Cache[wCoord] = new List<TilePointer>();
      List<TilePointer> _list = Cache[wCoord];
      _list.Add(pointer);
      _list.Sort();
      _list.ForEach(a => a.Index = _list.IndexOf(a));
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
      if (Cache.ContainsKey(wCoord) is false)
        return false;
      List<TilePointer> _list = Cache[wCoord];
      if (_list.Remove(pointer))
      {
        _list.Sort();
        _list.ForEach(a => a.Index = _list.IndexOf(a));
        return true;
      }
      else
        return true;
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