using Colin.Core.Graphics.Bridge;
using Colin.Core.Graphics.Tinters;
using ComputeSharp;
using System.Security.Cryptography;

namespace Colin.Core.Common
{
  public class ModulePostProcessor
  {
    public Dictionary<IRenderableISceneModule, IModulePostPass> Passes;

    public ModulePostProcessor()
    {
      Passes = new();
    }

    public void Add(IRenderableISceneModule iRComponent, IModulePostPass e)
    {
      Passes.Add(iRComponent, e);
    }
  }
  public interface IModulePostPass
  {
    public Texture2D PostProcess(RenderTarget2D rt);
  }
}