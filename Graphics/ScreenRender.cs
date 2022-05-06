using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Colin.Graphics
{
    /// <summary>
    /// [单例] 屏幕渲染器.
    /// <para>一般用于为屏幕添加后处理效果.</para>
    /// </summary>
    public sealed class ScreenRender : GameComponent
    {
        private static ScreenRender? _instance;
        public static ScreenRender? Instance
        {
            get
            {
                if ( _instance == null )
                    _instance = new ScreenRender( );
                return _instance;
            }
        }

        /// <summary>
        /// 屏幕着色器的实例集合.
        /// </summary>
        public List<ScreenShader>? ScreenShaders { get; private set; } = new List<ScreenShader>( );

        /// <summary>
        /// 向管理器内增加一个屏幕着色器.
        /// <para>[!] 已添加的不会重复添加.</para>
        /// </summary>
        /// <param name="data">要添加的屏幕着色器.</param>
        public static void AddScreenShaderData( ScreenShader data )
        {
            if ( !data.ForInternalOptimizationBoolen )
            {
                if ( !Instance.ScreenShaders.Contains( data ) )
                {
                    Instance.ScreenShaders.Add( data );
                    data.ForInternalOptimizationBoolen = true;
                }
            }
        }

        /// <summary>
        /// 根据指定类类型返回列表内指定屏幕着色器的实例; 
        /// 若寻找失败, 则返回null.
        /// </summary>
        /// <typeparam name="T">屏幕着色器类型.</typeparam>
        /// <returns></returns>
        public static T GetScreenShaderData<T>( ) where T : ScreenShader, new()
        {
            int num = Instance.ScreenShaders.FindIndex( a => typeof( T ).FullName == a.GetType( ).FullName );
            if ( num == -1 )
                return null;
            return Instance.ScreenShaders[ num ] as T;
        }

        /// <summary>
        /// 根据指定标识屏幕着色器的字符串返回列表内屏幕着色器的实例;
        /// 若寻找失败, 则返回null.
        /// </summary>
        /// <param name="screenShaderName">标识该屏幕着色器的字符串.</param>
        /// <returns></returns>
        public static ScreenShader GetScreenShaderData( string screenShaderName )
        {
            int num = Instance.ScreenShaders.FindIndex( a => a.ScreenShaderName == screenShaderName );
            if ( num == -1 )
                return null;
            return Instance.ScreenShaders[ num ];
        }

        /// <summary>
        /// 向管理器内删除一个屏幕着色器.
        /// <para>[!] 不存在的不会进行删除操作.</para>
        /// </summary>
        /// <param name="data">要删除的屏幕着色器.</param>
        public static void RemoveShaderData( ScreenShader data )
        {
            if ( data.ForInternalOptimizationBoolen )
            {
                if ( !Instance.ScreenShaders.Contains( data ) )
                {
                    Instance.ScreenShaders.Remove( data );
                    data.ForInternalOptimizationBoolen = false;
                }
            }
        }

         public override void Update( GameTime gameTime )
        {
            foreach ( ScreenShader item in ScreenShaders )
            {
                if ( item.Enable )
                    item.UpdateShader( gameTime );
            }
            base.Update( gameTime );
        }

        /// <summary>
        /// 绘制屏幕.
        /// </summary>
        internal static void RenderFrame( )
        {
            Engine.Instance.GraphicsDevice.SetRenderTarget( Engine.Instance.EngineRenderTargetSwap );
            for ( int screenShaderCount = 0; screenShaderCount < Instance.ScreenShaders.Count; screenShaderCount++ )
            {
                HardwareInfo.SpriteBatch.Begin( SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                if ( Engine._engineRenderTargetSwitch )
                {
                    Instance.ScreenShaders[ screenShaderCount ].ApplyPass( "ScreenPass" );
                    HardwareInfo.SpriteBatch.Draw( Engine.Instance.EngineRenderTarget, Vector2.Zero, Color.White );
                    Engine.Instance.GraphicsDevice.SetRenderTarget( Engine.Instance.EngineRenderTarget );
                }
                else
                {
                    Instance.ScreenShaders[ screenShaderCount ].ApplyPass( "ScreenPass" );
                    HardwareInfo.SpriteBatch.Draw( Engine.Instance.EngineRenderTargetSwap, Vector2.Zero, Color.White );
                    Engine.Instance.GraphicsDevice.SetRenderTarget( Engine.Instance.EngineRenderTargetSwap );
                }
                if ( Instance.ScreenShaders.Count > 1 )
                    Engine._engineRenderTargetSwitch = !Engine._engineRenderTargetSwitch;
                HardwareInfo.SpriteBatch.End( );
            }
            Engine.Instance.GraphicsDevice.SetRenderTarget( null );
            if( Engine._engineRenderTargetSwitch )
            {
                HardwareInfo.SpriteBatch.Begin( SpriteSortMode.Immediate, BlendState.AlphaBlend );
                HardwareInfo.SpriteBatch.Draw( Engine.Instance.EngineRenderTarget, Vector2.Zero, Color.White );
                HardwareInfo.SpriteBatch.End( );
            }
            else
            {
                HardwareInfo.SpriteBatch.Begin( SpriteSortMode.Immediate, BlendState.AlphaBlend );
                HardwareInfo.SpriteBatch.Draw( Engine.Instance.EngineRenderTargetSwap, Vector2.Zero, Color.White );
                HardwareInfo.SpriteBatch.End( );
            }
        }

        public ScreenRender( ) : base(Engine.Instance )
        {
            Engine._engineRenderTargetSwitch = true;
        }
    }
}