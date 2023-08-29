using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Modulars.Tiles
{
    public class TileBehavior
    {
        internal Tile tile;
        public Tile Tile => tile;

        internal int id = -1;
        public int ID => id;

        internal int coordinateX;
        public int CoordinateX => coordinateX;

        internal int coordinateY;
        public int CoordinateY => coordinateY;

        public TileInfo Info => Tile.infos[ID];

        public virtual void SetDefaults() { }
        public virtual void UpdateTile(int coordinateX, int coordinateY) { }
        public virtual void RenderTexture(int coordinateX, int coordinateY) { }
        public virtual void RenderBorder(int coordinateX, int coordinateY) { }
        /// <summary>
        /// 执行一次物块更新.
        /// </summary>
        public virtual void OnRefresh() { }
    }
}