namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 物块基本信息.
    /// </summary>
    public struct TileInfo
    {
        /// <summary>
        /// 指示物块非空状态.
        /// </summary>
        public bool Empty;

        /// <summary>
        /// 指示物块的索引.
        /// </summary>
        public int ID;

        /// <summary>
        /// 指示物块的横坐标.
        /// </summary>
        public int CoordinateX;

        /// <summary>
        /// 指示物块的纵坐标.
        /// </summary>
        public int CoordinateY;

        /// <summary>
        /// 指示物块所处的深度.
        /// </summary>
        public int CoordinateZ;

        public Vector2 CoordinateF => new Vector2( CoordinateX, CoordinateY );

        /// <summary>
        /// 指示物块纹理帧格.
        /// </summary>
        public TileFrame Texture;

        /// <summary>
        /// 指示物块的碰撞信息.
        /// </summary>
        public TileCollision Collision;

        public RectangleF HitBox => new RectangleF( CoordinateF * TileOption.TileSizeF, TileOption.TileSizeF );

        public TileInfo()
        {
            ID = 0;
            Empty = true;
            CoordinateX = 0;
            CoordinateY = 0;
            CoordinateZ = 0;
            Texture = new TileFrame( -1, -1 );
            Collision = TileCollision.Impassable;
        }

        internal void LoadStep( BinaryReader reader )
        {
            Empty = reader.ReadBoolean();
            if(!Empty)
            {
                Texture.LoadStep( reader );
                Collision = (TileCollision)reader.ReadInt32();
            }
        }

        internal void SaveStep( BinaryWriter writer )
        {
            writer.Write( Empty );
            if(!Empty)
            {
                Texture.SaveStep( writer );
                writer.Write( (int)Collision );
            }
        }
    }
}