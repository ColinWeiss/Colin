using Microsoft.Xna.Framework;

namespace Colin.Common.Code.Tiled
{
    /// <summary>
    /// 物块帧格选取.
    /// <para>[!] 物块所在瓦片地图的网格大小为单位 1.</para>
    /// </summary>
    public struct TileFrameGrid
    {
        /// <summary>
        /// 该帧格选取所绑定的物块.
        /// </summary>
        public Tile Tile { get; }

        /// <summary>
        /// 帧格选取横坐标.
        /// </summary>
        public int FrameX;

        /// <summary>
        /// 帧格选取纵坐标.
        /// </summary>
        public int FrameY;

        /// <summary>
        /// 帧格宽度.
        /// </summary>
        public int FrameWidth;

        /// <summary>
        /// 帧格高度.
        /// </summary>
        public int FrameHeight;

        private Rectangle frameCache;
        public Rectangle Frame
        {
            get
            {
                if ( frameCache == Rectangle.Empty )
                    frameCache = new Rectangle(
                        FrameX * Tile.TileMap.GridSize,
                        FrameY * Tile.TileMap.GridSize,
                       FrameWidth * Tile.TileMap.GridSize,
                      FrameHeight * Tile.TileMap.GridSize );
                return frameCache;
            }
        }

        /// <summary>
        /// 设置物块帧格.
        /// </summary>
        /// <param name="start">帧格选取起点.</param>
        /// <param name="size">帧格大小.</param>
        public void SetFrame( Point start , Point size )
        {
            FrameX = start.X;
            FrameY = start.Y;
            FrameWidth = size.X;
            FrameHeight = size.Y;
        }

        /// <summary>
        /// 设置物块帧格.
        /// </summary>
        /// <param name="frameX">帧格选取横坐标.</param>
        /// <param name="frameY">帧格选取纵坐标.</param>
        /// <param name="frameWidth">帧格宽度.</param>
        /// <param name="frameHeight">帧格高度.</param>
        public void SetFrame( int frameX , int frameY , int frameWidth , int frameHeight )
        {
            FrameX = frameX;
            FrameY = frameY;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
        }

        /// <summary>
        /// 设置物块帧格.
        /// </summary>
        /// <param name="frameGrid">帧格.</param>
        public void SetFrame( Rectangle frameGrid )
        {
            FrameX = frameGrid.X;
            FrameY = frameGrid.Y;
            FrameWidth = frameGrid.Width;
            FrameHeight = frameGrid.Height;
        }

        public TileFrameGrid( Tile tile )
        {
            Tile = tile;
            FrameX = 0;
            FrameY = 0;
            FrameWidth = 0;
            FrameHeight = 0;
            frameCache = Rectangle.Empty;
        }

    }
}