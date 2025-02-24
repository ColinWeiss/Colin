using System.Collections.Concurrent;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Diagnostics;

namespace Colin.Core.Modulars.Tiles
{
  public class TileDestructor : ISceneModule
  {
    public Scene Scene { get; set; }
    public bool Enable { get; set; }

    private Tile _tile;
    public Tile Tile => _tile ??= Scene.GetModule<Tile>();

    private TileRefresher _tileRefresher;
    public TileRefresher TileRefresher => _tileRefresher ??= Scene.GetModule<TileRefresher>();

    private ConcurrentQueue<(Point3, bool)> _queue = new ConcurrentQueue<(Point3, bool)>();
    public ConcurrentQueue<(Point3, bool)> Queue => _queue;

    public void DoInitialize()
    {

    }
    public void Start()
    {
    }
    public void DoUpdate(GameTime time)
    {
      ref TileInfo info = ref Tile[0, 0, 0];
      while (!_queue.IsEmpty)
      {
        if (_queue.TryDequeue(out ValueTuple<Point3, bool> coord))
        {
          info = ref Tile[coord.Item1];
          Handle(coord.Item1, coord.Item2);
        }
      }
    }

    public void Mark(Point3 coord, bool doEvent)
    {
      _queue.Enqueue((coord, doEvent));
    }
    public void Mark(int x, int y, int z, bool doEvent) =>
      Mark(new Point3(x, y, z), doEvent);

    /// <summary>
    /// 用于缓存区块;
    /// <br>若本次操作放置的物块与上次放置的物块属于同一个区块则不需要重新获取.</br>
    /// </summary>
    private TileChunk _chunk;

    private void Handle(Point3 wCoord, bool doEvent)
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

      if (doEvent)
      {
        TileKenel _com = _chunk.TileKenel[info.Index];

        foreach (var handler in _chunk.Handler.Values)
          handler.OnDestructHandle(this, info.Index, info.GetWCoord3());
        _com.OnDestruction(Tile, _chunk, info.Index, info.GetWCoord3());
      }
      info.Empty = true;

      TileRefresher.Mark(info.GetWCoord3(), 1); //将物块标记刷新, 刷新事件交由物块更新器处理
    }

    public void Dispose()
    {

    }
  }
}