namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块模块上下文信息;
  /// <br>允许于此处确定与其相关的一些状态.</br>
  /// </summary>
  public interface ITileContext
  {
    short TileLength { get; set; }

    Point TileSize => new Point(TileLength, TileLength);
    Vector2 TileSizeF => new Vector2(TileLength, TileLength);

    short ChunkWidth { get; set; }

    short ChunkHeight { get; set; }

    Point ChunkSize => new Point(ChunkWidth, ChunkHeight);
    Vector2 ChunkSizeF => new Vector2(ChunkWidth, ChunkHeight);

    short Depth { get; set; }

    /// <summary>
    /// 于区块初始化时执行; 允许于此处为区块添加物块处理方式.
    /// <br>[提示] 建议使用 SOA 组织数据的方式为行为附加数据; 并且尽量注意 <see langword="null"/> 相关值的利用.</br>
    /// </summary> 
    void DoTileHandleInit(TileChunk chunk);
  }
}