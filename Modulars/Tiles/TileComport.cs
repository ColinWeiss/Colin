using Colin.Core.Resources;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块行为.
  /// </summary>
  public class TileComport : ICodeResource
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
    private TileSpriteSheet.TileSpriteFormat _spriteSheetCategory = TileSpriteSheet.TileSpriteFormat.Normal;
    public TileSpriteSheet.TileSpriteFormat SpriteSheetCategory => _spriteSheetCategory;

    /// <summary>
    /// 指示该物块行为所属的 Tile 模块.
    /// </summary>
    public Tile Tile { get; internal set; }

    /// <summary>
    /// 执行于判断物块放置标记前.
    /// <br>若结果为 <see langword="true"/>, 则允许进行标记, 否则不进行标记.</br>
    /// </summary>
    public virtual bool CanPlaceMark(Tile tile, TileChunk chunk, Point3 wCoord, Point3 iCoord) => true;

    /// <summary>
    /// 执行于判断物块破坏标记前.
    /// <br>若结果为 <see langword="true"/>, 则允许进行标记, 否则不进行标记.</br>
    /// </summary>
    public virtual bool CanDestructMark(Tile tile, TileChunk chunk, Point3 wCoord, Point3 iCoord) => true;

    /// <summary>
    /// 执行于物块初始化.
    /// </summary>
    public virtual void OnInitialize(Tile tile, TileChunk chunk, Point3 wCoord, Point3 iCoord) { }

    /// <summary>
    /// 执行于物块放置时.
    /// </summary>
    public virtual void OnPlace(Tile tile, TileChunk chunk, Point3 wCoord, Point3 iCoord) { }

    /// <summary>
    /// 执行于物块刷新时.
    /// </summary>
    public virtual void OnRefresh(Tile tile, TileChunk chunk, Point3 wCoord, Point3 iCoord) { }

    /// <summary>
    /// 执行于物块被破坏时.
    /// </summary>
    public virtual void OnDestruction(Tile tile, TileChunk chunk, Point3 wCoord, Point3 iCoord) { }
  }
}