using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 物块基本信息.
    /// </summary>
    [Serializable]
    public struct TileInfo
    {
        /// <summary>
        /// 指示物块在区块内的索引.
        /// </summary>
        public int ID;

        /// <summary>
        /// 指示物块非空状态.
        /// </summary>
        public bool Empty;

        /// <summary>
        /// 指示物块的横坐标.
        /// </summary>
        public int CoordinateX;

        /// <summary>
        /// 指示物块的纵坐标.
        /// </summary>
        public int CoordinateY;

        public Point Coordinate => new Point( CoordinateX , CoordinateY );
        public Vector2 CoordinateF => new Vector2( CoordinateX, CoordinateY );

        /// <summary>
        /// 指示物块纹理帧格.
        /// </summary>
        public TileFrame Texture;

        /// <summary>
        /// 指示物块边框帧格.
        /// </summary>
        public TileFrame Border;

        /// <summary>
        /// 指示物块的碰撞信息.
        /// </summary>
        public TileCollision Collision;

        public Rectangle HitBox => new Rectangle( (CoordinateF * TileOption.TileSizeF + TileOption.TileSizeF / 2).ToPoint( ), TileOption.TileSize );

        public TileInfo()
        {
            ID = 0;
            Empty = true;
            CoordinateX = 0;
            CoordinateY = 0;
            Texture = new TileFrame(-1, -1);
            Border = new TileFrame(-1, -1);
            Collision = TileCollision.Impassable;
        }
    }
}