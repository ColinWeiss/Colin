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

    /// <summary>
    /// 表示物块的刷新信息队列, 其中存放需要刷新的物块的坐标.
    /// </summary>
    public ConcurrentQueue<Point3> RefreshQueue = new();

    /// <summary>
    /// 在物块刷新时发生; 用于模块之间的联动事件.
    /// </summary>
    public event Action<Point3> OnRefresh = null;

    public void DoInitialize()
    {
    }

    public void Start()
    {
    }

    public void DoUpdate(GameTime time)
    {
      ref TileInfo info = ref Tile[0, 0, 0];
      while (RefreshQueue.TryDequeue(out Point3 coord))
      {
        info = ref Tile[coord];
        Handle(coord);
        if (info.Empty)
        {
          info.Behavior = null;
          info.Scripts.Clear();
        }
      }
    }

    public void Mark(Point3 coord, int radius = 0)
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

    /// <summary>
    /// 在刷新环节, 进行物块刷新的处理.
    /// <br>在此处会触发 <see cref="OnRefresh"/> 事件.</br>
    /// </summary>
    /// <param name="coord"></param>
    public void Handle(Point3 coord)
    {
      ref TileInfo info = ref Tile[coord];
      if (info.IsNull)
        return;
      info.Behavior?.OnRefresh(ref info);
      foreach (var script in info.Scripts.Values)
        script.OnRefresh(this);
      OnRefresh?.Invoke(coord);
    }

    public void Dispose()
    {
      OnRefresh = null;
    }
  }
}