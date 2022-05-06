using Colin.Core.Scenes;
using Colin.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin
{
    /// <summary>
    /// 对 <seealso cref="Game"/> 类进行了简单的封装.
    /// </summary>
    public class Engine : Game
    {
        internal static Engine? Instance { get; private set; }

        /// <summary>
        /// 指示该 <seealso cref="Engine"/> 的目标帧数.
        /// </summary>
        public virtual int TargetFrame { get; } = 144;

        /// <summary>
        /// 用于该 <seealso cref="Engine"/> 的RenderTarget2D.
        /// </summary>
        public RenderTarget2D? EngineRenderTarget { get; internal set; }

        /// <summary>
        /// 用于程序本体的RenderTarget2D进行切换使用的备用RenderTarget2D.
        /// </summary>
        public RenderTarget2D? EngineRenderTargetSwap { get; internal set; }

        /// <summary>
        /// 用于进行屏幕管理器的RenderTarget切换.
        /// </summary>
        internal static bool _engineRenderTargetSwitch = true;

        public Engine( )
        {
            Instance = this;
            IsFixedTimeStep = true;
            HardwareInfo.Graphics = new GraphicsDeviceManager( this )
            {
                PreferHalfPixelOffset = true,
                HardwareModeSwitch = false,
                PreferMultiSampling = false
            };
            Window.AllowUserResizing = true;
            IsFixedTimeStep = true;
            HardwareInfo.Graphics.SynchronizeWithVerticalRetrace = false;
            TargetElapsedTime = new TimeSpan( 0, 0, 0, 0, (int)Math.Round( 1000f / TargetFrame ) );
            Content.RootDirectory = "Contents";
        }

        protected override sealed void Initialize( )
        {
            HardwareInfo.SpriteBatch = new SpriteBatch( GraphicsDevice );
            if ( HardwareInfo.Graphics != null )
            {
                HardwareInfo.Graphics.PreferredBackBufferWidth = 1280;
                HardwareInfo.Graphics.PreferredBackBufferHeight = 720;
                HardwareInfo.Graphics.ApplyChanges( );
            }
            EngineRenderTarget = new RenderTarget2D(
                    HardwareInfo.Graphics.GraphicsDevice,
                    HardwareInfo.Graphics.GraphicsDevice.Viewport.Width,
                    HardwareInfo.Graphics.GraphicsDevice.Viewport.Height );
            EngineRenderTargetSwap = new RenderTarget2D(
                    HardwareInfo.Graphics.GraphicsDevice,
                    HardwareInfo.Graphics.GraphicsDevice.Viewport.Width,
                    HardwareInfo.Graphics.GraphicsDevice.Viewport.Height );
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            void Window_ClientSizeChanged( object? sender, EventArgs e )
            {
                EngineRenderTarget = new RenderTarget2D(
                    HardwareInfo.Graphics.GraphicsDevice,
                    HardwareInfo.Graphics.GraphicsDevice.Viewport.Width,
                    HardwareInfo.Graphics.GraphicsDevice.Viewport.Height );
                EngineRenderTargetSwap = new RenderTarget2D(
                    HardwareInfo.Graphics.GraphicsDevice,
                    HardwareInfo.Graphics.GraphicsDevice.Viewport.Width,
                    HardwareInfo.Graphics.GraphicsDevice.Viewport.Height );
            }
            Initialization( );
            Components.Add( InitializeScene( ) );
            for ( int count = Components.Count - 1; count > 0 ; count-- )
                ( Components[ count ] as GameComponent ).UpdateOrder = count;
            base.Initialize( );
        }

        protected virtual void Initialization( )
        {

        }

        /// <summary>
        /// 重写该函数以于初始化场景时执行自定义操作.
        /// </summary>
        /// <returns>开始场景.</returns>
        protected virtual Scene InitializeScene( )
        {
            return new Scene( );
        }

        protected override sealed void LoadContent( ) => base.LoadContent( );

        protected override sealed void BeginRun( ) => base.BeginRun( );

        protected override sealed void Update( GameTime gameTime )
        {
            HardwareInfo.GetInformationFromDevice( gameTime );
            Input.GetInformationFromDevice( );
            base.Update( gameTime );
            Input.ResetInfomation( );
        }

        protected override sealed void EndRun( ) => base.EndRun( );



        protected override sealed void Draw( GameTime gameTime )
        {
            GraphicsDevice.Clear( Color.Black );
            GraphicsDevice.SetRenderTarget( EngineRenderTarget );
            base.Draw( gameTime );
            ScreenRender.RenderFrame( );
            Input.ResetInfomation( );
        }

        protected override sealed bool BeginDraw( ) => true;

        protected override sealed void EndDraw( ) => base.EndDraw( );

        protected override void UnloadContent( ) => base.UnloadContent( );
    }
}