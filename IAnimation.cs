using Microsoft.Xna.Framework;

namespace Colin
{
    /// <summary>
    /// 表示一个具有帧动画的对象.
    /// </summary>
    public interface IAnimation
    {
        /// <summary>
        /// 指示该对象的当前帧的列数.
        /// </summary>
        public int CurrentFrameX { get; }
        /// <summary>
        /// 指示该对象的当前帧的行数.
        /// </summary>
        public int CurrentFrameY { get; }
        /// <summary>
        /// 指示该对象一帧的大小.
        /// </summary>
        public Rectangle Frame { get; }
        /// <summary>
        /// 获取读帧矩形.
        /// </summary>
        /// <returns>读取的帧的矩形.</returns>
        public Rectangle GetFrame( );
    }
}