namespace Colin.Core.Extensions
{
  public static class ColorExt
  {
    /// <summary>
    /// 对颜色进行线性插值.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="target"></param>
    /// <param name="i"></param>
    /// <param name="maxi"></param>
    /// <returns></returns>
    public static Color Closer(this ref Color current, Color target, float i, float maxi)
    {
      float r = current.R;
      float g = current.G;
      float b = current.B;
      float a = current.A;
      float tr = target.R;
      float tg = target.G;
      float tb = target.B;
      float ta = target.A;
      r *= maxi - i;
      r /= maxi;
      g *= maxi - i;
      g /= maxi;
      b *= maxi - i;
      b /= maxi;
      a *= maxi - i;
      a /= maxi;
      tr *= i;
      tr /= maxi;
      tg *= i;
      tg /= maxi;
      tb *= i;
      tb /= maxi;
      ta *= i;
      ta /= maxi;
      current = new Color((int)(r + tr), (int)(g + tg), (int)(b + tb), (int)(a + ta));
      return new Color((int)(r + tr), (int)(g + tg), (int)(b + tb), (int)(a + ta));
    }
  }
}