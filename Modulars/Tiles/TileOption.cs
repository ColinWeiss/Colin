namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块配置.
  /// </summary>
  public class TileOption
  {
    public int TileWidth;
    public int TileHeight;
    public Point TileSize => new Point(TileWidth, TileHeight);
    public Vector2 TileSizeF => new Vector2(TileWidth, TileHeight);

    public int ChunkWidth;
    public int ChunkHeight;
    public Point ChunkSize => new Point(ChunkWidth, ChunkHeight);
    public Vector2 ChunkSizeF => new Vector2(ChunkWidth, ChunkHeight);
  }
}