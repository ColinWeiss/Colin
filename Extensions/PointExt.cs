using Colin.Core.IO;

namespace Colin.Core.Extensions
{
  public static class PointExt
  {
    extension(ref Point p)
    {
      public static Point Left => new Point(-1, 0);
      public static Point Top => new Point(0, -1);
      public static Point Right => new Point(1, 0);
      public static Point Down => new Point(0, 1);
      public static Point[] Around => new Point[]
      {
        Point.Left,
        Point.Top,
        Point.Right,
        Point.Down
      };
    }
    extension(Point p)
    {
      public Point3 ToPoint3()
      {
        return new Point3(p.X, p.Y, 0);
      }
    }
  }
}