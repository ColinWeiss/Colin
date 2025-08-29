namespace Colin.Core.Graphics.Tweens
{
  public class FloatTween : Tweener<float>
  {
    public override float Calculate()
    {
      return MathHelper.Lerp(Target, Start, Percentage);
    }
  }
}