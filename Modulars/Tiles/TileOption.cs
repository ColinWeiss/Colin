using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}