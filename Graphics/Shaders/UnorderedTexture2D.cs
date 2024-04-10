using SharpDX.Direct3D11;
using System.Reflection;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;
namespace Colin.Core.Graphics.Shaders
{
  public class UnorderedAccessTexture2D : RenderTarget2D
  {
    private UnorderedAccessView _uav = null;
    public UnorderedAccessTexture2D(int width, int height) :
        base(EngineInfo.Graphics.GraphicsDevice, width, height)
    {
    }
    public UnorderedAccessTexture2D(int width, int height, bool mipmap, SurfaceFormat format) :
        base(EngineInfo.Graphics.GraphicsDevice, width, height, mipmap, format, default)
    {
    }
    public UnorderedAccessTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
         : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared, 1) { }

    public UnorderedAccessTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared: false) { }

    public UnorderedAccessTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
        : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents) { }

    public UnorderedAccessTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared, int arraySize)
        : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared, arraySize) { }

    public UnorderedAccessView GetUAV(Device D3dDevice)
    {
      if (_uav is null)
      {
        Texture2DDescription texDesc = this.GetTexture2DDescriptionInternal();
        UnorderedAccessViewDescription uavDesc = default;
        uavDesc.Format = texDesc.Format;
        uavDesc.Dimension = UnorderedAccessViewDimension.Texture2D;
        uavDesc.Texture2D.MipSlice = 0;
        UnorderedAccessView uav = new(D3dDevice, typeof(Texture)
            .GetMethod("GetTexture", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(this, null) as Resource, uavDesc);
        _uav = uav;
      }
      return _uav;
    }
    internal Texture2DDescription GetTexture2DDescriptionInternal()
    {
      return GetTexture2DDescription();
    }
    protected override Texture2DDescription GetTexture2DDescription()
    {
      Texture2DDescription texDesc = default;
      texDesc.Width = Width;
      texDesc.Height = Height;
      texDesc.MipLevels = LevelCount;
      texDesc.ArraySize = (int)typeof(Texture2D).GetField("ArraySize", BindingFlags.NonPublic | BindingFlags.Instance)
          .GetValue(this);
      texDesc.Format = (SharpDX.DXGI.Format)typeof(Texture2D).Module.GetType("Microsoft.Xna.Framework.SharpDXHelper")
          .GetMethods(BindingFlags.Public | BindingFlags.Static)
          .First(a => a.Name == "ToFormat" && a.GetParameters()[0].ParameterType == typeof(SurfaceFormat))
          .Invoke(null, new object[] { Format });
      texDesc.BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess | BindFlags.RenderTarget;
      texDesc.CpuAccessFlags = CpuAccessFlags.None;
      texDesc.SampleDescription = SampleDescription;
      texDesc.Usage = ResourceUsage.Default;
      texDesc.OptionFlags = ResourceOptionFlags.None;
      if ((bool)typeof(Texture2D).GetField("_shared", BindingFlags.NonPublic | BindingFlags.Instance)
          .GetValue(this))
      {
        texDesc.OptionFlags |= ResourceOptionFlags.Shared;
      }
      return texDesc;
    }
  }
}
