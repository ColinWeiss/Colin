using Colin.Extensions;
using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin
{
    /// <summary>
    /// 表示一个三角形.
    /// </summary>
    public struct Triangle
    {
        /// <summary>
        /// 顶点A
        /// </summary>
        public Vector2 VertexA;

        /// <summary>
        /// 顶点B
        /// </summary>
        public Vector2 VertexB;

        /// <summary>
        /// 顶点C
        /// </summary>
        public Vector2 VertexC;

        /// <summary>
        /// 顶点们
        /// </summary>
        public List<Vector2> Vertices
        {
            get { return new List<Vector2>( ) { VertexA, VertexB, VertexC }; }
        }

        public Triangle( Vector2 a, Vector2 b, Vector2 c )
        {
            VertexA = a;
            VertexB = b;
            VertexC = c;
        }

        /// <summary>
        /// 俩三角形是否发生碰撞
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public bool Collision( Triangle triangle )
        {
            //基于SAT理论实现的三角形碰撞
            Vector2 point, point1, n, myInterval, hisInterval;
            int i, j;
            for ( i = 0; i < 6; i++ )
            {
                if ( i < 3 )
                {
                    point = Vertices[ i ];
                    point1 = Vertices[ ( i + 1 ) % 3 ];
                }
                else
                {
                    point = triangle.Vertices[ i % 3 ];
                    point1 = triangle.Vertices[ ( i + 1 ) % 3 ];
                }
                n = new Vector2( point.Y - point1.Y, point1.X - point.X );
                myInterval = new Vector2( Math.Min( Math.Min( VertexA.X * n.X + VertexA.Y * n.Y, VertexB.X * n.X + VertexB.Y * n.Y ),
                    VertexC.X * n.X + VertexC.Y * n.Y ),
                    Math.Max( Math.Max( VertexA.X * n.X + VertexA.Y * n.Y, VertexB.X * n.X + VertexB.Y * n.Y ),
                        VertexC.X * n.X + VertexC.Y * n.Y ) );
                hisInterval = new Vector2( Math.Min( Math.Min( triangle.VertexA.X * n.X + triangle.VertexA.Y * n.Y, triangle.VertexB.X * n.X + triangle.VertexB.Y * n.Y ),
                    triangle.VertexC.X * n.X + triangle.VertexC.Y * n.Y ),
                    Math.Max( Math.Max( triangle.VertexA.X * n.X + triangle.VertexA.Y * n.Y, triangle.VertexB.X * n.X + triangle.VertexB.Y * n.Y ),
                        triangle.VertexC.X * n.X + triangle.VertexC.Y * n.Y ) );
                if ( myInterval.X < hisInterval.X )
                {
                    if ( myInterval.Y < hisInterval.X )
                        return false;
                }
                else
                {
                    if ( hisInterval.Y < myInterval.X )
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 三角形是否包含点
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contain( Vector2 point )
        {
            //当P=Ax+By+Cz(x+y+z=1)求得x、y、z全部大于0时点被三角形包含。此处使用行列式求解
            float d = 1 * VertexB.X * VertexC.Y + 1 * VertexC.X * VertexA.Y + 1 * VertexA.X * VertexB.Y - 1 * VertexB.X * VertexA.Y - 1 * VertexA.X * VertexC.Y - 1 * VertexC.X * VertexB.Y,
                d1 = 1 * VertexB.X * VertexC.Y + 1 * VertexC.X * point.Y + 1 * point.X * VertexB.Y - 1 * VertexB.X * point.Y - 1 * point.X * VertexC.Y - 1 * VertexC.X * VertexB.Y,
                d2 = 1 * point.X * VertexC.Y + 1 * VertexC.X * VertexA.Y + 1 * VertexA.X * point.Y - 1 * point.X * VertexA.Y - 1 * VertexA.X * VertexC.Y - 1 * VertexC.X * point.Y,
                d3 = 1 * VertexB.X * point.Y + 1 * point.X * VertexA.Y + 1 * VertexA.X * VertexB.Y - 1 * VertexB.X * VertexA.Y - 1 * VertexA.X * point.Y - 1 * point.X * VertexB.Y;
            if ( d == 0 )
                return false;
            return d1 / d > 0 && d2 / d > 0 && d3 / d > 0;
        }

        /// <summary>
        /// 在现基础上以某点为中心旋转三角形
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="center"></param>
        public void Rotated( float rotation, Vector2 center = default( Vector2 ) )
        {
            VertexA = VertexA.GetRotateTo( rotation, center );
            VertexB = VertexB.GetRotateTo( rotation, center );
            VertexC = VertexC.GetRotateTo( rotation, center );
        }

        /// <summary>
        /// 获取三角形质心
        /// </summary>
        /// <returns>质心坐标</returns>
        public Vector2 GetCentroid( ) => ( VertexA + VertexB + VertexC ) / 3f;

        /// <summary>
        /// 以质心为基点更改三角形大小
        /// </summary>
        /// <param name="scale"></param>
        public void ChangeScale( float scale )
        {
            Vector2 centroid = GetCentroid( ),
            aDirection = VertexA - centroid,
            bDirection = VertexB - centroid,
            cDirection = VertexC - centroid;
            VertexA = Vector2.Normalize( aDirection ) * aDirection.Length( ) * scale + centroid;
            VertexB = Vector2.Normalize( bDirection ) * bDirection.Length( ) * scale + centroid;
            VertexC = Vector2.Normalize( cDirection ) * cDirection.Length( ) * scale + centroid;
        }
    }
}