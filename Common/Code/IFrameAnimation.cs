using Microsoft.Xna.Framework;

namespace Colin.Common.Code
{
    /// <summary>
    /// 表示一个帧动画对象.
    /// </summary>
    public interface IFrameAnimation
    {
        public Rectangle GetFrame( );
    }
}