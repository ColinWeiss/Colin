using Colin.Core.Assets.GameAssets;
using DeltaMachine.Core.GameContents.GameSystems;
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

        public void Create( int width, int height, int depth )
        {
            _width = width;
            _height = height;
            _depth = depth;
            Infos = new TileInfo[_width * _height * _depth];
            int coord;
            for(int count = 0; count < Infos.Length; count++)
            {
                coord = count % (Width * Height);
                Infos[count] = new TileInfo();
                Infos[count].Tile = Tile;
                Infos[count].ID = count;
                Infos[count].CoordX = coord % Width;
                Infos[count].CoordY = coord / Width;
                Infos[count].CoordZ = count / (Width * Height);
            }
        }

        public void Place<T>( int x, int y, int z ) where T : TileBehavior, new()
        {
            if(this[x, y, z].Empty)
            {
                CreateTileDefaultInfo( x, y, z );
                Set<T>( x, y, z );
                this[x, y, z].Behavior.OnPlace( ref this[x, y, z] );
                this[x, y, z].Behavior.DoRefresh( ref this[x, y, z], 1 );
            }
        }

        public void Place<T>( int index ) where T : TileBehavior, new()
        {
            CreateTileDefaultInfo( index );
            Set<T>( index );
            this[index].Behavior.OnPlace( ref this[index] );
            this[index].Behavior.DoRefresh( ref this[index], 1 );
        }

        public void Place( TileBehavior behavior, int x, int y, int z )
        {
            CreateTileDefaultInfo( x, y, z );
            Set( behavior , x , y , z );
            this[x, y, z].Behavior.OnPlace( ref this[x, y, z] );
            this[x, y, z].Behavior.DoRefresh( ref this[x, y, z], 1 );
        }

        public void Place( TileBehavior behavior, int index )
        {
            CreateTileDefaultInfo( index );
            Set( behavior, index );
            this[index].Behavior.Tile = Tile;
            this[index].Behavior.OnPlace( ref this[index] );
            this[index].Behavior.DoRefresh( ref this[index], 1 );
        }

        public void Set<T>( int x, int y, int z ) where T : TileBehavior, new()
        {
            this[x, y, z].Behavior = TileAssets.Get<T>();
            this[x, y, z].Behavior.Tile = Tile;
            this[x, y, z].Behavior.OnInitialize( ref this[x, y, z] );
        }

        public void Set<T>( int index ) where T : TileBehavior, new()
        {
            this[index].Behavior = TileAssets.Get<T>();
            this[index].Behavior.Tile = Tile;
            this[index].Behavior.OnInitialize( ref this[index] );
        }

        public void Set( TileBehavior behavior, int x, int y, int z )
        {
            this[x, y, z].Behavior = behavior;
            this[x, y, z].Behavior.Tile = Tile;
            this[x, y, z].Behavior.OnInitialize( ref this[x, y, z] );
        }

        public void Set( TileBehavior behavior, int index )
        {
            this[index].Behavior = behavior;
            this[index].Behavior.Tile = Tile;
            this[index].Behavior.OnInitialize( ref this[index] );
        }

        public void Destruction( int x, int y, int z )
        {
            if(!this[x, y, z].Empty)
            {
                DeleteTileInfo( x, y, z );
                this[x, y, z].Behavior.DoDestruction( ref this[x, y, z] );
                this[x, y, z].Behavior.DoRefresh( ref this[x, y, z] );
                this[x, y, z].Behavior = null;
                this[x, y, z].Scripts.Clear();
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
                else
                {
                    int id = count;
                    int coord = count % (Width * Height);
                    Infos[id].Tile = Tile;
                    Infos[id].ID = id;
                    Infos[id].CoordX = coord % Width;
                    Infos[id].CoordY = coord / Width;
                    Infos[id].CoordZ = count / (Width * Height);
                }
            }
            Dictionary<string, int> _cache = new Dictionary<string, int>();
            using(FileStream fileStream = new FileStream( tablePath, FileMode.Open ))
            {
                _cache = (Dictionary<string, int>)JsonSerializer.Deserialize( fileStream, typeof( Dictionary<string, int> ) );
            }
            List<string> _indexMap = _cache.Keys.ToList();
            int _index = 0;
            TileBehavior behavior;
            for(int count = 0; count < Length - 1; count++)
            {
                _index = reader.ReadInt32();
                if(_index != -1)
                    Set( TileAssets.Get( _indexMap[_index] ) , count );
            }
            for(int count = 0; count < Length - 1; count++)
                Infos[count].Behavior?.DoRefresh( ref Infos[count], 1 );
            for(int count = 0; count < Length - 1; count++)
                for(int scriptCount = 0; scriptCount < Infos[count].Scripts.Count; scriptCount++)
                    Infos[count].Scripts.Values.ElementAt( scriptCount ).LoadStep( reader );
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
                string ident = this[count].Behavior?.Identifier;
                if(!string.IsNullOrEmpty( ident ))
                {
                    if(_cache.TryGetValue( this[count].Behavior.Identifier, out int value ))
                        writer.Write( value );
                }
                else
                    writer.Write( -1 );
            }
            for(int count = 0; count < Length - 1; count++)
                for(int scriptCount = 0; scriptCount < Infos[count].Scripts.Count; scriptCount++)
                    Infos[count].Scripts.Values.ElementAt( scriptCount ).SaveStep( writer );
        }
    }
}