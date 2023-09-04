using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Colin.Core.Assets.GameAssets;
using Colin.Core.Common;
using Colin.Core.IO;
using Microsoft.Xna.Framework.Audio;

namespace Colin.Core.Modulars.Tiles
{
    public class Tile : ISceneComponent
    {
        private int _width = 0;
        public int Width => _width;

        private int _height = 0;
        public int Height => _height;

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

        public TileInfoCollection infos;

        public TileBehaviorCollection behaviors;

        public void Create( int width, int height )
        {
            _width = width;
            _height = height;
            infos = new TileInfoCollection( width, height );
            behaviors = new TileBehaviorCollection( width, height );
            behaviors.tile = this;
        }

        public void DoInitialize( )
        {

        }

        public void DoUpdate( GameTime time )
        {

        }

        public void Place<T>( int coorinateX, int coorinateY ) where T : TileBehavior, new()
        {
            if(infos[coorinateX, coorinateY].Empty)
            {
                infos.CreateTileDefaultInfo( coorinateX, coorinateY );
                behaviors.SetBehavior<T>( coorinateX, coorinateY );
            }
        }

        public void Place( TileBehavior behavior, int coorinateX, int coorinateY )
        {
            if(infos[coorinateX, coorinateY].Empty)
            {
                infos.CreateTileDefaultInfo( coorinateX, coorinateY );
                behaviors.SetBehavior( behavior, coorinateX, coorinateY );
            }
        }

        public void Place( TileBehavior behavior, int index )
        {
            if(infos[index].Empty)
            {
                infos.CreateTileDefaultInfo( index );
                behaviors.SetBehavior( behavior, index );
            }
        }

        public void Smash( int coorinateX, int coorinateY )
        {
            infos.DeleteTileInfo( coorinateX, coorinateY );
            behaviors.ClearBehavior( coorinateX, coorinateY );
        }

    }
}