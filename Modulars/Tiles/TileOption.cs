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
        public static Point TileSize => new Point( 16, 16 );
        public static Vector2 TileSizeF => new Vector2( TileSize.X, TileSize.Y );
    }
}