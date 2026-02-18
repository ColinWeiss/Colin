using Colin.Core.IO;
using System.Threading;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块行动处理方式.
  /// <br>[!] 按 SOA 组织一个区块内的物块数据.</br>
  /// </summary>
  public abstract class TileHandler : IOStep
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
    /// 在指定索引的物块格中启用该Handler.
    /// <br>需要在初始化时确定该值.</br>
    /// </summary>
    public bool[] Enable;

    public virtual bool EnableInitValue => false;

    /// <summary>
    /// 于处理方式被区块加载时执行.
    /// </summary>
    public virtual void DoInitialize() { }

    /// <summary>
    /// 执行于判断物块放置标记前.
    /// <br>若结果为 <see langword="true"/>, 则允许进行标记, 否则不进行标记.</br>
    /// </summary>
    public virtual bool CanPlaceMark(int index, Point3 wCoord) => true;

    /// <summary>
    /// 于放置物块时执行.
    /// </summary>
    public virtual void OnPlaceHandle(TileChunk target, int index, Point3 wCoord) { }

    /// <summary>
    /// 于物块刷新时执行.
    /// </summary>
    public virtual void OnRefreshHandle(TileChunk target, int index, Point3 wCoord) { }

    /// <summary>
    /// 于物块被破坏时执行.
    /// </summary>
    public virtual void OnDestructHandle(TileChunk target, int index, Point3 wCoord) { }

    /// <summary>
    /// 于任何建造过程中执行.
    /// <br>不受Builder中的分支参数控制.</br>
    /// </summary>
    public virtual void OnBuildProcess(TileBuilder builder, bool placeOrDestruct, int index, Point3 wCoord) { }

    public virtual StoreBox SaveStep() => new StoreBox();

    public virtual void LoadStep(StoreBox box) { }

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