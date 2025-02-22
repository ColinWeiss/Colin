using SharpDX.Direct3D9;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Colin.Core.Modulars.Tiles
{
  public class TilePlacer : ISceneModule
  {
    public Scene Scene { get; set; }
    public bool Enable { get; set; }

    private Tile _tile;
    public Tile Tile => _tile ??= Scene.GetModule<Tile>();

    private TileRefresher _tileRefresher;
    public TileRefresher TileRefresher => _tileRefresher ??= Scene.GetModule<TileRefresher>();

    private ConcurrentQueue<(Point3, TileComport)> _places = new ConcurrentQueue<(Point3, TileComport)>();
    public ConcurrentQueue<(Point3, TileComport)> Places => _places;

    public void DoInitialize()
    {

    }
    public void Start()
    {
    }

    /// <summary>
    /// 物块放置器更新.
    /// </summary>
    /// <param name="time"></param>
    public void DoUpdate(GameTime time)
    {
      ref TileInfo info = ref Tile[0, 0, 0];
      while (!_places.IsEmpty)
      {
        if (_places.TryDequeue(out ValueTuple<Point3, TileComport> element))
        {
          info = ref Tile[element.Item1]; //获取对应坐标的物块格的引用传递.
          Handle(element.Item1, element.Item2);
        }
      }
    }

    /// <summary>
    /// 标记物块放置事件.
    /// </summary>
    /// <param name="wCoord"></param>
    /// <param name="comport"></param>
    public void Mark(Point3 wCoord, TileComport comport)
    {
      _places.Enqueue((wCoord, comport));
    }

    /// <summary>
    /// 标记物块放置事件.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="comport"></param>
    public void Mark(int x, int y, int z, TileComport comport) =>
      Mark(new Point3(x, y, z), comport);

    /// <summary>
    /// 用于缓存区块;
    /// <br>若本次操作放置的物块与上次放置的物块属于同一个区块则不需要重新获取.</br>
    /// </summary>
    private TileChunk _chunk;

    /// <summary>
    /// 处理物块放置事件.
    /// </summary>
    /// <param name="wCoord"></param>
    /// <param name="targetComport"></param>
    public void Handle(Point3 wCoord, TileComport targetComport)
    {
      ref TileInfo info = ref Tile[wCoord]; //获取对应坐标的物块格的引用传递.
      if (info.IsNull)
        return;
      Debug.Assert(info.Empty || !info.IsPointer());

      var coords = Tile.GetCoords(wCoord.X, wCoord.Y);

      if (_chunk is not null)
        if (_chunk.Coord.Equals(coords.tCoord) is false)
          _chunk = Tile.GetChunk(coords.tCoord.X, coords.tCoord.Y);

      TileComport _com;
      int innerIndex = _chunk.GetIndex(_chunk.ConvertInner(wCoord));
      Point3 iCoord = new Point3(coords.tCoord, wCoord.Z);

      _chunk.TileComport[innerIndex] = targetComport;
      _com = _chunk.TileComport[innerIndex];
      _com.Tile = Tile;
      _com.OnInitialize(Tile, _chunk, wCoord, iCoord);

      info.Empty = false;

      foreach (var script in _chunk.ChunkComport.Values)
        script.OnPlaceHandle(this, wCoord, iCoord);
      _com.OnPlace(Tile, _chunk, wCoord, iCoord);

      TileRefresher.Mark(info.GetWCoord3(), 1); //将物块标记刷新, 刷新事件交由物块更新器处理
    }

    public void Dispose()
    {

    }
  }
}
