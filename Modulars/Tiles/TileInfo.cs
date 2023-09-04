using System.Runtime.Serialization;

namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 物块基本信息.
    /// </summary>
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

        public RectangleF HitBox => new RectangleF( CoordinateF * TileOption.TileSizeF , TileOption.TileSizeF );

        public TileInfo( )
        {
            ID = 0;
            Empty = true;
            CoordinateX = 0;
            CoordinateY = 0;
            Texture = new TileFrame(-1, -1);
            Border = new TileFrame(-1, -1);
            Collision = TileCollision.Impassable;
        }

        internal void LoadStep( BinaryReader reader )
        {
            ID = reader.ReadInt32( );
            Empty = reader.ReadBoolean( );
            CoordinateX = reader.ReadInt32( );
            CoordinateY = reader.ReadInt32( );
            Texture.LoadStep( reader );
            Border.LoadStep( reader );
            Collision = (TileCollision)reader.ReadInt32( );
        }

        internal void SaveStep( BinaryWriter writer )
        {
            writer.Write( ID );
            writer.Write( Empty );
            writer.Write( CoordinateX );
            writer.Write( CoordinateY );
            Texture.SaveStep( writer );
            Border.SaveStep( writer );
            writer.Write( (int)Collision );
        }
    }
}