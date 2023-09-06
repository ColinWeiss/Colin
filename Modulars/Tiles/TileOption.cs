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
        public static readonly int TileW = 16;
        public static readonly int TileH = 16;
        public static Point TileSize => new Point( TileW, TileH );
        public static Vector2 TileSizeF => new Vector2( TileW, TileH );
    }
}