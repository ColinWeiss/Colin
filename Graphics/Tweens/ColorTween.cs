using Microsoft.Xna.Framework;

namespace Colin.Core.Graphics.Tweens
{
  public class ColorTween : Tween<Color, ColorTween>
  {
    protected override Color Lerp(Color a, Color b, float t)
      => Color.Lerp(a, b, t);
  }
}
