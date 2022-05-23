using Microsoft.Xna.Framework.Graphics;

namespace Colin.Common.Code.Scenes
{
    /// <summary>
    /// 一个游戏场景.
    /// </summary>
    public class Scene : EngineComponent
    {
        List<SceneContentLayer>? _contentLayers = new List<SceneContentLayer>( );

        SceneUILayer _uiLayer;

        protected override void Initialization( )
        {
            for ( int count = 0; count < _contentLayers.Count; count++ )
                _contentLayers[ count ].DoInitialize( );
            _uiLayer.DoInitialize( );
            base.Initialization( );
        }

        protected override void UpdateSelf( )
        {
            for ( int count = 0; count < _contentLayers.Count; count++ )
                if ( _contentLayers[ count ].UpdateEnable )
                    _contentLayers[ count ].DoUpdate( );
            _uiLayer.DoUpdate( );
            base.UpdateSelf( );
        }

        protected override void DrawSelf( SpriteBatch spriteBatch )
        {

            for ( int count = _contentLayers.Count - 1; count >= 0; count-- )
                if ( _contentLayers[ count ].DrawEnable )
                    _contentLayers[ count ].DoDraw( );
            _uiLayer.DoDraw( );
            base.DrawSelf( spriteBatch );
        }

        public void Register( SceneContentLayer layer )
        {
            _contentLayers.Add( layer );
        }

        public void Register( SceneUILayer layer )
        {
            _uiLayer = layer;
        }

    }
}