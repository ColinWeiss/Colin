using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Colin.Core
{
    /// <summary>
    /// 标识一个圆.
    /// </summary>
    public struct Circle
    {
        /// <summary>
        /// 圆的中心点.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// 圆的半径.
        /// </summary>
        public float Radius;

        /// <summary>
        /// 构建一个新的圆.
        /// </summary>
        public Circle(Vector2 position, float radius)
        {
            Center = position;
            Radius = radius;
        }

        /// <summary>
        /// 确定该圆是否与指定圆有重叠.
        /// </summary>
        public bool Intersects(Circle circle)
        {
            return Vector2.Distance( Center, circle.Center ) < Radius + circle.Radius;
        }

        /// <summary>
        /// 确定该圆是否与指定矩形相交.
        /// </summary>
        public bool Intersects(Rectangle rectangle)
        {
            Vector2 v = new Vector2(
                MathHelper.Clamp( Center.X, rectangle.Left, rectangle.Right ),
                MathHelper.Clamp( Center.Y, rectangle.Top, rectangle.Bottom ) );
            Vector2 direction = Center - v;
            float distanceSquared = direction.LengthSquared();
            return distanceSquared > 0 && distanceSquared < Radius * Radius;
        }
    }
}