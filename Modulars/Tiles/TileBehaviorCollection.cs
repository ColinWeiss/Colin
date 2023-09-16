using Colin.Core.Assets.GameAssets;
using System;
using System.Text.Json;

namespace Colin.Core.Modulars.Tiles
{
    public class TileBehaviorCollection
    {
        internal Tile tile;
        public Tile Tile => tile;
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public int Length => _behaviors.Length;

        private TileBehavior[] _behaviors;

        /// <summary>
        /// 获取具有指定 ID 的 <see cref="TileBehavior"/>.
        /// </summary>
        /// <param name="index">ID.</param>
        /// <returns></returns>
        public TileBehavior this[int index] => _behaviors[index];

        /// <summary>
        /// 获取指定位置的 <see cref="TileBehavior"/>.
        /// </summary>
        /// <param name="x">横坐标.</param>
        /// <param name="y">纵坐标.</param>
        /// <param name="z">所属层数.</param>
        /// <returns></returns>
        public TileBehavior this[int x, int y, int z] => _behaviors[z * Width * Height + x + y * Width];

        public TileBehaviorCollection( int width, int height, int depth )
        {
            Width = width;
            Height = height;
            Depth = depth;
            _behaviors = new TileBehavior[Width * Height * Depth];
            for(int count = 0; count < _behaviors.Length - 1; count++)
                _behaviors[count] = new TileBehavior();
        }

        public void SetBehavior<T>( int x, int y, int z ) where T : TileBehavior, new()
        {
            int id = z * Width * Height + x + y * Width;
            _behaviors[id] = new T();
            _behaviors[id]._tile = tile;
            T _behavior = _behaviors[id] as T;
            _behavior._tile = tile;
            _behavior.coordinateX = x;
            _behavior.coordinateY = y;
            _behavior.coordinateZ = z;
            _behavior.id = id;
            _behavior.SetDefaults();
            _behavior.DoRefresh( 1 );
        }

        public void SetBehavior<T>( int index ) where T : TileBehavior, new()
        {
            int id = index;
            int coord = index % (Width * Height);
            _behaviors[id] = new T();
            _behaviors[id]._tile = tile;
            T _behavior = _behaviors[id] as T;
            _behavior._tile = tile;
            _behavior.coordinateX = coord % Width;
            _behavior.coordinateY = coord / Width;
            _behavior.coordinateZ = index / (Width * Height);
            _behavior.id = id;
            _behavior.SetDefaults();
            _behavior.DoRefresh( 1 );
        }

        public void SetBehavior( TileBehavior behavior, int x, int y, int z )
        {
            int id = z * Width * Height + x + y * Width;
            _behaviors[id] = behavior;
            _behaviors[id]._tile = tile;
            _behaviors[id].coordinateX = x;
            _behaviors[id].coordinateY = y;
            _behaviors[id].coordinateZ = z;
            _behaviors[id].id = id;
            _behaviors[id].SetDefaults();
            _behaviors[id].DoRefresh( 1 );
        }

        public void SetBehavior( TileBehavior behavior, int index )
        {
            int id = index;
            int coord = index % (Width * Height);
            _behaviors[id] = behavior;
            _behaviors[id]._tile = tile;
            _behaviors[id].coordinateX = coord % Width;
            _behaviors[id].coordinateY = coord / Width;
            _behaviors[id].coordinateZ = index / (Width * Height);
            _behaviors[id].id = id;
            _behaviors[id].SetDefaults();
            _behaviors[id].DoRefresh( 1 );
        }

        public void ClearBehavior( int x, int y, int z )
        {
            int id = z * Width * Height + x + y * Width;
            _behaviors[id].DoRefresh( 1 );
            _behaviors[id] = new TileBehavior();
        }

        public void LoadStep( string tablePath, BinaryReader reader )
        {
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
                    SetBehavior( (TileBehavior)Activator.CreateInstance( _behaviorType ), count );
                }
            }
        }

        public void SaveStep( string tablePath, BinaryWriter writer )
        {
            Dictionary<string, int> _cache = new Dictionary<string, int>();
            int index = 0;
            TileAssets.Behaviors.Keys.ToList().ForEach( v => { _cache.Add( v, index++ ); } );
            using(FileStream fileStream = new FileStream( tablePath, FileMode.OpenOrCreate ))
            {
                JsonSerializer.Serialize( fileStream, _cache );
            }
            for(int count = 0; count < Length - 1; count++)
            {
                if(_cache.TryGetValue( _behaviors[count].Name, out int value ))
                    writer.Write( value );
                else
                    writer.Write( -1 );
            }
        }
    }
}