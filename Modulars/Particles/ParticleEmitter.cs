using Colin.Core.Graphics.Shaders;
using SharpDX;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace Colin.Core.Modulars.Particles
{
    /// <summary>
    /// 粒子发射器.
    /// </summary>
    public class ParticleEmitter
    {
        public readonly int ParticleCountMax;
        public ParticleEmitter(int particleCountMax) => ParticleCountMax = particleCountMax;

        /// <summary>
        /// 图形设备.
        /// </summary>
        public GraphicsDevice Device => EngineInfo.Graphics.GraphicsDevice;
        /// <summary>
        /// 数据像素模板的顶点缓冲区.
        /// </summary>
        public VertexBuffer TemplateDataVertexBuffer;
        /// <summary>
        /// 数据像素的顶点索引缓冲区.
        /// </summary>
        public IndexBuffer TemplateDataIndexBuffer;
        /// <summary>
        /// 数据像素顶点缓冲区.
        /// </summary>
        public VertexBuffer DataVertexBuffer;
        /// <summary>
        /// 数据绑定.
        /// </summary>
        public VertexBufferBinding[] DataBindings;
        /// <summary>
        /// 用于粒子数据的写入着色器.
        /// </summary>
        public Effect ParticleDataStream;
        /// <summary>
        /// 粒子数据数组缓冲区队列.
        /// <br>用于 <see cref="DataWriteStep"/>.</br>
        /// </summary>
        public Queue<ParticleData[]> DataBufferQueue = new Queue<ParticleData[]>();

        /// <summary>
        /// 用于存储粒子信息的渲染目标.
        /// <br>交由 ComputeShader 完成行为计算.</br>
        /// </summary>
        public UnorderedAccessTexture2D DataRt;
        /// <summary>
        /// 用于存储粒子信息的渲染目标.
        /// <br>该 Rt 为经过 ComputeShader 计算后的数据结果.</br>
        /// </summary>
        public UnorderedAccessTexture2D DataResultRt;
        /// <summary>
        /// 指示当前于 <see cref="DataRt"/> 的写入位置.
        /// </summary>
        public int WritePointer = 0;

        /// <summary>
        /// 粒子通用行为逻辑.
        /// <br>在此处进行位置、缩放、旋转的更新.</br>
        /// </summary>
        public ComputeShader ParticleUpdateCompute;

        public Effect ParticleInstancing;

        /// <summary>
        /// 初始化步骤.
        /// <br>在这一步, 将会创建一张 <see cref="RenderTarget2D"/> 用于存储粒子数据.</br>
        /// <br>SurfaceFormat 为 <see cref="SurfaceFormat.Vector4"/>.</br>
        /// <br>渲染目标的宽度为支持的粒子上限大小.</br>
        /// <br>渲染目标的高度为 4.</br>
        /// <para>
        /// 此处为渲染目标的行和列对应的存储数据:
        /// <br>[1] 第一行存储粒子颜色.</br>
        /// <br>[2] R, G 存储粒子位置, B, A 存储粒子速度.</br>
        /// <br>[3] R, G 存储粒子缩放, B, A 存储粒子缩放速度.</br>
        /// <br>[4] R 存储粒子旋转, G 存储粒子旋转速度, B 存储粒子生命周期, A 表示粒子是否活跃.</br>
        /// </para>
        /// </summary>
        public void DoInitialize()
        {
            ParticleDataStream = EffectAssets.Get("Particles/ParticleDataStream");
            ParticleUpdateCompute = ShaderAssets.Get("ParticleUpdate");
            ParticleInstancing = EffectAssets.Get("Particles/ParticleInstancing");
            CreateParticleInfoBuffer();
            DataBindings = new VertexBufferBinding[2];
            DataBindings[0] = new VertexBufferBinding(TemplateDataVertexBuffer);
        }
        private void CreateParticleInfoBuffer()
        {
            DataRt = new UnorderedAccessTexture2D(
            EngineInfo.Graphics.GraphicsDevice,
            ParticleCountMax,
            4,
            false,
            SurfaceFormat.Vector4,
            DepthFormat.None,
            0,
            RenderTargetUsage.PreserveContents);

            DataResultRt = new UnorderedAccessTexture2D(
            EngineInfo.Graphics.GraphicsDevice,
            ParticleCountMax,
            4,
            false,
            SurfaceFormat.Vector4,
            DepthFormat.None,
            0,
            RenderTargetUsage.PreserveContents);

            VertexPositionTexture[] vertices = new VertexPositionTexture[4];
            vertices[0].Position = new Vector3(0, 0, 0);
            vertices[0].TextureCoordinate = new Vector2(0, 0);
            vertices[1].Position = new Vector3(0, 4, 0);
            vertices[1].TextureCoordinate = new Vector2(0, 1);
            vertices[2].Position = new Vector3(1, 0, 0);
            vertices[2].TextureCoordinate = new Vector2(1, 0);
            vertices[3].Position = new Vector3(1, 4, 0);
            vertices[3].TextureCoordinate = new Vector2(1, 1);
            TemplateDataVertexBuffer = new VertexBuffer(Device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            TemplateDataVertexBuffer.SetData(vertices);
            int[] indices = new int[4]
            {
                0, 1, 2, 3
            };
            TemplateDataIndexBuffer = new IndexBuffer(Device, IndexElementSize.ThirtyTwoBits, 4, BufferUsage.WriteOnly);
            TemplateDataIndexBuffer.SetData(indices);
        }

        public void DoRender()
        {
            DataWriteStep();
            DoCompute();

        }
        /// <summary>
        /// 在这一步, 发射器将读取 <see cref="DataBufferQueue"/> 的数据, 并将其全部写入 <see cref="DataRt"/>.
        /// </summary>
        public void DataWriteStep()
        {
            EngineInfo.Graphics.GraphicsDevice.SetRenderTarget(DataRt);
            Matrix matrix = Matrix.CreateOrthographicOffCenter(EngineInfo.ViewRectangle, 0, 100);
            while (DataBufferQueue.Count > 0)
            {
                ParticleData[] datas = DataBufferQueue.Dequeue();
                DataVertexBuffer = new VertexBuffer(Device, ParticleData.VertexDeclaration, datas.Length, BufferUsage.WriteOnly);
                DataBindings[1] = new VertexBufferBinding(DataVertexBuffer, 0, 1);
                Device.SamplerStates[0] = SamplerState.PointClamp;
                Device.RasterizerState = RasterizerState.CullNone;
                Device.Indices = TemplateDataIndexBuffer;
                Device.SetVertexBuffers(DataBindings);
                DataVertexBuffer.SetData(datas);
                ParticleDataStream.Parameters["Transform"].SetValue(matrix);
                ParticleDataStream.CurrentTechnique.Passes["P0"].Apply();
                Device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2, datas.Length);
            }
        }
        /// <summary>
        /// 使用 <see cref="ComputeShader"/> 对粒子执行行为和数据的更新.
        /// <br>在这一步, 允许更换的计算着色器用于更新粒子的行为.</br>
        /// </summary>
        public void DoCompute()
        {
            ParticleUpdateCompute.SetTexture(0, DataRt);
            ParticleUpdateCompute.SetUnorderedTexture(0, DataResultRt);
            ParticleUpdateCompute.Dispatch(128, 128, 1);
        }
        /// <summary>
        /// 使用 ParticleInstancing 对粒子进行实例绘制.
        /// </summary>
        public void DoParticleRender()
        {

        }
        public void NewParticle( ParticleData[] datas )
        {
            if (WritePointer + datas.Length < ParticleCountMax)
                WritePointer += datas.Length;
            else
            {
                WritePointer = 0;
                WritePointer += datas.Length;
            }
            for (int count = 0; count < datas.Length; count++)
            {
                datas[count].ID = WritePointer + count - 1;
            }
            DataBufferQueue.Enqueue(datas);
        }
    }
}