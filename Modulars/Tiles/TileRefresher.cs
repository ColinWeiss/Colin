using System.Collections.Concurrent;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块刷新器.
  /// </summary>
  public class TileRefresher : ISceneModule
  {
    public Scene Scene { get; set; }
    public bool Enable { get; set; }

    private Tile _tile;
    public Tile Tile
    {
      get
      {
        if (_tile is null)
          _tile = Scene.GetModule<Tile>();
        return _tile;
      }
    }

    public ConcurrentQueue<Point3> RefreshQueue = new();

    /// <summary>
    /// 在物块刷新时发生; 用于模块之间的联动事件.
    /// </summary>
    public event Action<Point3> OnTileRefresh = null;

    public void DoInitialize()
    {
    }

    public void Start()
    {
    }

    public void DoUpdate(GameTime time)
    {
      ref TileInfo info = ref Tile[0, 0, 0];
      while (!RefreshQueue.IsEmpty)
      {
        RefreshQueue.TryDequeue(out Point3 coord);
        info = ref Tile[coord];
        RefreshHandle(coord);
        if (info.Empty)
        {
          info.Behavior = null;
          info.Scripts.Clear();
        }
      }
    }

    public void RefreshMark(Point3 coord, int radius = 0)
    {
      Point3 refresh;
      for (int x = -radius; x <= radius; x++)
      {
        for (int y = -radius; y <= radius; y++)
        {
          refresh = new Point3(coord.X + x, coord.Y + y, coord.Z);
          RefreshQueue.Enqueue(refresh);
        }
      }
    }

    public void RefreshHandle(Point3 coord)
    {
      ref TileInfo info = ref Tile[coord];
      if (info.IsNull)
        return;
      if (Tile.TilePointers.TryGetValue(info.WorldCoord2, out Point coreCoord))
      {
        info = ref Tile[new Point3(coreCoord.X, coreCoord.Y, coord.Z)];
        info.Behavior?.OnRefresh(ref info);
        foreach (var script in info.Scripts.Values)
          script.OnRefresh();
      }
      else
      {
        info.Behavior?.OnRefresh(ref info);
        foreach (var script in info.Scripts.Values)
          script.OnRefresh();
      }
      OnTileRefresh?.Invoke(coord);
    }

    public void Dispose()
    {
      OnTileRefresh = null;
    }
  }
}