using Microsoft.Xna.Framework;

namespace Colin.Core.Graphics.Tweens
{
  public class Vector3Tween : Tween<Vector3, Vector3Tween>
  {
    protected override Vector3 Lerp(Vector3 a, Vector3 b, float t)
      => Vector3.Lerp(a, b, t);
  }
}
