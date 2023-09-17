namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 瓦片信息集合.
    /// </summary>
    public struct TileInfoCollection
    {
        private int _width;
        public int Width => _width;

        private int _height;
        public int Height => _height;

        private int _depth;
        public int Depth => _depth;

        public int Length => Infos.Length;

        public TileInfo[]? Infos;

        public ref TileInfo this[int index] => ref Infos[index];
        public ref TileInfo this[int x, int y, int z] => ref Infos[z * Width * Height + x + y * Width];

        public TileInfoCollection()
        {
            _width = 0;
            _height = 0;
            _depth = 0;
            Infos = new TileInfo[1];
        }

        public TileInfoCollection( int width, int height, int depth )
        {
            _width = width;
            _height = height;
            _depth = depth;
            Infos = new TileInfo[_width * _height * _depth];
            TileInfo _cache = new TileInfo();
            _cache.Empty = true;
            Span<TileInfo> infos = Infos;
            infos.Fill( _cache );
        }

        internal void CreateTileDefaultInfo( int coordinateX, int coordinateY, int coordinateZ )
        {
            int id = (coordinateZ * Height * Width) + (coordinateX + coordinateY * Width);
            Infos[id].CoordX = coordinateX;
            Infos[id].CoordY = coordinateY;
            Infos[id].CoordZ = coordinateZ;
            Infos[id].ID = id;
            Infos[id].Empty = false;
        }

        internal void CreateTileDefaultInfo( int index )
        {
            int id = index;
            int coord = index % (Width * Height);
            Infos[id].Empty = false;
            Infos[id].ID = id;
            Infos[id].CoordX = coord % Width;
            Infos[id].CoordY = coord / Width;
            Infos[id].CoordZ = index / (Width * Height);
        }

        internal void DeleteTileInfo( int coordinateX, int coordinateY, int coordinateZ )
        {
            int id = (coordinateZ * Width * Height) + (coordinateX + coordinateY * Width);
            Infos[id].Collision = TileCollision.Passable;
            Infos[id].Empty = true;
        }

        internal void DeleteTileInfo( int index )
        {
            int id = index;
            Infos[id].Empty = true;
            Infos[id].Collision = TileCollision.Passable;
        }

        internal void LoadStep( BinaryReader reader )
        {
            _width = reader.ReadInt32();
            _height = reader.ReadInt32();
            _depth = reader.ReadInt32();
            Infos = new TileInfo[_width * _height * _depth];
            for(int count = 0; count < _width * _height * _depth; count++)
            {
                Infos[count] = new TileInfo();
                Infos[count].LoadStep( reader );
                if(!Infos[count].Empty)
                    CreateTileDefaultInfo( count );
            }
        }

        internal void SaveStep( BinaryWriter writer )
        {
            writer.Write( _width );
            writer.Write( _height );
            writer.Write( _depth );
            foreach(var info in Infos)
                info.SaveStep( writer );
        }
    }
}