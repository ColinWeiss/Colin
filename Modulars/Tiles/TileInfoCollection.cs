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

        public int Length => Tiles.Length;

        public TileInfo[] Tiles;

        public ref TileInfo this[int index] => ref Tiles[index];
        public ref TileInfo this[int x, int y] => ref Tiles[x + y * Width];

        public TileInfoCollection()
        {
            _width = 0;
            _height = 0;
            Tiles = new TileInfo[1];
        }

        public TileInfoCollection( int width, int height )
        {
            _width = width;
            _height = height;
            Tiles = new TileInfo[_width * _height];
            TileInfo _emptyTile = new TileInfo();
            _emptyTile.Empty = true;
            Span<TileInfo> _map = Tiles;
            _map.Fill( _emptyTile );
        }

        public TileInfoCollection( Point size )
        {
            _width = size.X;
            _height = size.Y;
            Tiles = new TileInfo[_width * _height];
            TileInfo _emptyTile = new TileInfo();
            _emptyTile.Empty = true;
            Span<TileInfo> _map = Tiles;
            _map.Fill( _emptyTile );
        }

        internal void CreateTileDefaultInfo( int coordinateX, int coordinateY )
        {
            int id = coordinateX + coordinateY * Width;
            Tiles[id].CoordinateX = coordinateX;
            Tiles[id].CoordinateY = coordinateY;
            Tiles[id].ID = id;
            Tiles[id].Empty = false;
        }

        internal void CreateTileDefaultInfo( int index )
        {
            int id = index;
            Tiles[id].Empty = false;
            Tiles[id].ID = id;
            Tiles[id].CoordinateX = index % Width;
            Tiles[id].CoordinateY = index / Width;
        }

        internal void DeleteTileInfo( int coordinateX, int coordinateY )
        {
            int id = coordinateX + coordinateY * Width;
            Tiles[id].ID = 0;
            Tiles[id].Collision = TileCollision.Passable;
            Tiles[id].Empty = true;
        }

        internal void LoadStep( BinaryReader reader )
        {
            _width = reader.ReadInt32();
            _height = reader.ReadInt32();
            Tiles = new TileInfo[_width * _height];
            for(int count = 0; count < _width * _height; count++)
            {
                Tiles[count] = new TileInfo();
                Tiles[count].LoadStep( reader );
                if(!Tiles[count].Empty)
                    CreateTileDefaultInfo( count );
            }
        }

        internal void SaveStep( BinaryWriter writer )
        {
            writer.Write( _width );
            writer.Write( _height );
            foreach(var info in Tiles)
                info.SaveStep( writer );
        }
    }
}