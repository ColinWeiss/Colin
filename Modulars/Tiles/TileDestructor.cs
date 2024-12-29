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
        if (_queue.TryDequeue(out ValueTuple<Point3,bool> coord))
        {
          info = ref Tile[coord.Item1];
          Handle(coord.Item1, coord.Item2);
          if (info.Empty)
          {
            info.Behavior = null;
            info.Scripts.Clear();
          }
        }
      }
    }

    public void Mark(Point3 coord, bool doEvent)
    {
      _queue.Enqueue((coord, doEvent));
    }
    public void Mark(int x, int y, int z, bool doEvent) =>
      Mark(new Point3(x, y, z), doEvent);

    private void Handle(Point3 coord, bool doEvent)
    {
      ref TileInfo info = ref Tile[coord];
      if (info.IsNull)
        return;
      if (info.Empty is false)
      {
        if(doEvent)
        {
          info.Behavior.OnDestruction(ref info);
          foreach (var script in info.Scripts.Values)
            script.OnDestruction(this);
        }
        info.Empty = true;
        TileRefresher.Mark(info.WorldCoord3, 1); //将物块标记刷新, 刷新事件交由物块更新器处理
      }
    }

    public void Dispose()
    {

    }
  }
}