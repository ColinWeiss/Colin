using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Tiles
{
  public class TileSaver : SceneModule
  {
    public Queue<Point> SaveQueue = new Queue<Point>();

    public ConcurrentQueue<Point> AsyncSaveQueue = new ConcurrentQueue<Point>();

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

    public override void DoUpdate(GameTime time)
    {
      while(SaveQueue.Count > 0)
      {
        Point coord = SaveQueue.Dequeue();

      }
      base.DoUpdate(time);
    }
  }
}