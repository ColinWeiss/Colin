using System.Collections.Concurrent;

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

    private ConcurrentQueue<Point3> _destructs = new ConcurrentQueue<Point3>();
    public ConcurrentQueue<Point3> Destructs => _destructs;

    public void DoInitialize()
    {

    }
    public void Start()
    {
    }
    public void DoUpdate(GameTime time)
    {
      ref TileInfo info = ref Tile[0, 0, 0];
      while (!_destructs.IsEmpty)
      {
        if (_destructs.TryDequeue(out Point3 coord))
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
    }

    public void Mark(Point3 coord)
    {
      _destructs.Enqueue(coord);
    }
    public void Mark(int x, int y, int z) =>
      Mark(new Point3(x, y, z));

    private void Handle(Point3 coord)
    {
      ref TileInfo info = ref Tile[coord]; //获取对应坐标的物块格的引用传递.
      if (info.IsNull)
        return;
      if (!Tile.TilePointers.ContainsKey(info.WorldCoord2) && !info.Empty) //如果该格并非物块指针, 且不为空
      {
        info.Behavior.OnDestruction(ref info);
        foreach (var script in info.Scripts.Values)
          script.OnDestruction();
        info.Empty = true;
        TileRefresher.Mark(info.WorldCoord3, 1); //将物块标记刷新, 刷新事件交由物块更新器处理
      }
    }

    public void Dispose()
    {

    }
  }
}