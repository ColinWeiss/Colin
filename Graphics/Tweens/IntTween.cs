namespace Colin.Core.Graphics.Tweens
{
  public class IntTween : Tween<int, IntTween>
  {
    protected override int Lerp(int a, int b, float t)
      => (int)(a + (b - a) * t);
  }
}
