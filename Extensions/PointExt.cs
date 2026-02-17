namespace Colin.Core.Extensions
{
  public static class PointExt
  {
    public static void LoadStep(this ref Point point, BinaryReader reader)
    {
      point.X = reader.ReadInt32();
      point.Y = reader.ReadInt32();
    }
    public static void SaveStep(this Point point, BinaryWriter writer)
    {
      writer.Write(point.X);
      writer.Write(point.Y);
    }
    public static void LoadStep(this ref Point3 point, BinaryReader reader)
    {
      point.X = reader.ReadInt32();
      point.Y = reader.ReadInt32();
      point.Z = reader.ReadInt32();
    }
    public static void SaveStep(this Point3 point, BinaryWriter writer)
    {
      writer.Write(point.X);
      writer.Write(point.Y);
      writer.Write(point.Z);
    }
    public static Point3 ToPoint3(this Point point)
    {
      return new Point3(point.X, point.Y, 0);
    }
  }
}