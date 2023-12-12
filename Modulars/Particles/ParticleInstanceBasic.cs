using Colin.Core.ModLoaders;
using Colin.Core.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Particles
{
    public class ParticleInstanceBasic : ICodeResource
    {
        public virtual string ParticleShaderPath => "Effects/xxx";
        public virtual string BehaviorShaderPath => "Effects/xxx";
        public virtual string ParticleTexturePath => "Textures/xxx";
        public virtual int ComputeTextureHeight => 2;   // 每个像素是两个float，增大纹理高度可以存储更多数据

        public virtual int MaxCount => 10000;

        public Effect ParticleShader; // 绘制粒子Pass和拷贝Pass
        public Effect BehaviorShader; // 粒子行为计算Pass和初始化Pass

        public Texture2D ParticleTexture; // 粒子图形纹理

        // 记录每个粒子数据的纹理.
        // 含红绿通道代表 x 和 y 轴, 两行分别代表位置和速度.
        public RenderTarget2D ComputeRT;
        public RenderTarget2D ComputeRTSwap;

        /// <summary>
        /// 单个例子的四个顶点.
        /// </summary>
        public ParticleVertexInfo[] SingleParticleVertex = new ParticleVertexInfo[]
            {
                new ParticleVertexInfo(new Vector3(-1, -1, 0), new Vector2(0, 0)),
                new ParticleVertexInfo(new Vector3(-1, +1, 0), new Vector2(0, 1)),
                new ParticleVertexInfo(new Vector3(+1, -1, 0), new Vector2(1, 0)),
                new ParticleVertexInfo(new Vector3(+1, +1, 0), new Vector2(1, 1))
            };

        public InstanceInfo[] ParticleInstance; // 粒子实例列表，包含N个点

        /// <summary>
        /// 矩形四个顶点.
        /// </summary>
        public ParticleVertexInfo[] QuadVertex = new ParticleVertexInfo[]
            {
                new ParticleVertexInfo(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                new ParticleVertexInfo(new Vector3(-1, +1, 0), new Vector2(0, 0)),
                new ParticleVertexInfo(new Vector3(+1, -1, 0), new Vector2(1, 1)),
                new ParticleVertexInfo(new Vector3(+1, +1, 0), new Vector2(1, 0))
            };

        /// <summary>
        /// 单个粒子顶点缓冲, 长度4.
        /// </summary>
        public VertexBuffer SingleParticleVBO = null;

        /// <summary>
        /// 实例粒子顶点缓冲区.
        /// </summary>
        public VertexBuffer ParticleVBO = null;

        /// <summary>
        /// 顶点绘制顺序索引缓冲区.
        /// </summary>
        public IndexBuffer SingleParticleEBO = null;

        /// <summary>
        /// 实例绑定缓冲区.
        /// </summary>
        public VertexBufferBinding[] ParticleBinding = null;

        public virtual void DoInitialize(GraphicsDevice graphicsDevice)
        {
            ParticleShader = EffectAssets.Get(ParticleShaderPath);
            BehaviorShader = EffectAssets.Get(BehaviorShaderPath);
            ParticleTexture = TextureAssets.Get(ParticleTexturePath);
            // 初始化顶点数据
            List<InstanceInfo> particleInstance;
            // 实例Buffer中需要放入不同的东西来区分每个实例
            // 这里会把实例的id映射到0-1的浮点数，在shader通过这个值来索引
            particleInstance = new();
            for (int i = 0; i < MaxCount; i++)
            {
                particleInstance.Add(new InstanceInfo(i / (float)MaxCount));
            }

            ParticleInstance = particleInstance.ToArray();
        }

        public void SetupRenderTargets(GraphicsDevice graphicsDevice)
        {
            // 初始化单个粒子的顶点缓冲区.
            VertexBuffer vbo;
            vbo = new VertexBuffer(graphicsDevice, ParticleVertexInfo._VertexDeclaration, 4, BufferUsage.WriteOnly);
            vbo.SetData(SingleParticleVertex);
            SingleParticleVBO = vbo;

            // 初始化实例粒子们的顶点缓冲区.
            vbo = new VertexBuffer(graphicsDevice, InstanceInfo._VertexDeclaration, MaxCount, BufferUsage.WriteOnly);
            vbo.SetData(ParticleInstance);
            ParticleVBO = vbo;

            // 绑定单个粒子的四个顶点和实例粒子顶点缓冲区以用于实例绘制.
            ParticleBinding = new VertexBufferBinding[] {
                new VertexBufferBinding(SingleParticleVBO),
                new VertexBufferBinding(ParticleVBO, 0, 1)
            };

            // 指示顶点顺序.
            SingleParticleEBO = new IndexBuffer(
                graphicsDevice,
                IndexElementSize.ThirtyTwoBits,
                4, BufferUsage.WriteOnly);
            SingleParticleEBO.SetData(new int[] { 0, 1, 2, 3 });

            // 注意 SurfaceFormat.
            // 我们需要用单通道表示一个 float.
            // 那么需要的精度就是每个通道 32 位, 而不是平时用的 8 位 (0-255).
            // 总大小是 2*32=64 位 (平时用的RGBA是 8*4=32 位).
            ComputeRT = new RenderTarget2D(
                graphicsDevice,
                MaxCount,
                ComputeTextureHeight, false,
                SurfaceFormat.Vector2, DepthFormat.Depth16,
                0, RenderTargetUsage.PreserveContents);

            ComputeRTSwap = new RenderTarget2D(
                graphicsDevice,
                MaxCount,
                ComputeTextureHeight, false,
                SurfaceFormat.Vector2, DepthFormat.Depth16,
                0, RenderTargetUsage.PreserveContents);
        }

        /// <summary>
        /// 此处命名尊重原著, 并相比 "DoUpdate", 这个命名在描述作用上更加严谨.
        /// <br>进行粒子逻辑计算.</br>
        /// </summary>
        public virtual void DoCompute(GraphicsDevice graphicsDevice) { }

        /// <summary>
        /// 执行渲染.
        /// </summary>
        public virtual void DoRender(GraphicsDevice graphicsDevice) { }

        public struct ParticleVertexInfo : IVertexType
        {
            public static readonly VertexDeclaration _VertexDeclaration = new VertexDeclaration(new VertexElement[2]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(3 * sizeof(float), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            });
            public Vector3 position;
            public Vector2 texCoord;

            public ParticleVertexInfo(Vector3 pos, Vector2 uv)
            {
                position = pos;
                texCoord = uv;
            }

            public VertexDeclaration VertexDeclaration { get => _VertexDeclaration; }
        }
        public struct InstanceInfo : IVertexType
        {
            public static readonly VertexDeclaration _VertexDeclaration = new VertexDeclaration(new VertexElement[]
            {
                new VertexElement(0 * sizeof(float), VertexElementFormat.Single, VertexElementUsage.Position, 0)
            });
            public float texCoord;

            public InstanceInfo(float u)
            {
                texCoord = u;
            }

            public VertexDeclaration VertexDeclaration { get => _VertexDeclaration; }
        }
    }
}