using Microsoft.Xna.Framework;

namespace Colin.Extensions
{
    /// <summary>
    /// <seealso cref="Vector2"/> 的扩展类.
    /// <para>包含各种为 <seealso cref="Vector2"/> 编写的扩展方法, 旨在提高编码效率.</para>
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// 获取指定 <seealso cref="Vector2"/> 的单位向量.
        /// </summary>
        /// <param name="vector2">指定的 <seealso cref="Vector2"/>.</param>
        /// <returns>指定 <seealso cref="Vector2"/> 的单位向量.</returns>
        public static Vector2 GetNormalize( this Vector2 vector2 )
        {
            vector2.Normalize( );
            return vector2;
        }

        /// <summary>
        /// 获取向量旋转至一定的角度的值; 该角度计算单位使用弧度制.
        /// </summary>
        /// <param name="vec">要旋转的向量.</param>
        /// <param name="radian">要旋转的角度.</param>
        /// <returns></returns>
        public static Vector2 GetRotateTo( this Vector2 vec,float radian )
        {
            float l = vec.Length( );
            return new Vector2((float)Math.Cos(radian) * l,(float)Math.Sin(radian) * l);
        }

        /// <summary>
        /// 以指定坐标为旋转中心, 获取向量旋转至一定的角度的值; 该角度计算单位使用弧度制.
        /// </summary>
        /// <param name="vec">要旋转的向量.</param>
        /// <param name="radian">要旋转的角度.</param>
        /// <param name="center">作为旋转中心的坐标.</param>
        /// <returns></returns>
        public static Vector2 GetRotateTo( this Vector2 vec,float radian,Vector2 center )
        {
            vec -= center;
            float l = vec.Length( );
            return new Vector2((float)Math.Cos(radian) * l,(float)Math.Sin(radian) * l) + center;
        }

        /// <summary>
        /// 获取向量旋转一定的角度的值; 该角度计算单位使用弧度制.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="radian"></param>
        /// <returns></returns>
        public static Vector2 GetRotate( this Vector2 vec,float radian )
        {
            float c = (float)Math.Cos(radian);
            float s = (float)Math.Sin(radian);
            return new Vector2(c * vec.X - s * vec.Y,s * vec.X + c * vec.Y);
        }

        /// <summary>
        /// 以指定坐标为旋转中心, 获取向量旋转一定的角度的值; 该角度计算使用弧度制.
        /// </summary>
        /// <param name="vec">要旋转的向量.</param>
        /// <param name="radian">要旋转的角度.</param>
        /// <param name="center">作为旋转中心的坐标.</param>
        /// <returns></returns>
        public static Vector2 GetRotate( this Vector2 vec,float radian,Vector2 center = default )
        {
            vec -= center;
            float c = (float)Math.Cos(radian);
            float s = (float)Math.Sin(radian);
            return new Vector2(c * vec.X - s * vec.Y,s * vec.X + c * vec.Y) + center;
        }

        /// <summary>
        /// 获取该向量整数化后的值; 向下取整.
        /// </summary>
        /// <param name="vec">进行整数化计算的向量.</param>
        /// <returns></returns>
        public static Vector2 GetFloor( this Vector2 vec )
        {
            return new Vector2((int)vec.X,(int)vec.Y);
        }

        public static Vector2 GetAngle( this float value )
        {
            return new Vector2((float)Math.Cos(value),(float)Math.Sin(value));
        }

        /// <summary>
        /// 获取该向量与X轴的夹角.
        /// </summary>
        /// <param name="vec">参与计算的向量.</param>
        /// <returns></returns>
        public static float GetAngle( this Vector2 vec )
        {
            return (float)Math.Atan2(vec.Y,vec.X);
        }

        /// <summary>
        /// 获取该向量与指定向量的夹角.
        /// </summary>
        /// <param name="a">参与计算的向量.</param>
        /// <param name="b">参与计算的向量.</param>
        /// <returns></returns>
        public static float GetAngleBetween( this Vector2 a,Vector2 b )
        {
            return Math.Abs(a.GetAngle( ) - b.GetAngle( ));
        }

        /// <summary>
        /// 获取该向量整数化后的值; 向上取整.
        /// </summary>
        /// <param name="vec">参与计算的向量.</param>
        /// <returns></returns>
        public static Vector2 GetCeiling( this Vector2 vec )
        {
            return new Vector2((float)Math.Ceiling(vec.X),(float)Math.Ceiling(vec.Y));
        }

