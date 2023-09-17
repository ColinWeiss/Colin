using Colin.Core.Assets.GameAssets;
using System;
using System.Reflection;
using System.Text.Json;

namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 瓦片信息集合.
    /// </summary>
    public struct TileInfoCollection
    {
        public Tile Tile { get; internal set; } = null;

        private int _width;
        public int Width => _width;

        private int _height;
        public int Height => _height;

        private int _depth;
        public int Depth => _depth;

        public int Length => Infos.Length;

        public TileInfo[] Infos;

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

        public void Place<T>( int x, int y, int z ) where T : TileBehavior, new()
        {
            if(this[x, y, z].Empty)
            {
                CreateTileDefaultInfo( x, y, z );
                this[x, y, z].Behavior = TileAssets.Get<T>();
                this[x, y, z].Behavior.Tile = Tile;
                this[x, y, z].Behavior.OnPlace( ref this[x, y, z] );
                this[x, y, z].Behavior.DoRefresh( ref this[x, y, z], 1 );
            }
        }

        public void Place<T>( int index ) where T : TileBehavior, new()
        {
            CreateTileDefaultInfo( index );
            this[index].Behavior = TileAssets.Get<T>();
            this[index].Behavior.Tile = Tile;
            this[index].Behavior.OnPlace( ref this[index] );
            this[index].Behavior.DoRefresh( ref this[index], 1 );
        }

        public void Place( TileBehavior behavior, int x, int y, int z )
        {
            CreateTileDefaultInfo( x, y, z );
            this[x, y, z].Behavior = behavior;
            this[x, y, z].Behavior.Tile = Tile;
            this[x, y, z].Behavior.OnPlace( ref this[x, y, z] );
            this[x, y, z].Behavior.DoRefresh( ref this[x, y, z], 1 );
        }

        public void Place( TileBehavior behavior, int index )
        {
            CreateTileDefaultInfo( index );
            this[index].Behavior = behavior;
            this[index].Behavior.Tile = Tile;
            this[index].Behavior.OnPlace( ref this[index] );
            this[index].Behavior.DoRefresh( ref this[index], 1 );
        }

        public void Place( Type behaviorType, int x, int y, int z )
        {
            CreateTileDefaultInfo( x, y, z );
            this[x, y, z].Behavior = TileAssets.Get( behaviorType );
            this[x, y, z].Behavior.Tile = Tile;
            this[x, y, z].Behavior.OnPlace( ref this[x, y, z] );
            this[x, y, z].Behavior.DoRefresh( ref this[x, y, z], 1 );
        }

        public void Place( Type behaviorType, int index )
        {
            CreateTileDefaultInfo( index );
            this[index].Behavior = TileAssets.Get( behaviorType );
            this[index].Behavior.Tile = Tile;
            this[index].Behavior.OnPlace( ref this[index] );
            this[index].Behavior.DoRefresh( ref this[index], 1 );
        }

        public void Destruction( int x, int y, int z )
        {
            if( !this[x, y, z].Empty )
            {
                DeleteTileInfo( x, y, z );
                this[x, y, z].Behavior.DoDestruction( ref this[x, y, z] );
                this[x, y, z].Behavior.DoRefresh( ref this[x, y, z] );
                this[x, y, z].Behavior = null;
            }
        }

        internal void CreateTileDefaultInfo( int x, int y, int z )
        {
            int id = (z * Height * Width) + (x + y * Width);
            Infos[id].Tile = Tile;
            Infos[id].CoordX = x;
            Infos[id].CoordY = y;
            Infos[id].CoordZ = z;
            Infos[id].ID = id;
            Infos[id].Empty = false;
        }

        internal void CreateTileDefaultInfo( int index )
        {
            int id = index;
            int coord = index % (Width * Height);
            Infos[id].Tile = Tile;
            Infos[id].Empty = false;
            Infos[id].ID = id;
            Infos[id].CoordX = coord % Width;
            Infos[id].CoordY = coord / Width;
            Infos[id].CoordZ = index / (Width * Height);
        }

        internal void DeleteTileInfo( int x, int y, int z )
        {
            int id = (z * Width * Height) + (x + y * Width);
            Infos[id].Collision = TileCollision.Passable;
            Infos[id].Empty = true;
        }

        internal void DeleteTileInfo( int index )
        {
            int id = index;
            Infos[id].Empty = true;
            Infos[id].Collision = TileCollision.Passable;
        }

        internal void LoadStep( string tablePath, BinaryReader reader )
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
            Dictionary<string, int> _cache = new Dictionary<string, int>();
            using(FileStream fileStream = new FileStream( tablePath, FileMode.Open ))
            {
                _cache = (Dictionary<string, int>)JsonSerializer.Deserialize( fileStream, typeof( Dictionary<string, int> ) );
            }
            List<string> _indexMap = _cache.Keys.ToList();
            int _index = 0;
            Type _behaviorType;
            for(int count = 0; count < Length - 1; count++)
            {
                _index = reader.ReadInt32();
                if(_index != -1)
                {
                    _behaviorType = TileAssets.Get( _indexMap[_index] ).GetType();
                    Place( _behaviorType, count );
                }
            }
        }

        internal void SaveStep( string tablePath, BinaryWriter writer )
        {
            writer.Write( _width );
            writer.Write( _height );
            writer.Write( _depth );
            foreach(var info in Infos)
                info.SaveStep( writer );
            Dictionary<string, int> _cache = new Dictionary<string, int>();
            int index = 0;
            TileAssets.IdentDic.Keys.ToList().ForEach( v => { _cache.Add( v, index++ ); } );
            using(FileStream fileStream = new FileStream( tablePath, FileMode.OpenOrCreate ))
            {
                JsonSerializer.Serialize( fileStream, _cache );
            }
            for(int count = 0; count < Length - 1; count++)
            {
                if(_cache.TryGetValue( this[count].Behavior.Identifier, out int value ))
                    writer.Write( value );
                else
                    writer.Write( -1 );
            }
        }
    }
}