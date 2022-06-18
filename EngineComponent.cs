using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Colin
{
    /// <summary>
    /// 表示一个引擎插件.
    /// </summary>
    public class EngineComponent : DrawableGameComponent
    {
        /// <summary>
        /// 表示该引擎插件所包含的可用作渲染目标的2D纹理.
        /// </summary>
        public RenderTarget2D? RenderTarget;

        protected virtual void Initialization( )
        {

        }

        public override sealed void Initialize( )
        {
            base.Initialize( );
            _updateStarted = false;
            _RenderStarted = false;
            RenderTarget = new RenderTarget2D(
                EngineInfo.Graphics.GraphicsDevice,
                EngineInfo.Graphics.GraphicsDevice.Viewport.Width,
                EngineInfo.Graphics.GraphicsDevice.Viewport.Height);
            Engine.Instance.Window.ClientSizeChanged += Window_ClientSizeChanged;
            void Window_ClientSizeChanged( object? sender,EventArgs e )
            {
                RenderTarget = new RenderTarget2D(
                EngineInfo.Graphics.GraphicsDevice,
                EngineInfo.Graphics.GraphicsDevice.Viewport.Width,
                EngineInfo.Graphics.GraphicsDevice.Viewport.Height);
            }
            Initialization( );
        }

        protected override sealed void LoadContent( )
        {
            base.LoadContent( );
        }

        bool _updateStarted;
        public override sealed void Update( GameTime gameTime )
        {
            base.Update(gameTime);
            if( !_updateStarted )
            {
                UpdateStart( );
                _updateStarted = true;
            }
            this?.UpdateSelf( );
            this?.PostUpdate( );
        }
        protected virtual void UpdateStart( )
        {

        }
        protected virtual void UpdateSelf( )
        {
        }
        protected virtual void PostUpdate( )
        {
        }

        bool _RenderStarted;
        public override sealed void Draw( GameTime gameTime )
        {
            base.Draw(gameTime);
            if( !_RenderStarted )
            {
                RenderStart(EngineInfo.SpriteBatch);
                _RenderStarted = true;
            }
            this?.RenderSelf(EngineInfo.SpriteBatch);
            this?.PostRender(EngineInfo.SpriteBatch);
        }
        protected virtual void RenderStart( SpriteBatch spriteBatch )
        {

        }
        protected virtual void RenderSelf( SpriteBatch spriteBatch )
        {
        }
        protected virtual void PostRender( SpriteBatch spriteBatch )
        {
        }

        public EngineComponent( ) : base(Engine.Instance) { }
    }
}