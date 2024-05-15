using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Graphics.Tweens
{
  public class VectorTween : Tweener<Vector2>
  {
    public override Vector2 Calculate()
    {
      return Current.Closer(Target, Percentage, 1f);
    }
  }
}