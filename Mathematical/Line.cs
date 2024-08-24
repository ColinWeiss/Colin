namespace Colin.Core
{
  /// <summary>
  /// 标识一根线.
  /// </summary>
  public struct Line
  {
    /// <summary>
    /// 线的起点.
    /// </summary>
    public Vector2 Start;

    /// <summary>
    /// 线的终点.
    /// </summary>
    public Vector2 End;

    /// <summary>
    /// 定义一根线.
    /// </summary>
    /// <param name="start">起点.</param>
    /// <param name="end">终点.</param>
    public Line(Vector2 start, Vector2 end)
    {
      Start = start;
      End = end;
    }

    /// <summary>
    /// 将该线转化为平面向量.
    /// </summary>
    public Vector2 ToVector2()
    {
      return End - Start;
    }


    public int GetChaji(Vector2 pos)
    {
      Vector2 a = ToVector2();
      Vector2 b = pos - Start;
      float v = (a.X * b.Y - b.X * a.Y);
      if (v > 0.000001f)
        return -1;
      else if (v < 0.000001f)
        return 1;
      else
        return 0;
    }

    public float GetCross(Vector2 pos)
    {
      return (float)Math.Sqrt((End.X - Start.X) * (pos.X - Start.X) + (End.Y - Start.Y) * (pos.Y - Start.Y));
    }

    public float GetDistance(Vector2 pos)
    {
      return (float)PL(pos.X, pos.Y, Start.X, Start.Y, End.X, End.Y);
    }

    public double PL(double x, double y, double x1, double y1, double x2, double y2)
    {
      double cross = (x2 - x1) * (x - x1) + (y2 - y1) * (y - y1); // |AB| * |AC|*cos(x)
                                                                  //double cross2 = (x1 - x2) * (x1 - x) + (y1 - y2) * (y1 - y); // |AB| * |AC|*cos(x)
      if (cross <= 0)  //积小于等于0，说明 角BAC 是直角或钝角
        return Math.Pow(((x - x1) * (x - x1) + (y - y1) * (y - y1) + 0.0), 0.5);

      double d2 = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1); // |AB|
      if (cross >= d2)  //角ABC是直角或钝角
        return Math.Pow(((x - x2) * (x - x2) + (y - y2) * (y - y2) + 0.0), 0.5);

      //锐角三角形
      double r = cross / d2;
      double px = x1 + (x2 - x1) * r;  // C在 AB上的垂足点（px，py）
      double py = y1 + (y2 - y1) * r;
      return Math.Pow(((x - px) * (x - px) + (y - py) * (y - py) + 0.0), 0.5); //两点间距离公式
    }
  }
}