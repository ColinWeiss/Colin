using SharpDX;

namespace Colin.Core.Modulars.Particles
{
    /// <summary>
    /// 粒子发射器.
    /// </summary>
    public class ParticleEmitter
    {
        public GraphicsDevice Device => EngineInfo.Graphics.GraphicsDevice;

        /// <summary>
        /// 数据像素模板的顶点缓冲区.
        /// </summary>
        public VertexBuffer TemplateDataPixelVertexBuffer;
        /// <summary>
        /// 数据像素的顶点索引缓冲区.
        /// </summary>
        public IndexBuffer TemplateDataPixelIndexBuffer;
        /// <summary>
        /// 数据像素顶点缓冲区.
        /// </summary>
        public VertexBuffer DataPixelVertexBuffer;
        /// <summary>
        /// 数据像素绑定.
        /// </summary>
        public VertexBufferBinding[] DataPixelBindings;
        /// <summary>
        /// 用于粒子数据像素的写入着色器.
        /// </summary>
        public Effect ParticleDataStream;

        public readonly int ParticleCountMax;
        public ParticleEmitter(int particleCountMax)
        {
            ParticleCountMax = particleCountMax;
        }

        /// <summary>
        /// 用于存储粒子信息的渲染目标.
        /// <br>交由 ComputeShader 完成行为计算.</br>
        /// </summary>
        public RenderTarget2D DataPixelRt;

        public int WritePointer = 0;

        public void DoInitialize()
        {
            ParticleDataStream = EffectAssets.Get("Particles/ParticleDataStream");
          //  ParticleDataStream.Parameters["ParticleCountMax"].SetValue(ParticleCountMax);
            CreateParticleInfoBuffer();
            DataPixelBindings = new VertexBufferBinding[2];
            DataPixelBindings[0] = new VertexBufferBinding(TemplateDataPixelVertexBuffer);
        }

        /// <summary>
        /// 在这一步, 将会创建一张 <see cref="RenderTarget2D"/> 用于存储粒子数据.
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
        public void CreateParticleInfoBuffer()
        {
            DataPixelRt = new RenderTarget2D(
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
            TemplateDataPixelVertexBuffer = new VertexBuffer(Device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            TemplateDataPixelVertexBuffer.SetData(vertices);
            int[] indices = new int[4]
            {
                0, 1, 2, 3
            };
            TemplateDataPixelIndexBuffer = new IndexBuffer(Device, IndexElementSize.ThirtyTwoBits, 4, BufferUsage.WriteOnly);
            TemplateDataPixelIndexBuffer.SetData(indices);

        }

        public Queue<ParticleData[]> DataBuffer = new Queue<ParticleData[]>();

        public void DoRender()
        {
            EngineInfo.Graphics.GraphicsDevice.SetRenderTarget( DataPixelRt );
            EngineInfo.SpriteBatch.Begin();
            while (DataBuffer.Count > 0)
            {
                ParticleData[] datas = DataBuffer.Dequeue();
                DataPixelVertexBuffer = new VertexBuffer(Device, ParticleData.VertexDeclaration, datas.Length, BufferUsage.WriteOnly);
                DataPixelBindings[1] = new VertexBufferBinding(DataPixelVertexBuffer, 0, 1);

                Device.SamplerStates[0] = SamplerState.PointClamp;
                Device.RasterizerState = RasterizerState.CullNone;
                Device.Indices = TemplateDataPixelIndexBuffer;
                Device.SetVertexBuffers(DataPixelBindings);
                DataPixelVertexBuffer.SetData(datas);
                ParticleDataStream.CurrentTechnique.Passes["P0"].Apply();
                Device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2, datas.Length );
            }
            EngineInfo.SpriteBatch.End();
        }

        public void NewParticle( ParticleData[] datas )
        {
            WritePointer += datas.Length;
            for (int count = 0; count < datas.Length; count++)
            {
                datas[count].ID = (WritePointer + count - 1) / ParticleCountMax;
            }
            DataBuffer.Enqueue(datas);
        }
        public void NewParticle(Color[] colors, Vector2[] positions, Vector2[] velocities, Vector2[] scales, Vector2[] scaleVels, float[] rotations, float[] rotationVels, float[] activeTimes, float[] actives)
        {
            ParticleData[] ParticleDatas = new ParticleData[colors.Length];
            for (int count = 0; count < ParticleDatas.Length; count++)
            {
                ParticleDatas[count].Color = colors[count];
                ParticleDatas[count].Position = positions[count];
                ParticleDatas[count].Velocity = velocities[count];
                ParticleDatas[count].Scale = scales[count];
                ParticleDatas[count].Rotation = rotations[count];
                ParticleDatas[count].ActiveTime = activeTimes[count];
                ParticleDatas[count].ID = WritePointer + count - 1;
            }
            WritePointer += colors.Length;
            DataBuffer.Enqueue(ParticleDatas);
        }
    }
}