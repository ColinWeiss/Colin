namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 表示瓦片地图中的单个瓦片.
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
        public int CoordX;

        /// <summary>
        /// 指示物块的纵坐标.
        /// </summary>
        public int CoordY;

        /// <summary>
        /// 指示物块所处的深度.
        /// </summary>
        public int CoordZ;

        public Vector2 Coordinate => new Vector2( CoordX, CoordY );

        /// <summary>
        /// 指示物块纹理帧格.
        /// </summary>
        public TileFrame Texture;

        /// <summary>
        /// 指示物块的碰撞信息.
        /// </summary>
        public TileCollision Collision;

        public Tile Tile { get; internal set; }

        public ref TileInfo Top
        {
            get
            {
                if(CoordY - 1 >= 0)
                    return ref Tile.Infos[CoordX, CoordY - 1, CoordZ];
                else
                    return ref _null;
            }
        }

        public ref TileInfo Bottom
        {
            get
            {
                if(CoordY + 1 < Tile?.Height)
                    return ref Tile.Infos[CoordX, CoordY + 1, CoordZ];
                else
                    return ref _null;
            }
        }

        public ref TileInfo Left
        {
            get
            {
                if(CoordX - 1 >= 0)
                    return ref Tile.Infos[CoordX - 1, CoordY, CoordZ];
                else
                    return ref _null;
            }
        }

        public ref TileInfo Right
        {
            get
            {
                if(CoordX + 1 < Tile?.Width)
                    return ref Tile.Infos[CoordX + 1, CoordY, CoordZ];
                else
                    return ref _null;
            }
        }

        public ref TileInfo Front
        {
            get
            {
                if( CoordZ - 1 >= 0 )
                    return ref Tile.Infos[CoordX + 1, CoordY, CoordZ - 1 ];
                else
                    return ref _null;
            }
        }

        public ref TileInfo Behind
        {
            get
            {
                if(CoordZ + 1 < Tile.Depth )
                    return ref Tile.Infos[CoordX + 1, CoordY, CoordZ + 1];
                else
                    return ref _null;
            }
        }

        public Dictionary<Type, TileScript> Scripts = new Dictionary<Type, TileScript>();
        public T AddScript<T>() where T : TileScript, new()
        {
            T t = new T();
            t.Tile = Tile;
            t.ID = ID;
            Scripts.Add( t.GetType(), t );
            return t;
        }
        public T GetScript<T>() where T : TileScript => Scripts.GetValueOrDefault( typeof( T ), null ) as T;

        public TileBehavior Behavior = null;

        public RectangleF HitBox => new RectangleF( Coordinate * TileOption.TileSizeF, TileOption.TileSizeF );

        public TileInfo()
        {
            ID = 0;
            Empty = true;
            CoordX = 0;
            CoordY = 0;
            CoordZ = 0;
            Tile = null;
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

        internal static TileInfo _null = new TileInfo();
        public static TileInfo Null => _null;
    }
}