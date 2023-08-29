using Colin.Core.Collections;
using Colin.Core.Extensions;
using Colin.Core.Assets;

namespace Colin.Core.Common
{
    /// <summary>
    /// 场景.
    /// </summary>
    public class Scene : DrawableGameComponent
    {
        public SceneCamera SceneCamera;

        private SceneComponentList _components;
        /// <summary>
        /// 获取场景组件列表.
        /// </summary>
        public SceneComponentList ComponentList => _components;

        /// <summary>
        /// 指示场景在切换时是否执行初始化的值.
        /// </summary>
        public bool InitializeOnSwitch = true;

        /// <summary>
        /// 场景本身的 RenderTarget.
        /// </summary>
        public RenderTarget2D SceneRenderTarget;

        public SceneShaderManager SceneShaders = new SceneShaderManager( );

        public override sealed void Initialize( )
        {
            Started = false;
            if( InitializeOnSwitch )
            {
                InitRenderTarget( this, new EventArgs( ) );
                Game.Window.OrientationChanged += InitRenderTarget;
                Game.Window.ClientSizeChanged += InitRenderTarget;
                _components = new SceneComponentList( this );
                _components.Add( SceneCamera = new SceneCamera( ) );
                SceneInit( );
            }
            base.Initialize( );
        }

        internal void InitRenderTarget( object s, EventArgs e )
        {
            SceneRenderTarget?.Dispose( );
            SceneRenderTarget = RenderTargetExt.CreateDefault( );
        }

        /// <summary>
        /// 执行场景初始化.
        /// </summary>
        public virtual void SceneInit( ) { }

        private bool Started = false;
        public override sealed void Update( GameTime gameTime )
        {
            if( !Started )
            {
                Start( );
                Started = true;
            }
            ComponentList.DoUpdate( gameTime );
            SceneUpdate( );
            base.Update( gameTime );
        }
        public virtual void Start( ) { }

        public virtual void SceneUpdate( ) { }

        public override sealed void Draw( GameTime gameTime )
        {
            IRenderableSceneComponent renderMode;
            RenderTarget2D frameRenderLayer;
            EngineInfo.Graphics.GraphicsDevice.SetRenderTarget( SceneRenderTarget );
            EngineInfo.Graphics.GraphicsDevice.Clear( Color.Black );
            for( int count = 0; count < ComponentList.RenderableComponents.length; count++ )
            {
                renderMode = ComponentList.RenderableComponents[count];
                if( renderMode.Visible )
                {
                    frameRenderLayer = renderMode.SceneRt;
                    EngineInfo.Graphics.GraphicsDevice.SetRenderTarget( frameRenderLayer );
                    EngineInfo.Graphics.GraphicsDevice.Clear( Color.Transparent );
                    renderMode.DoRender( EngineInfo.SpriteBatch );
                }
            }
            EngineInfo.Graphics.GraphicsDevice.SetRenderTarget( SceneRenderTarget );
            for( int count = 0; count < ComponentList.RenderableComponents.length; count++ )
            {
                renderMode = ComponentList.RenderableComponents[count];
                if( renderMode.Visible )
                {
                    frameRenderLayer = renderMode.SceneRt;
                    if( SceneShaders.Effects.TryGetValue( renderMode, out Effect e ) )
                        EngineInfo.SpriteBatch.Begin( SpriteSortMode.Deferred, effect: e );
                    else
                        EngineInfo.SpriteBatch.Begin( SpriteSortMode.Deferred );
                    EngineInfo.SpriteBatch.Draw( frameRenderLayer, new Rectangle( 0, 0, EngineInfo.ViewWidth, EngineInfo.ViewHeight ), Color.White );
                    EngineInfo.SpriteBatch.End( );

                }
            }
            SceneRender( );
            EngineInfo.Graphics.GraphicsDevice.SetRenderTarget( null );
            EngineInfo.SpriteBatch.Begin( );
            EngineInfo.SpriteBatch.Draw( SceneRenderTarget, new Rectangle( 0, 0, EngineInfo.ViewWidth, EngineInfo.ViewHeight ), Color.White );
            EngineInfo.SpriteBatch.End( );
            base.Draw( gameTime );
        }

        public virtual void SceneRender( ) { }

        /// <summary>
        /// 我不在乎你加不加载, 但我希望玩家的电脑犯病的时候我们能把重要数据保存下来.
        /// <br>这个方法将在 <see cref="Game.Exit"/> 执行时跟着执行执行.</br>
        /// <br>你也可以把它写成能手动调用用来保存数据的样子.</br>
        /// </summary>
        public virtual void SaveDatas( ) { }

        /// <summary>
        ///  卸载场景时执行.
        /// </summary>
        public virtual void UnLoad( ) { }

        public Scene( ) : base( EngineInfo.Engine ) { }
    }
}