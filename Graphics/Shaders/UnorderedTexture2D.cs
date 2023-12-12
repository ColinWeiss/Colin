using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;
namespace Colin.Core.Graphics.Shaders
{
    public class UnorderedAccessTexture2D : Texture2D
    {
        public UnorderedAccessTexture2D(int width, int height) :
            base(EngineInfo.Graphics.GraphicsDevice, width, height)
        {
        }
        public UnorderedAccessTexture2D(int width, int height, bool mipmap, SurfaceFormat format) :
            base(EngineInfo.Graphics.GraphicsDevice, width, height, mipmap, format)
        {
        }
        public UnorderedAccessTexture2D(int width, int height, bool mipmap, SurfaceFormat format, int arraySize) :
            base(EngineInfo.Graphics.GraphicsDevice, width, height, mipmap, format, arraySize)
        {
        }
        protected UnorderedAccessTexture2D(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared, int arraySize) :
            base(EngineInfo.Graphics.GraphicsDevice, width, height, mipmap, format, type, shared, arraySize)
        {
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
            texDesc.BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess;
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
