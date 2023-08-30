namespace Colin.Core.Extensions
{
    /// <summary>
    /// <seealso cref="Vector2"/> 的扩展类.
    /// </summary>
    public static class VectorExt
    {
        public static Vector2 GetAbs( this Vector2 vector2 )
        {
            return new Vector2( Math.Abs( vector2.X ) , Math.Abs( vector2.Y ) );
        }

        /// <summary>
        /// 对向量进行线性插值.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <param name="i"></param>
        /// <param name="maxi"></param>
        /// <returns></returns>
        public static Vector2 Closer( this ref Vector2 current, Vector2 target, float i, float maxi )
        {
            float x = current.X;
            float y = current.Y;
            float tx = target.X;
            float ty = target.Y;
            x *= maxi - i;
            x /= maxi;
            y *= maxi - i;
            y /= maxi;
            tx *= i;
            tx /= maxi;
            ty *= i;
            ty /= maxi;
            current = new Vector2( x + tx, y + ty );
            return new Vector2( x + tx, y + ty );
        }
    }
}