using Microsoft.Xna.Framework;

namespace Colin.Common.DataStructures
{
    /// <summary>
    /// 表示一根线.
    /// </summary>
    public struct Line
    {
        /// <summary>
        /// 指示线的起点.
        /// </summary>
        public Vector2 Start;

        /// <summary>
        /// 指示线的终点.
        /// </summary>
        public Vector2 End;

        /// <summary>
        /// 定义一根线.
        /// </summary>
        /// <param name="start">起点.</param>
        /// <param name="end">终点.</param>
        public Line( Vector2 start, Vector2 end )
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// 将该线转化为平面向量.
        /// </summary>
        public Vector2 ToVector2( )
        {
            return End - Start;
        }
    }
}