using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Extensions
{
  public static class XNAExt
  {
    private static BlendState _humanityTranslucent;

    extension(BlendState blendState)
    {
      //实例全局缓存: 原实现每次访问都分配新的混合状态, 且会摧毁基于实例比较的批次状态机.
      public static BlendState HumanityTranslucent => _humanityTranslucent ??= new BlendState
      {
        ColorSourceBlend = Blend.SourceAlpha,
        AlphaSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.InverseSourceAlpha,
        AlphaDestinationBlend = Blend.InverseSourceAlpha
      };
    }
  }
}
