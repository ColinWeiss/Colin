using SharpDX;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace Colin.Core.Modulars.Particles
{
    /// <summary>
    /// 粒子发射器.
    /// </summary>
    public class ParticleEmitter
    {
        /// <summary>
        /// 粒子的缓冲区.
        /// </summary>
        public VertexBuffer ParticleBuffer;

        /// <summary>
        /// 模板粒子缓冲区.
        /// </summary>
        public VertexBuffer TemplateBuffer;

        /// <summary>
        /// 模板顶点索引缓冲区.
        /// </summary>
        public IndexBuffer TemplateIndexBuffer;

        /// <summary>
        /// 实例绑定.
        /// </summary>
        public VertexBufferBinding[] Bindings;

        /// <summary>
        /// 粒子信息.
        /// </summary>
        public ParticleInfo[] ParticleInfos;

        public VertexDeclaration InstanceVertexDeclaration;

        public GraphicsDevice Device => EngineInfo.Graphics.GraphicsDevice;

        public Texture2D Texture;

        public Effect Effect;

        public readonly int ParticleCountMax;
        public ParticleEmitter(int particleCountMax)
        {
            ParticleCountMax = particleCountMax;
        }

        /// <summary>
        /// 用于存储粒子信息的渲染目标.
        /// <br>交由 ComputeShader 完成行为计算.</br>
        /// </summary>
        public RenderTarget2D ParticleInfosBuffer;

        public Texture2D Sampler;

        public int WritePointer = 0;

        /*
         
            在C#部分, 我将使用一个实例绘制，在该Rt上进行粒子信息的写入
            其中，顶点着色器将接受
            本帧传输的粒子颜色、位置、速度、缩放、缩放速度、旋转、旋转速度、生命周期和是否活跃。
            以及，本帧传输的粒子数量。
            Csharp会记录本次传输的粒子数量，把它加到一个int上。
            在下次使用时，传输数据的起点将会以该int进行偏移。

        public int WritePointer = 0;
        public void NewParticle( 
            Color[] colors, 
            Vector2[] position, Vector2[] velocity, 
            Vector2[] scale, Vector2[] scaleVel,
            float[] rotation, float[] rotationVel,
            float activeTime, int active )
        {
            ParticleDataStream = EffectAssets.Get("Particles/ParticleDataStream");
            //  ParticleDataStream.Parameters["ParticleCountMax"].SetValue(ParticleCountMax);
            CreateParticleInfoBuffer();
            DataPixelBindings = new VertexBufferBinding[2];
            DataPixelBindings[0] = new VertexBufferBinding(TemplateDataPixelVertexBuffer);

            Sampler = TextureExt.Create(1, 4);
            Sampler.SetData(new Color[] { Color.White, Color.White, Color.White, Color.White });
        }
         */

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
            ParticleInfosBuffer = new RenderTarget2D(
            EngineInfo.Graphics.GraphicsDevice,
            ParticleCountMax,
            4,
            false,
            SurfaceFormat.Vector4,
            DepthFormat.None,
            0,
            RenderTargetUsage.PreserveContents);
        }
        /// <summary>
        /// 在这一步, 将为粒子模板创建顶点缓冲区和索引缓冲区.
        /// </summary>
        public void CreateTemplate()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];
            vertices[0].Position = new Vector3(0, 0, 0);
            vertices[0].TextureCoordinate = new Vector2(0, 0);
            vertices[1].Position = new Vector3(0, Texture.Height, 0);
            vertices[1].TextureCoordinate = new Vector2(1, 0);
            vertices[2].Position = new Vector3(Texture.Width, 0, 0);
            vertices[2].TextureCoordinate = new Vector2(0, 1);
            vertices[3].Position = new Vector3(Texture.Width, Texture.Height, 0);
            vertices[3].TextureCoordinate = new Vector2(1, 1);
            TemplateBuffer = new VertexBuffer(Device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            TemplateBuffer.SetData(vertices);
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
            EngineInfo.Graphics.GraphicsDevice.SetRenderTarget(DataPixelRt);
            EngineInfo.SpriteBatch.Begin();
            while (DataBuffer.Count > 0)
            {
                Matrix mvp = Matrix.CreateOrthographicOffCenter(0, EngineInfo.ViewWidth, EngineInfo.ViewHeight , 0, 0, 100f);
                ParticleData[] datas = DataBuffer.Dequeue();
                DataPixelVertexBuffer = new VertexBuffer(Device, ParticleData.VertexDeclaration, datas.Length, BufferUsage.WriteOnly);
                DataPixelVertexBuffer.SetData(datas);
                DataPixelBindings[1] = new VertexBufferBinding(DataPixelVertexBuffer, 0, 1);

                Device.SamplerStates[0] = SamplerState.PointClamp;
                Device.RasterizerState = RasterizerState.CullNone;
                Device.Indices = TemplateDataPixelIndexBuffer;
                Device.SetVertexBuffers(DataPixelBindings);
                Device.Textures[0] = Sampler;
                ParticleDataStream.Parameters["Transform"].SetValue(mvp);
                ParticleDataStream.CurrentTechnique.Passes["P0"].Apply();
                Device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2, datas.Length);
            }
            EngineInfo.SpriteBatch.End();
        }

        public void NewParticle(ParticleData[] datas)
        {
            ParticleInfos = new ParticleInfo[count];
            Random rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                  datas[count].ID = WritePointer + count - 1;
            }
            ParticleBuffer = new VertexBuffer(Device, ParticleInfo.VertexDeclaration, count, BufferUsage.WriteOnly);
            ParticleBuffer.SetData(ParticleInfos);
        }
        public void DoRender(SceneCamera camera)
        {
        }
    }
}