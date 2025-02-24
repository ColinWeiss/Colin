﻿using SharpDX.Direct3D9;
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

    private ConcurrentQueue<(Point3, TileKernel)> _places = new ConcurrentQueue<(Point3, TileKernel)>();
    public ConcurrentQueue<(Point3, TileKernel)> Places => _places;

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
        if (_places.TryDequeue(out ValueTuple<Point3, TileKernel> element))
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
    public void Mark(Point3 wCoord, TileKernel comport)
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
    public void Mark(int x, int y, int z, TileKernel comport) =>
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
    public void Handle(Point3 wCoord, TileKernel targetComport)
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

      Debug.Assert(info.Empty || !info.IsPointer);

      Point3 iCoord = new Point3(coords.tCoord, wCoord.Z);

      _chunk.TileKernel[info.Index] = targetComport;
      _chunk.TileKernel[info.Index] = _chunk.TileKernel[info.Index];
      _chunk.TileKernel[info.Index].Tile = Tile;
      _chunk.TileKernel[info.Index].OnInitialize(Tile, _chunk, info.Index);

      info.Empty = false;

      foreach (var handler in _chunk.Handler)
        handler.OnPlaceHandle(this, info.Index, wCoord);
      _chunk.TileKernel[info.Index].OnPlace(Tile, _chunk, info.Index, wCoord);

      TileRefresher.Mark(info.GetWCoord3(), 1); //将物块标记刷新, 刷新事件交由物块更新器处理
    }

    public void Dispose()
    {

    }
  }
}
