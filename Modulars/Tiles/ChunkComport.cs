using SharpDX.Direct3D9;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 区块行为.
  /// <br>[!] 按 SOA 组织一个区块内的物块数据.</br>
  /// </summary>
  public abstract class ChunkComport
  {
    public Tile Tile { get; internal set; }

    public TileChunk Chunk { get; internal set; }

    /// <summary>
    /// 于放置物块时执行.
    /// </summary>
    public virtual void OnPlaceHandle(TilePlacer placer, Point3 wCoord, Point3 iCoord) { }

    /// <summary>
    /// 于物块初始化时执行.
    /// </summary>
    public virtual void OnInitialize() { }

    /// <summary>
    /// 于物块刷新时执行.
    /// </summary>
    public virtual void OnRefreshHandle(TileRefresher refresher, Point3 wCoord, Point3 iCoord) { }

    /// <summary>
    /// 于物块被破坏时执行.
    /// </summary>
    public virtual void OnDestructHandle(TileDestructor destructor, Point3 wCoord, Point3 iCoord) { }

    public virtual void LoadStep(BinaryReader reader) { }

    public virtual void SaveStep(BinaryWriter writer) { }

    /// <summary>
    /// 以坐标转换至索引.
    /// </summary>
    public int GetIndex(int x, int y, int z)
      => Chunk.GetIndex(x, y, z);

    /// <summary>
    /// 以坐标转换至索引.
    /// </summary>
    public int GetIndex(Point3 coord)
      => GetIndex(coord.X, coord.Y, coord.Z);

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