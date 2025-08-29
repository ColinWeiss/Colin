namespace Colin.Core.Modulars.Collisions
{
  public static class CollisionHandle
  {
    /// <summary>
    /// 检测两个形状是否发生碰撞
    /// </summary>
    public static bool CheckCollision(Shape shape1, Shape shape2)
    {
      switch (shape1)
      {
        case PolygonShape polygon1 when shape2 is PolygonShape polygon2:
          return CheckPolygonVsPolygon(polygon1, polygon2);
        case CircleShape circle1 when shape2 is CircleShape circle2:
          return CheckCircleVsCircle(circle1, circle2);
        case SectorShape sector1 when shape2 is SectorShape sector2:
          return CheckSectorVsSector(sector1, sector2);
        case PolygonShape polygon when shape2 is CircleShape circle:
          return CheckPolygonVsCircle(polygon, circle);
        case CircleShape circle when shape2 is PolygonShape polygon:
          return CheckPolygonVsCircle(polygon, circle);
        case SectorShape sector when shape2 is CircleShape circle:
          return CheckSectorVsCircle(sector, circle);
        case CircleShape circle when shape2 is SectorShape sector:
          return CheckSectorVsCircle(sector, circle);
        case SectorShape sector when shape2 is PolygonShape polygon:
          return CheckSectorVsPolygon(sector, polygon);
        case PolygonShape polygon when shape2 is SectorShape sector:
          return CheckSectorVsPolygon(sector, polygon);
        default:
          return false;
      }
    }

    /// <summary>
    /// 检测多边形与多边形是否发生碰撞
    /// </summary>
    private static bool CheckPolygonVsPolygon(PolygonShape polygon1, PolygonShape polygon2)
    {
      List<Vector2> axes = polygon1.GetAxes();
      axes.AddRange(polygon2.GetAxes());

      foreach (var axis in axes)
      {
        if (!OverlapOnAxis(axis, polygon1, polygon2))
        {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// 检测圆形与圆形是否发生碰撞
    /// </summary>
    private static bool CheckCircleVsCircle(CircleShape circle1, CircleShape circle2)
    {
      float distance = Vector2.Distance(circle1.Position, circle2.Position);
      return distance <= circle1.Radius + circle2.Radius;
    }

    /// <summary>
    /// 检测扇形与扇形是否发生碰撞
    /// </summary>
    private static bool CheckSectorVsSector(SectorShape sector1, SectorShape sector2)
    {
      Vector2 relativePos = sector2.Position - sector1.Position;
      float distance = relativePos.Length();
      if (distance > sector1.Radius + sector2.Radius)
      {
        return false;
      }

      float angle = (float)Math.Atan2(relativePos.Y, relativePos.X);
      return sector1.IsAngleInRange(angle) && sector2.IsAngleInRange(angle);
    }

    /// <summary>
    /// 检测多边形与圆形是否发生碰撞
    /// </summary>
    private static bool CheckPolygonVsCircle(PolygonShape polygon, CircleShape circle)
    {
      // 1. 检测多边形顶点是否在圆内
      foreach (var vertex in polygon.Vertices)
      {
        Vector2 worldVertex = vertex + polygon.Position;
        if (Vector2.Distance(worldVertex, circle.Position) <= circle.Radius)
        {
          return true;
        }
      }

      // 2. 检测圆的圆心是否在多边形内
      if (IsPointInPolygon(circle.Position, polygon))
      {
        return true;
      }

      // 3. 检测多边形边是否与圆相交
      for (int i = 0; i < polygon.Vertices.Count; i++)
      {
        Vector2 p1 = polygon.Vertices[i] + polygon.Position;
        Vector2 p2 = polygon.Vertices[(i + 1) % polygon.Vertices.Count] + polygon.Position;

        if (IsCircleIntersectingLineSegment(circle.Position, circle.Radius, p1, p2))
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// 检测扇形与圆形是否发生碰撞
    /// </summary>
    private static bool CheckSectorVsCircle(SectorShape sector, CircleShape circle)
    {
      Vector2 relativePos = circle.Position - sector.Position;
      float distance = relativePos.Length();
      if (distance > sector.Radius + circle.Radius)
      {
        return false;
      }

      float angle = (float)Math.Atan2(relativePos.Y, relativePos.X);
      return sector.IsAngleInRange(angle);
    }

    /// <summary>
    /// 检测扇形与多边形是否发生碰撞
    /// </summary>
    private static bool CheckSectorVsPolygon(SectorShape sector, PolygonShape polygon)
    {
      // 1. 检测多边形的顶点是否在扇形内
      foreach (var vertex in polygon.Vertices)
      {
        Vector2 worldVertex = vertex + polygon.Position;
        Vector2 relativePos = worldVertex - sector.Position;
        float angle = (float)Math.Atan2(relativePos.Y, relativePos.X);
        if (sector.IsAngleInRange(angle) && relativePos.Length() <= sector.Radius)
        {
          return true;
        }
      }

      // 2. 检测扇形的边是否与多边形的边相交
      for (int i = 0; i < polygon.Vertices.Count; i++)
      {
        Vector2 p1 = polygon.Vertices[i] + polygon.Position;
        Vector2 p2 = polygon.Vertices[(i + 1) % polygon.Vertices.Count] + polygon.Position;

        if (IsSectorIntersectingLineSegment(sector, p1, p2))
        {
          return true;
        }
      }

      // 3. 检测扇形的圆心是否在多边形内
      if (IsPointInPolygon(sector.Position, polygon))
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// 检测扇形是否与线段相交
    /// </summary>
    private static bool IsSectorIntersectingLineSegment(SectorShape sector, Vector2 p1, Vector2 p2)
    {
      // 检测线段是否与扇形的弧相交
      if (IsLineSegmentIntersectingArc(sector, p1, p2))
      {
        return true;
      }

      // 检测线段是否与扇形的两条边相交
      Vector2 startPoint = sector.Position + new Vector2(
          sector.Radius * (float)Math.Cos(sector.StartAngle),
          sector.Radius * (float)Math.Sin(sector.StartAngle)
      );
      Vector2 endPoint = sector.Position + new Vector2(
          sector.Radius * (float)Math.Cos(sector.StartAngle + sector.SweepAngle),
          sector.Radius * (float)Math.Sin(sector.StartAngle + sector.SweepAngle)
      );

      if (IsLineSegmentIntersectingLineSegment(p1, p2, sector.Position, startPoint) ||
          IsLineSegmentIntersectingLineSegment(p1, p2, sector.Position, endPoint))
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// 检测线段是否与扇形的弧相交
    /// </summary>
    private static bool IsLineSegmentIntersectingArc(SectorShape sector, Vector2 p1, Vector2 p2)
    {
      // 计算线段与圆的交点
      List<Vector2> intersections = GetLineSegmentCircleIntersections(p1, p2, sector.Position, sector.Radius);

      foreach (var intersection in intersections)
      {
        Vector2 relativePos = intersection - sector.Position;
        float angle = (float)Math.Atan2(relativePos.Y, relativePos.X);
        if (sector.IsAngleInRange(angle))
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// 获取线段与圆的交点
    /// </summary>
    private static List<Vector2> GetLineSegmentCircleIntersections(Vector2 p1, Vector2 p2, Vector2 center, float radius)
    {
      List<Vector2> intersections = new List<Vector2>();

      Vector2 d = p2 - p1;
      Vector2 f = p1 - center;

      float a = Vector2.Dot(d, d);
      float b = 2 * Vector2.Dot(f, d);
      float c = Vector2.Dot(f, f) - radius * radius;

      float discriminant = b * b - 4 * a * c;

      if (discriminant >= 0)
      {
        discriminant = (float)Math.Sqrt(discriminant);
        float t1 = (-b - discriminant) / (2 * a);
        float t2 = (-b + discriminant) / (2 * a);

        if (t1 >= 0 && t1 <= 1)
        {
          intersections.Add(p1 + t1 * d);
        }
        if (t2 >= 0 && t2 <= 1)
        {
          intersections.Add(p1 + t2 * d);
        }
      }

      return intersections;
    }

    /// <summary>
    /// 检测两条线段是否相交
    /// </summary>
    private static bool IsLineSegmentIntersectingLineSegment(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {

      float cross1 = VectorExt.Cross(p2 - p1, q1 - p1);
      float cross2 = VectorExt.Cross(p2 - p1, q2 - p1);
      float cross3 = VectorExt.Cross(q2 - q1, p1 - q1);
      float cross4 = VectorExt.Cross(q2 - q1, p2 - q1);

      return (cross1 * cross2 < 0) && (cross3 * cross4 < 0);
    }

    /// <summary>
    /// 检测点是否在多边形内
    /// </summary>
    private static bool IsPointInPolygon(Vector2 point, PolygonShape polygon)
    {
      bool inside = false;
      for (int i = 0, j = polygon.Vertices.Count - 1; i < polygon.Vertices.Count; j = i++)
      {
        Vector2 p1 = polygon.Vertices[i] + polygon.Position;
        Vector2 p2 = polygon.Vertices[j] + polygon.Position;

        if ((p1.Y > point.Y) != (p2.Y > point.Y) &&
            point.X < (p2.X - p1.X) * (point.Y - p1.Y) / (p2.Y - p1.Y) + p1.X)
        {
          inside = !inside;
        }
      }
      return inside;
    }

    /// <summary>
    /// 检测圆是否与线段相交
    /// </summary>
    private static bool IsCircleIntersectingLineSegment(Vector2 circleCenter, float radius, Vector2 p1, Vector2 p2)
    {
      Vector2 closestPoint = GetClosestPointOnLineSegment(circleCenter, p1, p2);
      float distance = Vector2.Distance(circleCenter, closestPoint);
      return distance <= radius;
    }

    /// <summary>
    /// 获取线段上距离点最近的点
    /// </summary>
    private static Vector2 GetClosestPointOnLineSegment(Vector2 point, Vector2 p1, Vector2 p2)
    {
      Vector2 line = p2 - p1;
      Vector2 pointToP1 = point - p1;
      float t = Vector2.Dot(pointToP1, line) / line.LengthSquared();

      t = MathHelper.Clamp(t, 0, 1);
      return p1 + t * line;
    }

    /// <summary>
    /// 检查两个多边形在给定轴上的投影是否重叠
    /// </summary>
    private static bool OverlapOnAxis(Vector2 axis, PolygonShape polygon1, PolygonShape polygon2)
    {
      float min1, max1, min2, max2;
      polygon1.ProjectOntoAxis(axis, out min1, out max1);
      polygon2.ProjectOntoAxis(axis, out min2, out max2);
      return !(max1 < min2 || max2 < min1);
    }
  }
}
