namespace Colin.Core.Extensions
{
    public static class RectangleExt
    {
        /// <summary>
        /// 获取矩形左边缘的中心位置.
        /// </summary>
        public static Vector2 GetLeftCenter(this Rectangle rect)
        {
            return new Vector2( rect.Left, rect.Y + rect.Height / 2.0f );
        }

        /// <summary>
        /// 获取矩形右边缘的中心位置.
        /// </summary>
        public static Vector2 GetRightCenter(this Rectangle rect)
        {
            return new Vector2( rect.Right, rect.Y + rect.Height / 2.0f );
        }

        /// <summary>
        /// 获取矩形上边缘的中心位置.
        /// </summary>
        public static Vector2 GetTopCenter(this Rectangle rect)
        {
            return new Vector2( rect.X + rect.Width / 2.0f, rect.Top );
        }

        /// <summary>
        /// 获取矩形下边缘的中心位置.
        /// </summary>
        public static Vector2 GetBottomCenter(this Rectangle rect)
        {
            return new Vector2( rect.X + rect.Width / 2.0f, rect.Bottom );
        }
    }
}