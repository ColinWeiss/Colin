using System.Text.Json;

namespace Colin.Core.Modulars.Tiles
{
    public class Tile : ISceneModule
    {
        private int _width = 0;
        public int Width => _width;

        private int _height = 0;
        public int Height => _height;

        private int _depth = 0;
        public int Depth => _depth;

        public Point Half => new Point( _width / 2, _height / 2 );

        private Scene _scene;
        public Scene Scene
        {
            get => _scene;
            set => _scene = value;
        }

        private bool _enable = false;
        public bool Enable
        {
            get => _enable;
            set => _enable = value;
        }

        public TileInfoCollection Infos;

        /// <summary>
        /// 区块字典.
        /// <br>用于存储当前已被加载进内存的区块.</br>
        /// <br>键: 区块坐标.</br>
        /// <br>值: 区块.</br>
        /// </summary>
        public Dictionary<Point, TileChunk> Chunks = new Dictionary<Point, TileChunk>();
        public TileChunk GetChunk( int x , int y )
        {
            if(Chunks.TryGetValue( new Point( x, y ), out TileChunk chunk ))
                return chunk;
            else
                return null;
        }
        public TileChunk GetChunk( Point coord )
        {
            return GetChunk( coord.X , coord.Y );
        }

        public Tile()
        {
            TileInfo._null.Tile = this;
        }

        public void Create( int width, int height, int depth )
        {
            _width = width;
            _height = height;
            _depth = depth;
            Infos = new TileInfoCollection();
            Infos.Tile = this;
            Infos.Create( width, height, depth );
        }
        public void DoInitialize() { }
        public void Start() { }
        public void DoUpdate( GameTime time ) { }
        public void Dispose()
        {
            Infos = new TileInfoCollection();
        }

        public void Place<T>( int x, int y, int z ) where T : TileBehavior, new() => Infos.Place<T>( x, y, z );

        public void Place( TileBehavior behavior, int x, int y, int z ) => Infos.Place( behavior, x, y, z );

        public void Destruction( int x, int y, int z ) => Infos.Destruction( x, y, z );
    }
}