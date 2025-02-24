using SharpDX.Direct3D9;
using System.Threading;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块行动处理方式.
  /// <br>[!] 按 SOA 组织一个区块内的物块数据.</br>
  /// </summary>
  public abstract class TileHandler
  {
    private static int _countOfHandlerTypes = 0;
    /// <summary>
    /// 类-ID分配器，摒弃Dictionary(Type, TileHandler)的查询开销
    /// </summary>
    public static class HandlerIDHelper<T> where T : TileHandler
    {
      public static readonly int HandlerID = Interlocked.Increment(ref _countOfHandlerTypes) - 1;
    }

    public Tile Tile { get; internal set; }

    public TileChunk Chunk { get; internal set; }

    public int Length => Tile.Context.ChunkWidth * Tile.Context.ChunkHeight * Tile.Context.Depth;

    /// <summary>
    /// 于处理方式被区块加载时执行.
    /// </summary>
    public virtual void DoInitialize() { }

    /// <summary>
    /// 于放置物块时执行.
    /// </summary>
    public virtual void OnPlaceHandle(TilePlacer placer, int index, Point3 wCoord) { }

    /// <summary>
    /// 于物块刷新时执行.
    /// </summary>
    public virtual void OnRefreshHandle(TileRefresher refresher, int index, Point3 wCoord) { }

    /// <summary>
    /// 于物块被破坏时执行.
    /// </summary>
    public virtual void OnDestructHandle(TileDestructor destructor, int index, Point3 wCoord) { }

    public virtual void LoadStep(BinaryReader reader) { }

    public virtual void SaveStep(BinaryWriter writer) { }

    /// <summary>
    /// 判断指定相对于该物块坐标具有指定偏移位置处的物块是否具有相同的行为方式.
    /// </summary>
    /// <returns></returns>
    public bool IsSame(Point3 own, Point3 target)
    {
      return Chunk.IsSame(own, target);
    }
  }
}