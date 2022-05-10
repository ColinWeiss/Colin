using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Colin
{
    public class HardwareInfo
    {
        /// <summary>
        ///用于初始化和控制图形设备的显示.
        /// </summary>
        public static GraphicsDeviceManager? Graphics { get; internal set; }

        /// <summary>
        /// 纹理批管道.
        /// </summary>
        public static SpriteBatch? SpriteBatch { get; internal set; }

        /// <summary>
        /// 游戏刻缓存.
        /// </summary>
        public static GameTime? GameTimeCache { get; internal set; }

        /// <summary>
        /// 游戏视图分辨率宽度.
        /// </summary>
        public static int GameViewWidth
        {
            get
            {
                return Graphics.GraphicsDevice.Viewport.Width;
            }
        }

        /// <summary>
        /// 游戏视图分辨率高度.
        /// </summary>
        public static int GameViewHeight
        {
            get
            {
                return Graphics.GraphicsDevice.Viewport.Height;
            }
        }

        /// <summary>
        /// 游戏视图分辨率中心坐标.
        /// </summary>
        public static Vector2 GameViewCenter
        {
            get
            {
                return new Vector2( GameViewWidth / 2, GameViewHeight / 2 );
            }
        }

        /// <summary>
        /// 屏幕大小
        /// </summary>
        public static Rectangle GameViewSize
        {
            get
            {
                return new Rectangle( 0, 0, GameViewWidth, GameViewHeight );
            }
        }

        /// <summary>
        /// 从设备获取信息.
        /// </summary>
        /// <param name="gameTime">游戏刻.</param>
        internal static void GetInformationFromDevice( GameTime gameTime )
        {
            GameTimeCache = gameTime;
        }
    }
}