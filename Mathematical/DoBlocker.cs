namespace Colin.Core.Mathematical
{
  public interface IDoBlockable
  {
    public RectangleF Bounds { get; }
  }
  public class DoBlocker
  {
    public static Dictionary<Point, List<T>> Do<T>(IEnumerable<T> list, int size) where T : IDoBlockable
    {
      var result = new Dictionary<Point, List<T>>();
      T target;
      RectangleF bounds;
      Point blockCoord;
      int startX;
      int endX;
      int startY;
      int endY;
      IEnumerable<T> interactLayer = list; //交互层
      if (list is null)
        return null;
      for (int c = 0; c < interactLayer.Count(); c++)
      {
        target = interactLayer.ElementAt(c); // 获取目标碰撞器对象
        if (target is null)
          continue;
        bounds = target.Bounds;
        startX = (int)Math.Floor(bounds.Left / size);
        endX = (int)Math.Floor(bounds.Right / size);
        startY = (int)Math.Floor(bounds.Top / size);
        endY = (int)Math.Floor(bounds.Bottom / size);
        blockCoord = new Point(startX, startY);
        if (!result.ContainsKey(blockCoord))
          result[blockCoord] = new List<T>();
        result[blockCoord].Add(target);
      }
      return result;
    }
  }
}