using System.Diagnostics;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块建造指令.
  /// </summary>
  /// <param name="PlaceOrDestruct">指示该指令控制放置还是破坏.</param>
  /// <param name="WorldCoord">指示该指令所作用的物块坐标, 使用世界坐标.</param>
  /// <param name="Kernel">指示该指令所使用的物块内核.</param>
  /// <param name="DoEvent">指示本次建造指令是否触发Handler及物块本身的相关事件函数.</param>
  public record TileBuildCommand(
    Tile Tile,
    TileBuilder Builder,
    TileRefresher Refresher,
    Point3 WorldCoord,
    TileKernel Kernel,
    bool PlaceOrDestruct,
    bool DoEvent = true,
    int? DoRefresh = 1,
    bool Immediately = false) : IBusinessCase
  {
    static TileChunk _chunkCache;
    public static void ResetCache()
    {
      _chunkCache = null;
    }
    public void Execute()
    {
      var coords = Tile.GetCoords(WorldCoord.X, WorldCoord.Y);
      if (_chunkCache is not null)
      {
        if (_chunkCache.Coord.Equals(coords.cCoord) is false)
          _chunkCache = Tile.GetChunk(coords.cCoord.X, coords.cCoord.Y);
      }
      else
        _chunkCache = Tile.GetChunk(coords.cCoord.X, coords.cCoord.Y);
      if (_chunkCache is null)
        return;

      ref TileInfo info = ref _chunkCache[coords.tCoord.X, coords.tCoord.Y, WorldCoord.Z]; //获取对应坐标的物块格的引用传递.

      if (info.IsNull)
        return;

      if (PlaceOrDestruct)
      {
        Builder.DoPlace(_chunkCache, info.GetICoord3(), Kernel, DoEvent, DoRefresh, Immediately);
      }
      else
      {
        Builder.DoDestruct(_chunkCache, info.GetICoord3(), DoEvent, DoRefresh, Immediately);
      }
    }
  }

  public class TileBuildArgs
  {
    public TileChunk Chunk;
    public int Index;
    public Point3 WorldCoord;
    public TileBuildArgs(TileChunk chunk, int index, Point3 wCoord)
    {
      Chunk = chunk;
      Index = index;
      WorldCoord = wCoord;
    }
  }

  public class TileBuilder : BusinessLine
  {
    private Tile _tile;
    public Tile Tile => _tile ??= Scene.GetModule<Tile>();

    private TileRefresher _refresher;
    public TileRefresher Refresher => _refresher ??= Scene.Business.Get<TileRefresher>();

    public event EventHandler<TileBuildArgs> OnPlaceHandle;

    public event EventHandler<TileBuildArgs> OnDestructHandle;

    /// <summary>
    /// 将指定坐标标记为需要放置物块.
    /// </summary>
    public void MarkPlace(Point3 wCoord, TileKernel kernel, bool doEvent = true, int? doRefresh = 1)
    {
      TileBuildCommand command = new TileBuildCommand(Tile, this, Refresher, wCoord, kernel, true, doEvent, doRefresh);
      Mark(command);
    }

    public void MarkDestruct(Point3 wCoord, bool doEvent = true, int? doRefresh = 1)
    {
      TileBuildCommand command = new TileBuildCommand(Tile, this, Refresher, wCoord, null, false, doEvent, doRefresh);
      Mark(command);
    }

    protected override void OnPrepare() { }

    public void DoPlace(TileChunk _chunk, Point3 cCoord, TileKernel kernel, bool doEvent = true, int? doRefresh = 1, bool immediately = false)
    {
      Debug.Assert(kernel is not null);
      ref TileInfo info = ref _chunk[cCoord.X, cCoord.Y, cCoord.Z];
      Debug.Assert(info.GetICoord3() == cCoord);
      info.Empty = false;
      _chunk.TileKernel[info.Index] = kernel;
      _chunk.TileKernel[info.Index].Tile = Tile;
      _chunk.TileKernel[info.Index].OnInitialize(Tile, _chunk, info.Index);
      Debug.Assert(_chunk.TileKernel[info.Index] == kernel);
      if (doEvent)
      {
        foreach (var handler in _chunk.Handler)
        {
          if (handler.Enable[info.Index])
            handler.OnPlaceHandle(this, info.Index, _chunk.ConvertWorld(cCoord));
        }
        OnPlaceHandle?.Invoke(this, new TileBuildArgs(_chunk, info.Index, _chunk.ConvertWorld(cCoord)));
        _chunk.TileKernel[info.Index]?.OnPlace(Tile, _chunk, info.Index, _chunk.ConvertWorld(cCoord));
      }
      foreach (var handler in _chunk.Handler)
        handler.OnBuildProcess(this, true, info.Index, info.GetWCoord3());
      if (doRefresh is not null)
      {
        Debug.Assert(doRefresh >= 0);
        if (immediately)
          Refresher.DoRefresh(info.GetWCoord3(), doRefresh.Value);
        else
          Refresher.MarkRefresh(info.GetWCoord3(), doRefresh.Value);
      }
    }

    public void DoDestruct(TileChunk _chunk, Point3 cCoord, bool doEvent = true, int? doRefresh = 1, bool immediately = false)
    {
      ref TileInfo info = ref _chunk[cCoord.X, cCoord.Y, cCoord.Z];
      if (doEvent)
      {
        TileKernel _com = _chunk.TileKernel[info.Index];
        foreach (var handler in _chunk.Handler)
        {
          handler.OnDestructHandle(this, info.Index, info.GetWCoord3());
        }
        OnDestructHandle?.Invoke(this, new TileBuildArgs(_chunk, info.Index, _chunk.ConvertWorld(cCoord)));
        _com?.OnDestruction(Tile, _chunk, info.Index, info.GetWCoord3());
      }
      foreach (var handler in _chunk.Handler)
        handler.OnBuildProcess(this, false, info.Index, info.GetWCoord3());
      info.Empty = true;
      info.Collision = TileSolid.None;
      if (doRefresh is not null)
      {
        Debug.Assert(doRefresh >= 0);
        if (immediately)
          Refresher.DoRefresh(info.GetWCoord3(), doRefresh.Value);
        else
          Refresher.MarkRefresh(info.GetWCoord3(), doRefresh.Value);
      }
    }

    public void Dispose()
    {
    }
  }
}