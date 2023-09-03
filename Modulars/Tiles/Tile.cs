using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colin.Core.Assets.GameAssets;
using Colin.Core.Common;
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

        public void LoadDatas( BinaryReader reader )
        {
            Create( reader.ReadInt32( ), reader.ReadInt32( ) );
            TileBehavior _behavior;
            string _behaviorName;
            for(int count = 0 ; count < behaviors.Count ; count++)
            {
                _behaviorName = reader.ReadString( );
                if(_behaviorName != null && _behaviorName != "Empty")
                    _behavior = (TileBehavior)Activator.CreateInstance( TileAssets.Get( _behaviorName ).GetType( ) );
                else
                    _behavior = null;
                if(_behavior != null )
                    Place( _behavior, count );
            }
        }

        public void SaveDatas( BinaryWriter writer )
        {
            writer.Write( Width );
            writer.Write( Height );

            TileBehavior _behavior;
            for(int count = 0 ; count < behaviors.Count ; count++)
            {
                _behavior = behaviors[count];
                if(!infos[count].Empty)
                    writer.Write( _behavior.Name );
                else
                    writer.Write( "Empty" );
            }
        }
    }
}