namespace Colin.Core.Graphics.Tweens
{
  public class FloatTween : Tween<float, FloatTween>
  {
    protected override float Lerp(float a, float b, float t)
      => a + (b - a) * t;
  }
}
