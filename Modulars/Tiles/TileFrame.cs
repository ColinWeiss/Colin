using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 物块帧格.
    /// </summary>
    [Serializable]
    public struct TileFrame
    {
        public int X;

        public int Y;

        public int Width;

        public int Height;

        /// <summary>
        /// 指示该帧格是否为有效选帧.
        /// <br>[!] 将 <see cref="X"/> 或 <see cref="Y"/> 设为负数即可令帧格失效.</br>
        /// <br>[!] 同理, 若 <see cref="X"/> 和 <see cref="Y"/> 均大于等于 0, 帧格生效.</br>
        /// </summary>
        public bool Effective => X >= 0 && Y >= 0;

        public Rectangle Frame =>
            new Rectangle(
                X * TileOption.TileSize.X,
                Y * TileOption.TileSize.Y,
                Width * TileOption.TileSize.X,
                Height * TileOption.TileSize.X);

        public TileFrame(int x, int y, int width = 1, int height = 1)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}