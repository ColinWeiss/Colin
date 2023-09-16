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

        public TileBehaviorCollection Behaviors;

        public void Create( int width, int height, int depth )
        {
            _width = width;
            _height = height;
            _depth = depth;
            Infos = new TileInfoCollection( width, height, depth );
            Behaviors = new TileBehaviorCollection( width, height, depth );
            Behaviors.tile = this;
        }

        public void DoInitialize()
        {

        }
        public void Start()
        {

        }
        public void DoUpdate( GameTime time )
        {

        }

        public void Place<T>( int coorX, int coordY, int coordZ ) where T : TileBehavior, new()
        {
            if(Infos[coorX, coordY, coordZ].Empty)
            {
                Infos.CreateTileDefaultInfo( coorX, coordY, coordZ );
                Behaviors.SetBehavior<T>( coorX, coordY, coordZ );
            }
        }

        public void Place<T>( int index ) where T : TileBehavior, new()
        {
            if(Infos[index].Empty)
            {
                Infos.CreateTileDefaultInfo( index );
                Behaviors.SetBehavior<T>( index );
            }
        }

        public void Place( TileBehavior behavior, int index )
        {
            if(Infos[index].Empty)
            {
                Infos.CreateTileDefaultInfo( index );
                Behaviors.SetBehavior( behavior, index );
            }
        }

        public void Smash( int coordX, int coordY, int coordZ )
        {
            Infos.DeleteTileInfo( coordX, coordY, coordZ );
            Behaviors.ClearBehavior( coordX, coordY, coordZ );
        }

        public void Dispose()
        {
            Behaviors = null;
            Infos = new TileInfoCollection();
        }
    }
}