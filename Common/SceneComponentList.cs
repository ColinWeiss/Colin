using Colin.Core.Collections;
using Colin.Core.Extensions;

namespace Colin.Core.Common
{
    /// <summary>
    /// 场景模块集合.
    /// </summary>
    public class SceneComponentList : ITraceable
    {
        private readonly Scene Scene;

        public SceneComponentList( Scene scene ) => Scene = scene;

        /// <summary>
        /// 在向 <see cref="SceneComponentList"/> 添加组件时引发。
        /// </summary>
        public event EventHandler<SceneComponentListEventArgs> OnAdd;

        /// <summary>
        /// 在从 <see cref="SceneComponentList"/> 移除组件时引发。
        /// </summary>
        public event EventHandler<SceneComponentListEventArgs> OnRemove;

        public LiteList<ISceneComponent> Components = new LiteList<ISceneComponent>( );

        public LiteList<IRenderableSceneComponent> RenderableComponents = new LiteList<IRenderableSceneComponent>( );

        public string Name => nameof( SceneComponentList );

        public string DisplayName => Name;

        public void DoUpdate( GameTime gameTime )
        {
            ISceneComponent _com;
            for( int count = 0; count < Components.length; count++ )
            {
                _com = Components.buffer[count];
                if( _com.Enable )
                    _com.DoUpdate( gameTime );
            }
        }

        public void Add( ISceneComponent sceneMode )
        {
            sceneMode.Scene = Scene;
            sceneMode.Enable = true;
            sceneMode.DoInitialize( );
            if( sceneMode is IRenderableSceneComponent dwMode )
            {
                dwMode.Visible = true;
                RenderableComponents.Add( dwMode );
                dwMode.InitRenderTarget( );
                Scene.Game.Window.ClientSizeChanged += dwMode.OnClientSizeChanged;
            }
            OnComponentAdded( new SceneComponentListEventArgs( sceneMode ) );
            Components.Add( sceneMode );
        }

        public void Remove( ISceneComponent sceneMode )
        {
            if( sceneMode is IRenderableSceneComponent dwMode )
            {
                RenderableComponents.Remove( dwMode );
                Scene.Game.Window.ClientSizeChanged -= dwMode.OnClientSizeChanged;
            }
            OnComponentRemoved( new SceneComponentListEventArgs( sceneMode ) );
            Components.Remove( sceneMode );
        }

        public void Clear( )
        {
            Components.Clear( );
            RenderableComponents.Clear( );
        }

        private void OnComponentAdded( SceneComponentListEventArgs eventArgs ) => this.EventRaise( OnAdd, eventArgs );

        private void OnComponentRemoved( SceneComponentListEventArgs eventArgs ) => this.EventRaise( OnRemove, eventArgs );

    }
}