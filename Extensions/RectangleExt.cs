namespace Colin.Core.Extensions
{
  public static class RectangleExt
  {
    /// <summary>
    /// 获取矩形左边缘的中心位置.
    /// </summary>
    public static Vector2 GetLeftCenter(this Rectangle rect)
    {
      return new Vector2(rect.Left, rect.Y + rect.Height / 2.0f);
    }

    /// <summary>
    /// 获取矩形右边缘的中心位置.
    /// </summary>
    public static Vector2 GetRightCenter(this Rectangle rect)
    {
      return new Vector2(rect.Right, rect.Y + rect.Height / 2.0f);
    }

    /// <summary>
    /// 获取矩形上边缘的中心位置.
    /// </summary>
    public static Vector2 GetTopCenter(this Rectangle rect)
    {
      return new Vector2(rect.X + rect.Width / 2.0f, rect.Top);
    }

    /// <summary>
    /// 获取矩形下边缘的中心位置.
    /// </summary>
    public static Vector2 GetBottomCenter(this Rectangle rect)
    {
      return new Vector2(rect.X + rect.Width / 2.0f, rect.Bottom);
    }

    /// <summary>
    /// 获取相交深度.
    /// </summary>
    /// <param name="rectA"></param>
    /// <param name="rectB"></param>
    /// <returns></returns>
    public static Vector2 GetIntersectionDepth(this Rectangle rectA, Rectangle rectB)
    {
      float halfWidthA = rectA.Width / 2.0f;
      float halfHeightA = rectA.Height / 2.0f;
      float halfWidthB = rectB.Width / 2.0f;
      float halfHeightB = rectB.Height / 2.0f;
      Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
      Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);
      float distanceX = centerA.X - centerB.X;
      float distanceY = centerA.Y - centerB.Y;
      float minDistanceX = halfWidthA + halfWidthB;
      float minDistanceY = halfHeightA + halfHeightB;
      if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
        return Vector2.Zero;
      float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
      float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
      return new Vector2(depthX, depthY);
    }
  }
}