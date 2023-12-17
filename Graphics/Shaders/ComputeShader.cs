using SharpDX.Direct3D11;
using System.Reflection;
using System.Text;
using XNA = Microsoft.Xna.Framework;

namespace Colin.Core.Graphics.Shaders
{
    public class ComputeShader
    {
        public GraphicsDevice GraphicsDevice { get; init; }
        public Device D3dDevice;
        public DeviceContext D3dDeviceContext;
        public SharpDX.Direct3D11.ComputeShader D3dComputeShader;
        public SharpDX.Direct3D11.Buffer CBuffer { get; private set; } = null;

        private byte[] _buffer;
        private bool _bufferDirty = true;

        private int _bufferBegin = -1;

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

        public void CreateCBuffer(int bufferLength)
        {
            if (CBuffer is not null)
            {
                CBuffer.Dispose();
            }
            _buffer = new byte[bufferLength];
            BufferDescription description = default(BufferDescription);
            description.SizeInBytes = bufferLength;
            description.Usage = ResourceUsage.Default;
            description.BindFlags = BindFlags.ConstantBuffer;
            description.CpuAccessFlags = CpuAccessFlags.None;
            lock (D3dDeviceContext)
            {
                CBuffer = new SharpDX.Direct3D11.Buffer(D3dDevice, description);
            }
        }

        /// <summary>
        /// 提交Buffer数据到CBuffer中
        /// </summary>
        public void UpdateCBuffer()
        {
            if (_bufferDirty)
            {
                D3dDeviceContext.UpdateSubresource(_buffer, CBuffer);
                _bufferDirty = false;
            }
            D3dDeviceContext.ComputeShader.SetConstantBuffer(0, CBuffer);
        }

        /// <summary>
        /// 向CBuffer写入数据
        /// <br>会按照顺序写入</br>
        /// <br>会以16字节对齐（float4向量的大小）</br>
        /// <br>start: 起始地址（字节）</br>
        /// </summary>
        public int SetBufferData(int start, params object[] objects)
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream(_buffer));
            writer.Seek(start, SeekOrigin.Begin);
            for (int i = 0; i < objects.Length; i++)
            {
                start = (int)writer.BaseStream.Position;
                switch (objects[i])
                {

                    case float x: writer.Write(x); break;
                    case Vector2 v: writer.Write(v.X); writer.Write(v.Y); break;
                    case Vector3 v: writer.Write(v.X); writer.Write(v.Y); writer.Write(v.Z); break;
                    case Vector4 v: writer.Write(v.X); writer.Write(v.Y); writer.Write(v.Z); writer.Write(v.W); break;

                    case int x: writer.Write(x); break;
                    case Point p: writer.Write(p.X); writer.Write(p.Y); break;
                    case ValueTuple<int, int, int, int> t: 
                        writer.Write(t.Item1); writer.Write(t.Item2); writer.Write(t.Item3); writer.Write(t.Item4); break;
                    default:
                        throw new NotImplementedException($"无法将{objects[i].GetType().Name}导入CBuffer中");
                }
                // 需要对齐到16字节, 不能跨越
                // 如果当前变量的末尾有一部分外露到下一个16字节区间，则改为对齐下一个区间，重新写入
                if (((int)writer.BaseStream.Position - 1) / 16 > start / 16)
                {
                    writer.BaseStream.Seek((start / 16 + 1) * 16, SeekOrigin.Begin);
                    // 重新写入当前变量
                    i--;
                }
            }
            _bufferDirty = true;
            return (int)writer.BaseStream.Position;
        }

        /// <summary>
        /// Batch式CBuffer数据导入，宝宝模式
        /// </summary>
        /// <param name="startPosition"></param>
        public void DataBatchBegin(int startPosition = 0)
        {
            _bufferBegin = startPosition;
            D3dDeviceContext.ComputeShader.Set(D3dComputeShader);
        }

        public void DataBatchAdd(object item)
        {
            _bufferBegin = SetBufferData(_bufferBegin, item);
        }

        public void DataBatchEnd(bool flush = true)
        {
            if (_bufferBegin == -1)
                throw new Exception("DataBatch未Begin先End了");
            _bufferBegin = -1;
            if (flush)
                UpdateCBuffer();
        }

        public void Dispatch(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
        {
            lock (D3dDeviceContext)
            {
                D3dDeviceContext.ComputeShader.Set(D3dComputeShader);
                D3dDeviceContext.ComputeShader.SetConstantBuffer(0, CBuffer);
                D3dDeviceContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
            }
        }
        public void SetTexture(int slot, Texture texture)
        {
            if (texture == null)
            {
                D3dDeviceContext.ComputeShader.SetShaderResource(slot, null);
                return;
            }
            D3dDeviceContext.ComputeShader.SetShaderResource(slot, texture.GetType()
                .GetMethod("GetShaderResourceView", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(texture, null) as ShaderResourceView);
        }
        public void SetUnorderedTexture(int slot, UnorderedAccessTexture2D texture)
        {
            if (texture == null)
            {
                D3dDeviceContext.ComputeShader.SetUnorderedAccessView(slot, null);
                return;
            }
            UnorderedAccessView uav = texture.GetUAV(D3dDevice);
            D3dDeviceContext.ComputeShader.SetUnorderedAccessView(slot, uav);
        }
        public void Dispose()
        {
            if (CBuffer != null)
                CBuffer.Dispose();
            D3dComputeShader.Dispose();
        }
    }
}