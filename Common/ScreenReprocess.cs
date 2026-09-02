using Colin.Core.Graphics.Bridge;
using Colin.Core.Graphics.Tinters;
using ComputeSharp;
using System.Security.Cryptography;

namespace Colin.Core.Common
{
  public class ScreenReprocess
  {
    public Dictionary<IRenderableISceneModule, Effect> Effects = new Dictionary<IRenderableISceneModule, Effect>();

    public Dictionary<Type, Effect> TypeCheck = new Dictionary<Type, Effect>();

    public void Add(IRenderableISceneModule iRComponent, Effect e)
    {
      Effects.Add(iRComponent, e);
      TypeCheck.Add(iRComponent.GetType(), e);
    }


    public Texture2D TestProcess(RenderTarget2D rt)
    {
      Texture2D t = CoreInfo.Tinter.Process(rt,
         (src, dst) => new GrayScaleShader(src, dst, 500f));
       // (src, dst) => new VignetteShader(src, dst));
      return t;
    }
  }
}