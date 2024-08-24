
namespace Colin.Core.Modulars.Collisions
{
  /// <summary>
  /// 利用多个 <see cref="Vector2"/> 以顺时针划成的多边形.
  /// </summary>
  public class Polygon
  {
    /// <summary>
    /// 指示划成该多边形的顶点数组.
    /// <br>[!] 其为模板顶点数组.</br>
    /// <br>[!] 若需要获取某个顶点当前状态, 请使用</br>
    /// </summary>
    public Vector2[] Vertices;

    /// <summary>
    /// 实际点与多边形的偏移量, offset为多边形的原点, 多边形的顶点位置都是相对于这个点的偏移量.
    /// </summary>
    public Vector2 Position;

    public float Rotation;

    public Line GetLine(int index0, int index1)
    {
      return new Line(Vertices[index0], Vertices[index1]);
    }

    public void Render()
    {
      Line[] Edges;
      Edges = new Line[Vertices.Length];
      for (int count = 0; count < Vertices.Length; count++)
      {
        Vector2 vertex0 = GetVertex(count < Vertices.Length - 1 ? count + 1 : 0);
        Vector2 vertex1 = GetVertex(count);
        Edges[count].Start = vertex1;
        Edges[count].End = vertex0;
      }
      for (int count = 0; count < Edges.Length; count++)
      {
        CoreInfo.Batch.DrawLine(Edges[count], Color.OrangeRed);
      }
    }

    public Vector2 GetVertex(int index) => Vertices[index].RotateBy(Rotation) + Position;

    public bool Overlaps(Polygon polygon)
    {
      for (int count = 0; count < Vertices.Length; count++)
      {
        Vector2 vertex0 = GetVertex(count < Vertices.Length - 1 ? count + 1 : 0);
        Vector2 vertex1 = GetVertex(count);
        Vector2 segment = vertex0 - vertex1;
        Vector2 segmentNormal = new Vector2(segment.Y, -segment.X);
        Vector2 aProjection = GetProjectionWithAxis(segmentNormal);
        Vector2 bProjection = polygon.GetProjectionWithAxis(segmentNormal);
        if (!ProjectionContains(aProjection, bProjection))
          return false;
      }
      for (int count = 0; count < polygon.Vertices.Length; count++)
      {
        Vector2 vertex0 = polygon.GetVertex(count < Vertices.Length - 1 ? count + 1 : 0);
        Vector2 vertex1 = polygon.GetVertex(count);
        Vector2 segment = vertex0 - vertex1;
        Vector2 segmentNormal = new Vector2(segment.Y, -segment.X);
        Vector2 aProjection = polygon.GetProjectionWithAxis(segmentNormal);
        Vector2 bProjection = GetProjectionWithAxis(segmentNormal);
        if (!ProjectionContains(aProjection, bProjection))
          return false;
      }
      return true;
    }

    private Vector2 GetProjectionWithAxis(Vector2 axis)
    {
      Vector2 axisN = Vector2.Normalize(axis);
      float min = Vector2.Dot(GetVertex(0), axisN);
      float max = min;
      for (int count = 0; count < Vertices.Length; count++)
      {
        float proj = Vector2.Dot(GetVertex(count), axisN);
        if (proj < min)
          min = proj;
        if (proj > max)
          max = proj;
      }
      min = (float)Math.Round(min, 2);
      max = (float)Math.Round(max, 2);
      return new Vector2(min, max);
    }

    private bool ProjectionContains(Vector2 a, Vector2 b)
        =>
        Math.Max(Math.Min(a.X, a.Y), Math.Min(b.X, b.Y))
        <=
        Math.Min(Math.Max(a.X, a.Y), Math.Max(b.X, b.Y));
  }
}