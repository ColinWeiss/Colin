using System.Reflection;
using D3D = SharpDX.Direct3D11;
using XNA = Microsoft.Xna.Framework;

namespace Colin.Core.Graphics.Shaders
{
    public class ComputeShader
    {
        public GraphicsDevice GraphicsDevice { get; init; }
        D3D.Device _d3dDevice;
        D3D.DeviceContext _d3dContext;
        D3D.ComputeShader _d3dShader;
        public ComputeShader(GraphicsDevice graphicsDevice, byte[] data)
        {
            GraphicsDevice = graphicsDevice;
            _d3dDevice = GraphicsDevice.GetType().GetField("_d3dDevice", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(GraphicsDevice)
                as D3D.Device;
            _d3dContext = GraphicsDevice.GetType().GetField("_d3dContext", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(GraphicsDevice)
                as D3D.DeviceContext;
            _d3dShader = new(_d3dDevice, data);
        }
        public void Dispatch(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
        {
            _d3dContext.ComputeShader.Set(_d3dShader);
            _d3dContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
        }
        public void SetTexture(int slot, Texture texture)
        {
            _d3dContext.ComputeShader.SetShaderResource(slot, texture.GetType()
                .GetMethod("GetShaderResourceView", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(texture, null) as D3D.ShaderResourceView);
        }
        public void SetUnorderedTexture(int slot, UnorderedAccessTexture2D texture)
        {
            D3D.Texture2DDescription texDesc = texture.GetTexture2DDescriptionInternal();
            D3D.UnorderedAccessViewDescription uavDesc = default;
            uavDesc.Format = texDesc.Format;
            uavDesc.Dimension = D3D.UnorderedAccessViewDimension.Texture2D;
            uavDesc.Texture2D.MipSlice = 0;
            D3D.UnorderedAccessView uav = new(_d3dDevice, typeof(Texture)
                .GetMethod("GetTexture", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(texture, null) as D3D.Resource, uavDesc);
            _d3dContext.ComputeShader.SetUnorderedAccessView(slot, uav);
        }
    }
}