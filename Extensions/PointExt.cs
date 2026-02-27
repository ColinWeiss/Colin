using Colin.Core.IO;
using SharpDX.Direct3D9;

namespace Colin.Core.Extensions
{
  public static class PointExt
  {
    extension(Point p)
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

      public Point3 ToPoint3()
      {
        return new Point3(p.X, p.Y, 0);
      }

      public void LoadStep(BinaryReader reader)
      {
        p.X = reader.ReadInt32();
        p.Y = reader.ReadInt32();
      }
      public void SaveStep(BinaryWriter writer)
      {
        writer.Write(p.X);
        writer.Write(p.Y);
      }
    }
    extension(Point3 p)
    {
      public void LoadStep(BinaryReader reader)
      {
        p.X = reader.ReadInt32();
        p.Y = reader.ReadInt32();
        p.Z = reader.ReadInt32();
      }
      public void SaveStep(BinaryWriter writer)
      {
        writer.Write(p.X);
        writer.Write(p.Y);
        writer.Write(p.Z);
      }
    }
  }
}