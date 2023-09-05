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

        public TileInfoCollection Infos;

        public TileBehaviorCollection Behaviors;

        public void Create( int width, int height )
        {
            _width = width;
            _height = height;
            Infos = new TileInfoCollection( width, height );
            Behaviors = new TileBehaviorCollection( width, height );
            Behaviors.tile = this;
        }

        public void DoInitialize( )
        {

        }

        public void DoUpdate( GameTime time )
        {

        }

        public void Place<T>( int coorinateX, int coorinateY ) where T : TileBehavior, new()
        {
            if(Infos[coorinateX, coorinateY].Empty)
            {
                Infos.CreateTileDefaultInfo( coorinateX, coorinateY );
                Behaviors.SetBehavior<T>( coorinateX, coorinateY );
            }
        }

        public void Place( TileBehavior behavior, int coorinateX, int coorinateY )
        {
            if(Infos[coorinateX, coorinateY].Empty)
            {
                Infos.CreateTileDefaultInfo( coorinateX, coorinateY );
                Behaviors.SetBehavior( behavior, coorinateX, coorinateY );
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

        public void Smash( int coorinateX, int coorinateY )
        {
            Infos.DeleteTileInfo( coorinateX, coorinateY );
            Behaviors.ClearBehavior( coorinateX, coorinateY );
        }

    }
}