namespace Colin.Core.Modulars.Tiles
{
  using System.Threading;

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
    /// <br>世界生成在后台也会递增它, 用原子操作, 丢了序号会让两个结构物块叠在同一层.</br>
    /// </summary>
    public static int UpdateSortCache()
    {
      return Interlocked.Increment(ref _sortCache) - 1;
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
    /// <br>后台读档和生成已经不直写这本字典了, 指针都由 OnChunkReady 在主线程收尾补上, 这里怎么清都安全.</br>
    /// <br>渲染端每帧全量重建, 所以列表就不池化了, 每帧这点列表开销交给 GC 可以接受.</br>
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