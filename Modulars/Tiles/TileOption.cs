namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 物块配置.
    /// </summary>
    public class TileOption
    {
        public static readonly int TileWidth = 16;
        public static readonly int TileHeight = 16;
        public static Point TileSize => new Point( TileWidth, TileHeight );
        public static Vector2 TileSizeF => new Vector2( TileWidth, TileHeight );

        public static readonly int ChunkWidth = 50;
        public static readonly int ChunkHeight = 50;
        public static Point ChunkSize => new Point( ChunkWidth, ChunkHeight );
        public static Vector2 ChunkSizeF => new Vector2( ChunkWidth, ChunkHeight );

    }
}