using Colin.Common.Code.Fecs;
using Colin.Common.Code.Physics.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Common.Code.Tiled
{
    /// <summary>
    /// 瓦片地图.
    /// </summary>
    public class TileMap : Entity
    {
        public World world;
        public World World => world;

        public virtual int GridSize => 16;

        private Tile[ , ] tile;
        public Tile[ , ] Tile => tile;

        public int Width;

        public int Height;
    }
}