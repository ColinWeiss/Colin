using System.Collections.Concurrent;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

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
    /// 用于缓存区块;
    /// <br>若本次操作放置的物块与上次放置的物块属于同一个区块则不需要重新获取.</br>
    /// </summary>
    private TileChunk _chunk;

    /// <summary>
    /// 在刷新环节, 进行物块刷新的处理.
    /// <br>在此处会触发 <see cref="OnRefresh"/> 事件.</br>
    /// </summary>
    /// <param name="wCoord"></param>
    public void Handle(Point3 wCoord)
    {
      var coords = Tile.GetCoords(wCoord.X, wCoord.Y);
      if (_chunk is not null)
      {
        if (_chunk.Coord.Equals(coords.cCoord) is false)
          _chunk = Tile.GetChunk(coords.cCoord.X, coords.cCoord.Y);
      }
      else
        _chunk = Tile.GetChunk(coords.cCoord.X, coords.cCoord.Y);

      if (_chunk is null)
        return;

      ref TileInfo info = ref _chunk[coords.tCoord.X, coords.tCoord.Y, wCoord.Z]; //获取对应坐标的物块格的引用传递.
      if (info.IsNull)
        return;

      TileKernel _com;
      _com = _chunk.TileKernel[info.Index];
      if (_com is null)
        return;
      foreach (var script in _chunk.Handler)
        script.OnRefreshHandle(this, info.Index, wCoord);
      OnRefresh?.Invoke(wCoord);
      _com.OnRefresh(Tile, _chunk, info.Index, wCoord);
      if (info.Empty)
        _chunk.TileKernel[info.Index] = null;
    }

    public void Dispose()
    {
      OnRefresh = null;
    }
  }
}