﻿using Colin.Core.Resources;
using System.Collections.Concurrent;

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
    public void DoUpdate(GameTime time)
    {
      ref TileInfo info = ref Tile[0, 0, 0];
      while (!_places.IsEmpty)
      {
        if (_places.TryDequeue(out ValueTuple<Point3, TileBehavior> element))
        {
          info = ref Tile[element.Item1];
          Handle(element.Item1, element.Item2);
          if (info.Empty)
          {
            info.Behavior = null;
            info.Scripts.Clear();
          }
        }
      }
    }

    public void Mark(Point3 coord, TileBehavior behavior)
    {
      _places.Enqueue((coord, behavior));
    }
    public void Mark(int x, int y, int z, TileBehavior behaivor) =>
      Mark(new Point3(x, y, z), behaivor);

    public void Handle(Point3 coord, TileBehavior behavior)
    {
      ref TileInfo info = ref Tile[coord]; //获取对应坐标的物块格的引用传递.
      if (info.IsNull)
        return;
      info.Scripts.Clear();
      info.Behavior = behavior;
      info.Behavior.Tile = Tile;
      info.Behavior.OnInitialize(ref info); //执行行为初始化放置
      info.Behavior.OnScriptAdded(ref info);
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
        script.OnPlace(this);
      TileRefresher.Mark(info.WorldCoord3, 1); //将物块标记刷新, 刷新事件交由物块更新器处理
    }

    public void Dispose()
    {

    }
  }
}
