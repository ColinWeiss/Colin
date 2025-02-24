using Colin.Core.Resources;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块内核.
  /// <br>用以定制分种类物块相关行为.</br>
  /// </summary>
  public class TileKernel : ICodeResource
  {
    private string _identifier;
    public string Identifier
    {
      get
      {
        if (_identifier is null || _identifier == string.Empty)
          _identifier = GetType().FullName;
        return _identifier;
      }
    }

    public ITileSpriteFormat SpriteFormat;

    /// <summary>
    /// 指示该物块行为所属的 Tile 模块.
    /// </summary>
    public Tile Tile { get; internal set; }

    /// <summary>
    /// 执行于判断物块放置标记前.
    /// <br>若结果为 <see langword="true"/>, 则允许进行标记, 否则不进行标记.</br>
    /// </summary>
    public virtual bool CanPlaceMark(Tile tile, TileChunk chunk, int index, Point3 wCoord) => true;

    /// <summary>
    /// 执行于判断物块破坏标记前.
    /// <br>若结果为 <see langword="true"/>, 则允许进行标记, 否则不进行标记.</br>
    /// </summary>
    public virtual bool CanDestructMark(Tile tile, TileChunk chunk, int index, Point3 wCoord) => true;

    /// <summary>
    /// 执行于初始化.
    /// </summary>
    public virtual void OnInitialize(Tile tile, TileChunk chunk, int index) { }

    /// <summary>
    /// 执行于物块放置时.
    /// </summary>
    public virtual void OnPlace(Tile tile, TileChunk chunk, int index, Point3 wCoord) { }

    /// <summary>
    /// 执行于物块刷新时.
    /// </summary>
    public virtual void OnRefresh(Tile tile, TileChunk chunk, int index, Point3 wCoord) { }

    /// <summary>
    /// 执行于物块被破坏时.
    /// </summary>
    public virtual void OnDestruction(Tile tile, TileChunk chunk, int index, Point3 wCoord) { }
  }
}