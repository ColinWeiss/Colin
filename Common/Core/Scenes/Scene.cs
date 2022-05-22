using Microsoft.Xna.Framework.Graphics;

namespace Colin.Common.Core.Scenes
{
    /// <summary>
    /// 一个游戏场景.
    /// </summary>
    public class Scene : EngineComponent
    {
        List<SceneContentLayer>? _contentLayers = new List<SceneContentLayer>( );

        protected override void Initialization( )
        {
            for ( int count = 0; count < _contentLayers.Count; count++ )
                _contentLayers[ count ].Initialize( );
            base.Initialization( );
        }

        protected override void UpdateSelf( )
        {
            for ( int count = 0; count < _contentLayers.Count; count++ )
                if ( _contentLayers[ count ].Enable )
                    _contentLayers[ count ].Update( EngineInfo.GameTimeCache );
            base.UpdateSelf( );
        }

        protected override void DrawSelf( SpriteBatch spriteBatch )
        {
            for ( int count = _contentLayers.Count - 1; count >= 0; count-- )
                if ( _contentLayers[ count ].Visable )
                    _contentLayers[ count ].Draw( EngineInfo.GameTimeCache );
            base.DrawSelf( spriteBatch );

        }

        public void Register( SceneContentLayer layer )
        {
            _contentLayers.Add( layer );
        }
    }
}