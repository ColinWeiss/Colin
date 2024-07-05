namespace Colin.Core
{
  /// <summary>
  /// 定义具有三个组件的三角形.
  /// </summary>
  public struct Triangle
  {
    /// <summary>
    /// 顶点A.
    /// </summary>
    public Vector2 VertexA;

    /// <summary>
    /// 顶点B.
    /// </summary>
    public Vector2 VertexB;

    /// <summary>
    /// 顶点C.
    /// </summary>
    public Vector2 VertexC;

    /// <summary>
    /// 以 A、B、C 作为元素的顶点数组.
    /// </summary>
    public Vector2[] Vertices => new Vector2[] { VertexA, VertexB, VertexC };

    /// <summary>
    /// 定义具有三个组件的三角形.
    /// </summary>
    /// <param name="a">顶点A.</param>
    /// <param name="b">顶点B.</param>
    /// <param name="c">顶点C.</param>
    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
      VertexA = a;
      VertexB = b;
      VertexC = c;
    }

    /// <summary>
    /// 判断两个三角形是否发生重叠.
    /// </summary>
    /// <param name="triangle">三角形.</param>
    /// <returns>若重叠, 则返回 <see href="true"/>, 否则返回 <see href="false"/>.</returns>
    public bool Collision(Triangle triangle)
    {
      //基于SAT理论实现的三角形碰撞
      Vector2 point, point1, n, myInterval, hisInterval;
      int i;
      for (i = 0; i < 6; i++)
      {
        if (i < 3)
        {
          point = Vertices[i];
          point1 = Vertices[(i + 1) % 3];
        }
        else
        {
          point = triangle.Vertices[i % 3];
          point1 = triangle.Vertices[(i + 1) % 3];
        }
        n = new Vector2(point.Y - point1.Y, point1.X - point.X);
        myInterval = new Vector2(Math.Min(Math.Min(VertexA.X * n.X + VertexA.Y * n.Y, VertexB.X * n.X + VertexB.Y * n.Y),
            VertexC.X * n.X + VertexC.Y * n.Y),
            Math.Max(Math.Max(VertexA.X * n.X + VertexA.Y * n.Y, VertexB.X * n.X + VertexB.Y * n.Y),
                VertexC.X * n.X + VertexC.Y * n.Y));
        hisInterval = new Vector2(Math.Min(Math.Min(triangle.VertexA.X * n.X + triangle.VertexA.Y * n.Y, triangle.VertexB.X * n.X + triangle.VertexB.Y * n.Y),
            triangle.VertexC.X * n.X + triangle.VertexC.Y * n.Y),
            Math.Max(Math.Max(triangle.VertexA.X * n.X + triangle.VertexA.Y * n.Y, triangle.VertexB.X * n.X + triangle.VertexB.Y * n.Y),
                triangle.VertexC.X * n.X + triangle.VertexC.Y * n.Y));
        if (myInterval.X < hisInterval.X)
        {
          if (myInterval.Y < hisInterval.X)
            return false;
        }
        else
        {
          if (hisInterval.Y < myInterval.X)
            return false;
        }
      }
      return true;
    }

    /// <summary>
    /// 判断三角形是否包含指定点.
    /// </summary>
    /// <param name="point">判断点.</param>
    /// <returns>若包含, 则返回 <see href="true"/>, 否则返回 <see href="false"/>.</returns>
    public bool Contain(Vector2 point)
    {
      //当P=Ax+By+Cz(x+y+z=1)求得x、y、z全部大于0时点被三角形包含。此处使用行列式求解
      float d = 1 * VertexB.X * VertexC.Y + 1 * VertexC.X * VertexA.Y + 1 * VertexA.X * VertexB.Y - 1 * VertexB.X * VertexA.Y - 1 * VertexA.X * VertexC.Y - 1 * VertexC.X * VertexB.Y,
          d1 = 1 * VertexB.X * VertexC.Y + 1 * VertexC.X * point.Y + 1 * point.X * VertexB.Y - 1 * VertexB.X * point.Y - 1 * point.X * VertexC.Y - 1 * VertexC.X * VertexB.Y,
          d2 = 1 * point.X * VertexC.Y + 1 * VertexC.X * VertexA.Y + 1 * VertexA.X * point.Y - 1 * point.X * VertexA.Y - 1 * VertexA.X * VertexC.Y - 1 * VertexC.X * point.Y,
          d3 = 1 * VertexB.X * point.Y + 1 * point.X * VertexA.Y + 1 * VertexA.X * VertexB.Y - 1 * VertexB.X * VertexA.Y - 1 * VertexA.X * point.Y - 1 * point.X * VertexB.Y;
      if (d == 0)
        return false;
      return d1 / d > 0 && d2 / d > 0 && d3 / d > 0;
    }

    /// <summary>
    /// 在现基础上将三角形以某点为中心旋转至一定角度.
    /// </summary>
    /// <param name="rotation">旋转角度.</param>
    /// <param name="center">旋转中心.</param>
    public void Rotated(float rotation, Vector2 center = default)
    {
      VertexA = VertexA.GetRotateTo(rotation, center);
      VertexB = VertexB.GetRotateTo(rotation, center);
      VertexC = VertexC.GetRotateTo(rotation, center);
    }

    /// <summary>
    /// 获取三角形质心的坐标值.
    /// </summary>
    /// <returns>质心坐标.</returns>
    public Vector2 Centroid => (VertexA + VertexB + VertexC) / 3f;

    /// <summary>
    /// 以质心为基点缩放三角形.
    /// </summary>
    /// <param name="scale">缩放倍数, 单位为 1.</param>
    public void ChangeScale(float scale)
    {
      Vector2 centroid = Centroid,
      aDirection = VertexA - centroid,
      bDirection = VertexB - centroid,
      cDirection = VertexC - centroid;
      VertexA = Vector2.Normalize(aDirection) * aDirection.Length() * scale + centroid;
      VertexB = Vector2.Normalize(bDirection) * bDirection.Length() * scale + centroid;
      VertexC = Vector2.Normalize(cDirection) * cDirection.Length() * scale + centroid;
    }
  }
}