using System.Collections.Concurrent;
using System.Diagnostics;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块刷新器.
  /// </summary>
  public class TileRefresher : BusinessLine
  {
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

    public ConcurrentDictionary<Point, ConcurrentQueue<Point3>> RefreshQueue = new();

    /// <summary>
    /// 在物块刷新时发生; 用于模块之间的联动事件.
    /// </summary>
    public event Action<Point3> OnRefresh = null;

    public void MarkRefresh(Point3 wCoord, int radius = 0) //标记刷新方法
    {
      Point3 refresh;
      for (int x = -radius; x <= radius; x++)
      {
        for (int y = -radius; y <= radius; y++)
        {
          refresh = new Point3(wCoord.X + x, wCoord.Y + y, wCoord.Z);
          MarkRefresh(refresh);
        }
      }
    }

    public void MarkRefresh(Point3 wCoord)
    {
      var coords = Tile.GetCoords(wCoord.X, wCoord.Y);
      if (RefreshQueue.ContainsKey(coords.cCoord) is false)
        RefreshQueue.TryAdd(coords.cCoord, new ConcurrentQueue<Point3>());
      RefreshQueue[coords.cCoord].Enqueue(new Point3(coords.tCoord, wCoord.Z)); //建队
    }

    public void DoRefresh(TileChunk chunk, int index, Point3 wCoord)
    {
      ref TileInfo info = ref chunk[index]; //获取对应坐标的物块格的引用传递.
      if (info.IsNull)
        return;
      TileKernel _com;
      _com = chunk.TileKernel[index];
      if (!info.Empty)
        Debug.Assert(_com is not null);
      if (_com is null)
        return;
      foreach (var handler in chunk.Handler)
      {
        handler.OnRefreshHandle(chunk, index, wCoord);
        if (info.Empty)
          handler.Enable[info.Index] = false;
      }
      OnRefresh?.Invoke(wCoord);
      _com.OnRefresh(Tile, chunk, index, wCoord);
      if (info.Empty)
      {
        chunk.TileKernel[index] = null;
      }
    }

    public void DoRefresh(Point3 wCoord, int radius = 0) //立刻刷新方法
    {
      var coords = Tile.GetCoords(wCoord.X, wCoord.Y);
      Point3 refresh;
      Point targetChunk;
      for (int x = -radius; x <= radius; x++)
      {
        for (int y = -radius; y <= radius; y++)
        {
          refresh = new Point3(wCoord.X + x, wCoord.Y + y, wCoord.Z);
          targetChunk = Tile.GetChunkCoordForWorldCoord(refresh.X, refresh.Y);
          if (targetChunk != coords.cCoord)
          {
            MarkRefresh(refresh); //跨区块则放入主线程
          }
          else
            Handle(refresh); //本区块则立刻刷新
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
      DoRefresh(_chunk, info.Index, wCoord);
    }

    public void Dispose()
    {
      OnRefresh = null;
    }

    protected override void OnPrepare()
    {
      ConcurrentQueue<Point3> queue;
      TileChunk chunk;
      for (int i = 0; i < Tile.Chunks.Count; i++)
      {
        chunk = Tile.Chunks.ElementAt(i).Value;
        if (chunk.InOperation)
          continue;
        else
        {
          if (RefreshQueue.ContainsKey(chunk.Coord))
          {
            queue = RefreshQueue[chunk.Coord];
            while (queue.TryDequeue(out Point3 cCoord))
            {
              DoRefresh(chunk, chunk.GetIndex(cCoord), chunk.ConvertWorld(cCoord));
            }
          }
        }
      }
    }
  }
}