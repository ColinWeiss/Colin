using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colin.Core.Common;

namespace Colin.Core.Modulars.Tiles
{
    public class Tile : ISceneComponent
    {
        private int _width = 0;
        public int Width => _width;

        private int _height = 0;
        public int Height => _height;

        public Point Half => new Point( _width / 2 , _height / 2 );

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

        public TileInfoCollection infos;

        public TileBehaviorCollection behaviors;

        public void Create(int width, int height)
        {
            _width = width;
            _height = height;
            infos = new TileInfoCollection(width, height);
            behaviors = new TileBehaviorCollection(width, height);
            behaviors.tile = this;
        }

        public void DoInitialize()
        {

        }
        public void DoUpdate(GameTime time)
        {

        }

        public void Place<T>(int coorinateX, int coorinateY) where T : TileBehavior, new()
        {
            infos.CreateTileDefaultInfo(coorinateX, coorinateY);
            behaviors.SetBehavior<T>(coorinateX, coorinateY);
        }
    }
}