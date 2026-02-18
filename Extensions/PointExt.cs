using Colin.Core.IO;

namespace Colin.Core.Extensions
{
  public static class PointExt
  {
    public static void LoadStep(this ref Point point, StoreBox box)
    {
      point.X = box.GetInt("X");
      point.Y = box.GetInt("Y");
    }
    public static StoreBox SaveStep(this Point point)
    {
      StoreBox box = new StoreBox();
      box.Add("X", point.X);
      box.Add("Y", point.Y);
      return box;
    }
    public static void LoadStep(this ref Point3 point, StoreBox box)
    {
      point.X = box.GetInt("X");
      point.Y = box.GetInt("Y");
      point.Z = box.GetInt("Z");
    }
    public static StoreBox SaveStep(this Point3 point)
    {
      StoreBox box = new StoreBox();
      box.Add("X", point.X);
      box.Add("Y", point.Y);
      box.Add("Z", point.Z);
      return box;
    }
    public static Point3 ToPoint3(this Point point)
    {
      return new Point3(point.X, point.Y, 0);
    }
  }
}