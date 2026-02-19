using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Extensions
{
  public static class XNAExt
  {
    extension(BlendState blendState)
    {
      public static BlendState HumanityTranslucent => new BlendState
      {
        ColorSourceBlend = Blend.SourceAlpha,
        AlphaSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.InverseSourceAlpha,
        AlphaDestinationBlend = Blend.InverseSourceAlpha
      };
    }
  }
}