        /// <summary>
        /// 向量插值.
        /// </summary>
        /// <param name="a">参与计算的原向量.</param>
        /// <param name="b">目标向量.</param>
        /// <param name="progress">当前所在段数.</param>
        /// <param name="max">总分段数.</param>
        /// <returns></returns>
        public static Vector2 LerpTo( this Vector2 a,Vector2 b,float progress,float max )
        {
            return progress / max * b + (max - progress) / max * a;
        }

        /// <summary>
        /// 将向量以Y轴作为对称轴进行对称.
        /// </summary>
        /// <param name="vec">对称过后的向量.</param>
        /// <returns></returns>
        public static Vector2 FlipHorizontally( this Vector2 vec )
        {
            vec.X *= -1;
            return vec;
        }

        /// <summary>
        /// 将向量以X轴作为对称轴进行对称.
        /// </summary>
        /// <param name="vec">对称过后的向量.</param>
        /// <returns></returns>
        public static Vector2 FlipVertically( this Vector2 vec )
        {
            vec.Y *= -1;
            return vec;
        }

        /// <summary>
        ///  将当前向量转为单位向量; 并获取与原始矢量相同方向的单位向量.
        /// </summary>
        /// <param name="vec">参与计算的向量.</param>
        /// <returns>与原始向量相同方向的单位向量</returns>
        public static Vector2 NormalizeAlt( this Vector2 vec )
        {
            vec.Normalize( );
            return vec;
        }

        /// <summary>
        ///  将当前向量转为单位向量; 并获取与原始矢量相同方向的单位向量.
        /// </summary>
        /// <param name="vec">参与计算的向量.</param>
        /// <returns>与原始向量相同方向的单位向量</returns>
        public static Vector3 NormalizeAlt( this Vector3 vec )
        {
            vec.Normalize( );
            return vec;
        }

        /// <summary>
        /// 将该向量的X与Y增加指定的值.
        /// </summary>
        /// <param name="vec">参与计算的向量.</param>
        /// <param name="x">X 增量.</param>
        /// <param name="y">Y 增量</param>
        /// <returns></returns>
        public static Vector2 Plus( this Vector2 vec,float x,float y )
        {
            return vec + new Vector2(x,y);
        }

        /// <summary>
        /// 获取该向量的X向的对应长度向量.
        /// </summary>
        /// <param name="vec">参与计算的向量.</param>
        /// <returns></returns>
        public static Vector2 GetUnitX( this Vector2 vec )
        {
            return new Vector2(vec.X,0);
        }

        /// <summary>
        /// 获取该向量的Y向的对应长度向量.
        /// </summary>
        /// <param name="vec">参与计算的向量.</param>
        /// <returns></returns>
        public static Vector2 GetUnitY( this Vector2 vec )
        {
            return new Vector2(0,vec.Y);
        }

        /// <summary>
        /// 获取该向量的X向增加指定值后的对应长度向量.
        /// </summary>
        /// <param name="vec">参与计算的向量.</param>
        /// <param name="xVec">参与计算的增向量.</param>
        /// <returns></returns>
        public static Vector2 GetUnitXPlus( this Vector2 vec,Vector2 xVec )
        {
            return new Vector2(vec.X + xVec.X,0);
        }

        /// <summary>
        /// 获取该向量的Y向增加指定值后的对应长度向量.
        /// </summary>
        /// <param name="vec">参与计算的向量.</param>
        /// <param name="yVec">参与计算的增向量.</param>
        /// <returns></returns>
        public static Vector2 GetUnitYPlus( this Vector2 vec,Vector2 yVec )
        {
            return new Vector2(0,vec.Y + yVec.Y);
        }

        /// <summary>
        /// 获取该向量X与Y交换的值.
        /// </summary>
        /// <param name="a">参与计算的向量.</param>
        /// <returns></returns>
        public static Vector2 SwapXY( this Vector2 a )
        {
            return new Vector2(a.Y,a.X);
        }

        public static Vector2 GetVector2( this Vector3 vec )
        {
            return new Vector2(vec.X,vec.Y);
        }

        public static Color GetColor( this Vector3 vec,float a = 1 )
        {
            return new Color(vec.X,vec.Y,vec.Z,a);
        }

        public static float GetDot( this Vector3 vec1,Vector3 vec2 )
        {
            return Vector3.Dot(vec1,vec2);
        }

        public static Vector3 GetVector3( this Vector4 vec )
        {
            return new Vector3(vec.X,vec.Y,vec.Z);
        }
    }
}