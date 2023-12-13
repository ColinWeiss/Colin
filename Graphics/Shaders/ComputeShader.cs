using SharpDX.Direct3D11;
using System.Reflection;
using XNA = Microsoft.Xna.Framework;

namespace Colin.Core.Graphics.Shaders
{
    public class ComputeShader
    {
        public GraphicsDevice GraphicsDevice { get; init; }
        public Device D3dDevice;
        public DeviceContext D3dDeviceContext;
        public SharpDX.Direct3D11.ComputeShader D3dComputeShader;
        public ComputeShader(GraphicsDevice graphicsDevice, byte[] data)
        {
            GraphicsDevice = graphicsDevice;
            D3dDevice = GraphicsDevice.GetType().GetField("_d3dDevice", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(GraphicsDevice)
                as Device;
            D3dDeviceContext = GraphicsDevice.GetType().GetField("_d3dContext", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(GraphicsDevice)
                as DeviceContext;
            D3dComputeShader = new(D3dDevice, data);
        }
        public void Dispatch(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
        {
            D3dDeviceContext.ComputeShader.Set(D3dComputeShader);
            D3dDeviceContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
        }
        public void SetTexture(int slot, Texture texture)
        {
            D3dDeviceContext.ComputeShader.SetShaderResource(slot, texture.GetType()
                .GetMethod("GetShaderResourceView", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(texture, null) as ShaderResourceView);
        }
        public void SetUnorderedTexture(int slot, UnorderedAccessTexture2D texture)
        {
            Texture2DDescription texDesc = texture.GetTexture2DDescriptionInternal();
            UnorderedAccessViewDescription uavDesc = default;
            uavDesc.Format = texDesc.Format;
            uavDesc.Dimension = UnorderedAccessViewDimension.Texture2D;
            uavDesc.Texture2D.MipSlice = 0;
            UnorderedAccessView uav = new(D3dDevice, typeof(Texture)
                .GetMethod("GetTexture", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(texture, null) as Resource, uavDesc);
            D3dDeviceContext.ComputeShader.SetUnorderedAccessView(slot, uav);
        }
    }
}