using Colin.Core.Inputs;
using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Extensions
{
    public static class RectangleExt
    {
        /// <summary>
        /// 获取矩形左边缘的中心位置.
        /// </summary>
        public static Vector2 GetLeftCenter( this Rectangle rect )
        {
            return new Vector2( rect.Left, rect.Y + rect.Height / 2.0f );
        }

        /// <summary>
        /// 获取矩形右边缘的中心位置.
        /// </summary>
        public static Vector2 GetRightCenter( this Rectangle rect )
        {
            return new Vector2( rect.Right, rect.Y + rect.Height / 2.0f );
        }

        /// <summary>
        /// 获取矩形上边缘的中心位置.
        /// </summary>
        public static Vector2 GetTopCenter( this Rectangle rect )
        {
            return new Vector2( rect.X + rect.Width / 2.0f, rect.Top );
        }

        /// <summary>
        /// 获取矩形下边缘的中心位置.
        /// </summary>
        public static Vector2 GetBottomCenter( this Rectangle rect )
        {
            return new Vector2( rect.X + rect.Width / 2.0f, rect.Bottom );
        }

        /// <summary>
        /// 计算两个矩形之间的有符号相交深度.
        /// </summary>
        /// <returns>
        /// 两个相交矩形之间的重叠量。
        /// 这些深度值可以是负的，
        /// 这取决于矩形相交的宽度。
        /// 这允许调用方确定推送对象的正确方向，
        /// 以解决冲突。如果矩形不相交，
        /// 则返回Vector2.Zero。
        /// </returns>
        public static Vector2 GetIntersectionDepth( this RectangleF rectA, RectangleF rectB )
        {
            // Calculate half sizes.
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            // Calculate centers.
            Vector2 centerA = new Vector2( rectA.Left + halfWidthA, rectA.Top + halfHeightA );
            Vector2 centerB = new Vector2( rectB.Left + halfWidthB, rectB.Top + halfHeightB );

            // Calculate current and minimum-non-intersecting distances between centers.
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            // If we are not intersecting at all, return (0, 0).
            if( Math.Abs( distanceX ) >= minDistanceX || Math.Abs( distanceY ) >= minDistanceY )
                return Vector2.Zero;

            // Calculate and return intersection depths.
            float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2( depthX, depthY );
        }

        /// <summary>
        /// 判断交互点是否与指定的 <seealso cref="Rectangle"/> 重合.
        /// </summary>
        /// <param name="rectangle">指定的 <seealso cref="Rectangle"/>.</param>
        /// <returns>如若是, 返回 <see href="true"/>, 否则返回 <see href="false"/>.</returns>
        public static bool IntersectInteractive( this RectangleF rectangle )
        {
            return rectangle.Intersects( new RectangleF( Input.InteractionPoint.ToPoint(), Point.Zero ) );
        }

    }
}