using Colin.Common.Code.Physics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Colin.Common.Code.Scenes
{
    /// <summary>
    /// 一个游戏场景.
    /// </summary>
    public abstract class Scene : EngineComponent
    {
        List<SceneContentLayer>? _contentLayers = new List<SceneContentLayer>( );

        SceneUserInterfaceLayer _uiLayer;

        public World world;
        public World World => world;

        public Scene( )
        {
            world = new World(Vector2.UnitY);
        }

        /// <summary>
        /// 设置场景的用户交互界面内容层.
        /// </summary>
        /// <returns></returns>
        protected abstract SceneUserInterfaceLayer SetUserInterface( );

        protected override void Initialization( )
        {
            _uiLayer = SetUserInterface( );
            for( int count = 0; count < _contentLayers.Count; count++ )
                _contentLayers[count].DoInitialize( );
            _uiLayer.DoInitialize( );
            base.Initialization( );
        }

        protected override void UpdateSelf( )
        {
            world.Step((float)EngineInfo.GameTimeCache.ElapsedGameTime.TotalSeconds,3,3);
            for( int count = 0; count < _contentLayers.Count; count++ )
                if( _contentLayers[count].UpdateEnable )
                    _contentLayers[count].DoUpdate( );
            _uiLayer.DoUpdate( );
            base.UpdateSelf( );
        }

        protected override void RenderSelf( SpriteBatch spriteBatch )
        {
            for( int count = _contentLayers.Count - 1; count >= 0; count-- )
                if( _contentLayers[count].RenderEnable )
                    _contentLayers[count].DoRender( );
            _uiLayer.DoRender( );
            base.RenderSelf(spriteBatch);
        }

        public void Register( SceneContentLayer layer )
        {
            _contentLayers.Add(layer);
        }

        public void Register( SceneUserInterfaceLayer layer )
        {
            _uiLayer = layer;
        }

    }
}