using Colin.Core.Graphics.Shaders;
using DeltaMachine.Core.Common;

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

        public void DoInitialize(Texture2D texture)
        {
            Effect = EffectAssets.Get("Instancing");
            Texture = texture;
            CreateTemplate();
            GenerateInstanceInformation(10000);
            Bindings = new VertexBufferBinding[2];
            Bindings[0] = new VertexBufferBinding(TemplateBuffer);
            Bindings[1] = new VertexBufferBinding(ParticleBuffer, 0, 1);
        }

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
            // TODO: 这里创建实例信息数组后
            // 绑定到 ParticleDatasBuffer.
            // 然后执行实例绘制将数据写入粒子信息 Rt 上.
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
            TemplateIndexBuffer = new IndexBuffer(Device, IndexElementSize.ThirtyTwoBits, 4, BufferUsage.WriteOnly);
            TemplateIndexBuffer.SetData(indices);
        }

        public void GenerateInstanceInformation(int count)
        {
            ParticleInfos = new ParticleInfo[count];
            Random rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                ParticleInfos[i].Position.X = rnd.Next(0, 1280);
                ParticleInfos[i].Position.Y = rnd.Next(0, 720);
            }
            ParticleBuffer = new VertexBuffer(Device, ParticleInfo.VertexDeclaration, count, BufferUsage.WriteOnly);
            ParticleBuffer.SetData(ParticleInfos);
        }
        public void DoRender(SceneCamera camera)
        {
            Perfmon.Start();

            Device.SamplerStates[0] = SamplerState.PointClamp;
            Device.RasterizerState = RasterizerState.CullNone;
            Device.Indices = TemplateIndexBuffer;
            Device.SetVertexBuffers(Bindings);

            Device.Textures[0] = Texture;

            Effect.Parameters["Transform"].SetValue(camera.Transform);
            Effect.Parameters["SpriteTexture"].SetValue(Texture);
            Effect.CurrentTechnique.Passes["P0"].Apply();

            Device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2, 10000);
            Perfmon.End("实例绘制");

        }
    }
}