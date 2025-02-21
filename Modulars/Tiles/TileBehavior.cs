using Colin.Core.Resources;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块行为.
  /// </summary>
  public class TileBehavior : ICodeResource
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

    public Tile Tile { get; internal set; }

    /// <summary>
    /// 执行于判断物块放置标记前.
    /// <br>若结果为 <see langword="true"/>, 则允许进行标记, 否则不进行标记.</br>
    /// </summary>
    public virtual bool CanPlaceMark(ref TileInfo info) => true;

    /// <summary>
    /// 执行于物块初始化.
    /// </summary>
    /// <param name="info"></param>
    public virtual void OnInitialize(ref TileInfo info) { }

    /// <summary>
    /// 执行于添加 <see cref="TileScript"/> 时.
    /// </summary>
    /// <param name="info"></param>
    public virtual void OnScriptAdded(ref TileInfo info) { }

    /// <summary>
    /// 执行于物块放置时.
    /// </summary>
    public virtual void OnPlace(ref TileInfo info) { }

    /// <summary>
    /// 执行于物块刷新时.
    /// </summary>
    public virtual void OnRefresh(ref TileInfo info) { }

    /// <summary>
    /// 执行于物块被破坏时.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="conduct"></param>
    public virtual void OnDestruction(ref TileInfo info) { }

    public T AddScript<T>(ref TileInfo info) where T : TileScript, new() => info.AddScript<T>();

    public T GetScript<T>(ref TileInfo info) where T : TileScript, new() => info.GetScript<T>();
  }
}