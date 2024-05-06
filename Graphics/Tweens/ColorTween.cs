namespace Colin.Core.Graphics.Tweens
{
  public class ColorTween : Tweener<Color>
  {
    public override Color Calculate()
    {
      return Current.Closer(Target, Percentage, 1f);
    }
  }
}