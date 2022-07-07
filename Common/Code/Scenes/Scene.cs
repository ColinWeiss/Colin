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

        private World _physicWorld;
        /// <summary>
        /// 用于计算物理的 <see cref="World"/> 对象.
        /// </summary>
        public World PhysicWorld => _physicWorld;

        public Scene( )
        {
        }

        /// <summary>
        /// 设置场景的用户交互界面内容层.
        /// </summary>
        /// <returns></returns>
        protected abstract void SetUserInterface( ref SceneUserInterfaceLayer uiLayer );

        protected override void Initialization( )
        {
            ModifyLayers( ref _contentLayers );
            _physicWorld = new World( Vector2.UnitY );
            SetPhysicWorld( ref _physicWorld );
            SetUserInterface( ref _uiLayer );
            for( int count = 0; count < _contentLayers.Count; count++ )
                _contentLayers[count].DoInitialize( );
            _uiLayer.DoInitialize( );
            base.Initialization( );
        }
        protected virtual void ModifyLayers( ref List<SceneContentLayer> layers )
        {
        }

        protected override void UpdateSelf( )
        {
            _physicWorld.Step((float)EngineInfo.GameTimeCache.ElapsedGameTime.TotalSeconds,3,3);
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

        public virtual void SetPhysicWorld( ref World world )
        {

        }

        public void Register( SceneContentLayer layer )
        {
            _contentLayers.Add(layer);
        }

        public void Register( SceneUserInterfaceLayer layer )
        {
            _uiLayer = layer;
        }

        public void Remove( SceneContentLayer layer )
        {
            _contentLayers.Remove( layer );
        }

    }
}