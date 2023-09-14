using Colin.Core.Assets.GameAssets;
using System.Text.Json;

namespace Colin.Core.Modulars.Tiles
{
    public class TileBehaviorCollection
    {
        internal Tile tile;
        public Tile Tile => tile;
        public int Width { get; }
        public int Height { get; }
        public int Length => _behaviors.Length;
        private TileBehavior[] _behaviors;
        public TileBehavior this[int index] => _behaviors[index];
        public TileBehavior this[int x, int y] => _behaviors[x + y * Width];
        public TileBehaviorCollection( int width, int height )
        {
            Width = width;
            Height = height;
            _behaviors = new TileBehavior[width * height];
            for(int count = 0; count < _behaviors.Length - 1; count++)
                _behaviors[count] = new TileBehavior();
        }
        public TileBehaviorCollection( Point size )
        {
            Width = size.X;
            Height = size.Y;
            _behaviors = new TileBehavior[size.X * size.Y];
            for(int count = 0; count < _behaviors.Length; count++)
            {
                _behaviors[count] = new TileBehavior();
            }
        }

        public void SetBehavior<T>( int x, int y ) where T : TileBehavior, new()
        {
            _behaviors[x + y * Width] = new T();
            _behaviors[x + y * Width]._tile = tile;
            T _behavior = _behaviors[x + y * Width] as T;
            _behavior._tile = tile;
            _behavior.coordinateX = x;
            _behavior.coordinateY = y;
            _behavior.id = x + y * Width;
            _behavior.SetDefaults();
            _behavior.DoRefresh( 1 );
        }

        public void SetBehavior<T>( int index ) where T : TileBehavior, new()
        {
            SetBehavior<T>( index % Height , index % Width );
        }

        public void SetBehavior( TileBehavior behavior, int x, int y )
        {
            _behaviors[x + y * Width] = behavior;
            _behaviors[x + y * Width]._tile = tile;
            _behaviors[x + y * Width].coordinateX = x;
            _behaviors[x + y * Width].coordinateY = y;
            _behaviors[x + y * Width].id = x + y * Width;
            _behaviors[x + y * Width].SetDefaults();
            _behaviors[x + y * Width].DoRefresh( 1 );
        }

        public void SetBehavior( TileBehavior behavior, int index )
        {
            _behaviors[index] = behavior;
            _behaviors[index]._tile = tile;
            _behaviors[index].coordinateX = index % Height;
            _behaviors[index].coordinateY = index % Width;
            _behaviors[index].id = index;
            _behaviors[index].SetDefaults();
            _behaviors[index].DoRefresh( 1 );
        }

        public void ClearBehavior( int x, int y )
        {
            _behaviors[x + y * Width].DoRefresh( 1 );
            _behaviors[x + y * Width] = new TileBehavior();
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