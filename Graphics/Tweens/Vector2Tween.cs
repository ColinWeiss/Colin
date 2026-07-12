using Microsoft.Xna.Framework;

namespace Colin.Core.Graphics.Tweens
{
  public class Vector2Tween : Tween<Vector2, Vector2Tween>
  {
    protected override Vector2 Lerp(Vector2 a, Vector2 b, float t)
      => Vector2.Lerp(a, b, t);
  }
}
