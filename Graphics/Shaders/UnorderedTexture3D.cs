using SharpDX.Direct3D11;
using System.Reflection;
using Texture3D = Microsoft.Xna.Framework.Graphics.Texture3D;
namespace Colin.Core.Graphics.Shaders
{
  public class UnorderedAccessTexture3D : RenderTarget3D
  {
    private UnorderedAccessView _uav = null;
    public UnorderedAccessTexture3D(int width, int height, int depth) :
        base(CoreInfo.Graphics.GraphicsDevice, width, height, depth)
    {
    }
    public UnorderedAccessTexture3D(int width, int height, int depth, bool mipmap, SurfaceFormat format) :
        base(CoreInfo.Graphics.GraphicsDevice, width, height, depth, mipmap, format, default)
    {
    }
    public UnorderedAccessTexture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
         : base(graphicsDevice, width, height, depth, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage) { }

    public UnorderedAccessTexture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
        : base(graphicsDevice, width, height, depth, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents) { }

    public UnorderedAccessView GetUAV(Device D3dDevice)
    {
      if (_uav is null)
      {
        Texture3DDescription texDesc = this.GetTexture3DDescription();
        Resource resource = CreateTexture(D3dDevice);
        typeof(Texture).GetField("_texture", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, resource);

        UnorderedAccessViewDescription uavDesc = default;
        uavDesc.Format = texDesc.Format;
        uavDesc.Dimension = UnorderedAccessViewDimension.Texture3D;
        uavDesc.Texture3D.MipSlice = 0;
        uavDesc.Texture3D.FirstWSlice = 0;
        uavDesc.Texture3D.WSize = Depth;
        UnorderedAccessView uav = new(D3dDevice, resource, uavDesc);
        _uav = uav;
      }
      return _uav;
    }

    internal Resource CreateTexture(Device D3dDevice)
    {
      Texture3DDescription texture3DDescription = GetTexture3DDescription();
      return new SharpDX.Direct3D11.Texture3D(D3dDevice, texture3DDescription);
    }

    protected Texture3DDescription GetTexture3DDescription()
    {
      Texture3DDescription texDesc = default;
      texDesc.Width = Width;
      texDesc.Height = Height;
      texDesc.Depth = Depth;
      texDesc.MipLevels = LevelCount;
      texDesc.Format = (SharpDX.DXGI.Format)typeof(Texture3D).Module.GetType("Microsoft.Xna.Framework.SharpDXHelper")
          .GetMethods(BindingFlags.Public | BindingFlags.Static)
          .First(a => a.Name == "ToFormat" && a.GetParameters()[0].ParameterType == typeof(SurfaceFormat))
          .Invoke(null, new object[] { Format });
      texDesc.BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess | BindFlags.RenderTarget;
      texDesc.CpuAccessFlags = CpuAccessFlags.None;
      texDesc.Usage = ResourceUsage.Default;
      texDesc.OptionFlags = ResourceOptionFlags.None;
      return texDesc;
    }
  }
}
