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
            _drawStarted = false;
            RenderTarget = new RenderTarget2D(
                HardwareInfo.Graphics.GraphicsDevice,
                HardwareInfo.Graphics.GraphicsDevice.Viewport.Width,
                HardwareInfo.Graphics.GraphicsDevice.Viewport.Height );
           Engine.Instance.Window.ClientSizeChanged += Window_ClientSizeChanged;
            void Window_ClientSizeChanged( object? sender, EventArgs e )
            {
                RenderTarget = new RenderTarget2D(
                HardwareInfo.Graphics.GraphicsDevice,
                HardwareInfo.Graphics.GraphicsDevice.Viewport.Width,
                HardwareInfo.Graphics.GraphicsDevice.Viewport.Height );
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
            base.Update( gameTime );
            if ( !_updateStarted )
            {
                UpdateStart( );
                _updateStarted = true;
            }
            if ( this != null )
                PreUpdate( );
            if ( this != null )
                Update( );
            if ( this != null )
                PostUpdate( );
        }
         protected virtual void UpdateStart( )
        {

        }
         protected virtual void PreUpdate( )
        {
        }
         protected virtual void Update( )
        {
        }
         protected virtual void PostUpdate( )
        {
        }

        bool _drawStarted;
        public override sealed void Draw( GameTime gameTime )
        {
            base.Draw( gameTime );
            if ( !_drawStarted )
            {
                DrawStart( HardwareInfo.SpriteBatch );
                _drawStarted = true;
            }
            if ( this != null )
                PreDraw( HardwareInfo.SpriteBatch );
            if ( this != null )
                Draw( HardwareInfo.SpriteBatch );
            if ( this != null )
                PostDraw( HardwareInfo.SpriteBatch );
        }
        protected virtual void DrawStart( SpriteBatch spriteBatch )
        {

        }
         protected virtual void PreDraw( SpriteBatch spriteBatch )
        {
        }
         protected virtual void Draw( SpriteBatch spriteBatch )
        {
        }
         protected virtual void PostDraw( SpriteBatch spriteBatch )
        {
        }

         public EngineComponent( ) : base(Engine.Instance ) { }
    }
}