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
        /// 指示物块在区块内的索引.
        /// </summary>
        public int Index;

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

        /// <summary>
        /// 指示物块在所属区块中的坐标.
        /// </summary>
        public Point ChunkCoord2 => new Point( CoordX, CoordY );

        public Point3 ChunkCoord3 => new Point3( CoordX, CoordY, CoordZ );

        /// <summary>
        /// 指示物块在世界内的坐标.
        /// </summary>
        public Point WorldCoord2 =>
            new Point(
                Chunk.CoordX * TileOption.ChunkWidth + ChunkCoord2.X,
                Chunk.CoordY * TileOption.ChunkHeight + ChunkCoord2.Y );

        /// <summary>
        /// 指示物块在世界内的坐标.
        /// </summary>
        public Point3 WorldCoord3 =>
            new Point3(
                Chunk.CoordX * TileOption.ChunkWidth + ChunkCoord2.X,
                Chunk.CoordY * TileOption.ChunkHeight + ChunkCoord2.Y ,
                CoordZ );

        /// <summary>
        /// 指示物块纹理帧格.
        /// </summary>
        public TileFrame Texture;

        /// <summary>
        /// 指示物块的碰撞信息.
        /// </summary>
        public TileCollision Collision;

        public Tile Tile { get; internal set; }

        /// <summary>
        /// 指示物块所属区块.
        /// </summary>
        public TileChunk Chunk;

        public ref TileInfo Top => ref Tile[WorldCoord2.X, WorldCoord2.Y - 1, CoordZ];

        public ref TileInfo Bottom => ref Tile[WorldCoord2.X, WorldCoord2.Y + 1, CoordZ];

        public ref TileInfo Left => ref Tile[WorldCoord2.X - 1, WorldCoord2.Y, CoordZ];

        public ref TileInfo Right => ref Tile[WorldCoord2.X + 1, WorldCoord2.Y, CoordZ];

        public ref TileInfo Front
        {
            get
            {
                if(CoordZ - 1 >= 0)
                    return ref Tile[WorldCoord2.X, WorldCoord2.Y, CoordZ - 1];
                else
                    return ref _null;
            }
        }
        public ref TileInfo Behind
        {
            get
            {
                if(CoordZ + 1 < Tile.Depth )
                    return ref Tile[WorldCoord2.X, WorldCoord2.Y, CoordZ + 1];
                else
                    return ref _null;
            }
        }
    

        public Dictionary<Type, TileScript> Scripts = new Dictionary<Type, TileScript>();
        public T AddScript<T>() where T : TileScript, new()
        {
            T t = new T();
            t.Tile = Tile;
            t.Chunk = Chunk;
            t.Index = Index;
            Scripts.Add( t.GetType(), t );
            return t;
        }
        public T GetScript<T>() where T : TileScript => Scripts.GetValueOrDefault( typeof( T ), null ) as T;

        public TileBehavior Behavior = null;

        public RectangleF HitBox => new RectangleF( WorldCoord2.ToVector2() * TileOption.TileSizeF, TileOption.TileSizeF );

        public TileInfo()
        {
            Index = 0;
            Empty = true;
            CoordX = 0;
            CoordY = 0;
            CoordZ = 0;
            Tile = null;
            Temp = null;
            Chunk = null;
            Texture = new TileFrame( -1, -1 );
            Collision = TileCollision.Passable;
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
        public static ref TileInfo Null => ref _null;
    }
}