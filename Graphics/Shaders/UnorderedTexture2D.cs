using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;
namespace Colin.Core.Graphics.Shaders
{
    public class UnorderedAccessTexture2D : RenderTarget2D
    {
        public UnorderedAccessTexture2D(GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice, width, height) { }

        public UnorderedAccessTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
             : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared, 1) { }

        public UnorderedAccessTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
            : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared: false) { }

        public UnorderedAccessTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
            : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents) { }

        public UnorderedAccessTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared, int arraySize)
            : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared, arraySize) { }

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
