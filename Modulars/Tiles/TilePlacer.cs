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

    private ConcurrentQueue<(Point3, TileBehavior)> _places = new ConcurrentQueue<(Point3, TileBehavior)>();
    public ConcurrentQueue<(Point3, TileBehavior)> Places => _places;

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
        if (_places.TryDequeue(out ValueTuple<Point3, TileBehavior> element))
        {
          info = ref Tile[element.Item1]; //获取对应坐标的物块格的引用传递.
          Handle(element.Item1, element.Item2);
          if (info.Empty)
          {
            info.Behavior = null;
            info.Scripts.Clear();
          }
        }
      }
    }

    /// <summary>
    /// 标记物块放置事件.
    /// </summary>
    /// <param name="coord"></param>
    /// <param name="behavior"></param>
    public void Mark(Point3 coord, TileBehavior behavior)
    {
      _places.Enqueue((coord, behavior));
    }
    /// <summary>
    /// 标记物块放置事件.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="behaivor"></param>
    public void Mark(int x, int y, int z, TileBehavior behaivor) =>
      Mark(new Point3(x, y, z), behaivor);

    /// <summary>
    /// 处理物块放置事件.
    /// </summary>
    /// <param name="coord"></param>
    /// <param name="behavior"></param>
    public void Handle(Point3 coord, TileBehavior behavior)
    {
      ref TileInfo info = ref Tile[coord]; //获取对应坐标的物块格的引用传递.
      if (info.IsNull)
        return;
      Debug.Assert(info.Empty || !info.IsPointer);
      info.Scripts.Clear();//清空脚本
      info.Behavior = behavior;//设置物块行为
      info.Behavior.Tile = Tile;
      info.Behavior.OnInitialize(ref info); //执行行为初始化放置
      info.Behavior.OnScriptAdded(ref info); //执行脚本初始化放置
      foreach (var script in info.Scripts.Values)
      {
        if (script.CanPlace() is false) //判断是否允许放置
        {
          info.Behavior = null;
          info.Scripts.Clear();
          return;
        }
      }
      info.Empty = false;
      info.Behavior.OnPlace(ref info); //触发行为放置事件
      foreach (var script in info.Scripts.Values)
        script.OnPlace(this); //
      TileRefresher.Mark(info.WorldCoord3, 1); //将物块标记刷新, 刷新事件交由物块更新器处理
    }

    public void Dispose()
    {

    }
  }
}
