namespace Colin.Core
{
  public static class DirectionConvert
  {
    public static int GetHorizontal(this Direction dir)
    {
      return (int)dir;
    }
  }
  public enum Direction
  {
    /// <summary>
    /// 朝左.
    /// </summary>
    Left = -1,
    /// <summary>
    /// 没有方向.
    /// </summary>
    None,
    /// <summary>
    /// 朝右.
    /// </summary>
    Right,
    /// <summary>
    /// 纵向.
    /// </summary>
    Vertical,
    /// <summary>
    /// 水平的.
    /// </summary>
    Horizontal,
    /// <summary>
    /// 朝上.
    /// </summary>
    Up,
    /// <summary>
    /// 朝下.
    /// </summary>
    Down,
    /// <summary>
    /// 居中.
    /// </summary>
    Center
  }
}