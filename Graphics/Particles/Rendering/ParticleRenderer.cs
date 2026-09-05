using Particle.Core;

namespace Particle.Rendering
{
  /// <summary>四边形角点顶点 (实例化绘制的流 0 模板).</summary>
  public struct ParticleQuadVertex
  {
    /// <summary>角点坐标 (±0.5; 拉伸模式下 X 沿运动方向).</summary>
    public Vector2 Corner;
    /// <summary>角点纹理坐标.</summary>
    public Vector2 TexCoord;

    /// <summary>顶点声明 (POSITION0 + TEXCOORD0).</summary>
    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
      new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
      new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
  }

  /// <summary>
  /// 粒子渲染器 (纹理粒子版): 将更新策略产出的"数据纹理"按槽位 ID 实例化绘制.
  /// <br>实例顶点流只携带槽位 ID (静态缓冲), 粒子数据一律由顶点着色器从数据纹理取样 ——
  /// 无论更新策略是零拷贝共享纹理、读回还是 CPU 回退, 渲染路径完全一致.</br>
  /// <br>渲染后端可替换 —— 换用别的 Effect 或绘制方式只需重写本类 (渲染模块与核心解耦).</br>
  /// </summary>
  public sealed class ParticleRenderer
  {
    /// <summary>共享渲染器实例 (由 ParticleManager 创建).</summary>
    public static ParticleRenderer Shared { get; internal set; }

    private readonly GraphicsDevice _device;
    private readonly Effect _effect;
    private readonly VertexBuffer _quadBuffer;
    private readonly IndexBuffer _quadIndices;
    private VertexBuffer _idBuffer;
    private readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

    /// <summary>混合模式映射.</summary>
    private static readonly Dictionary<ParticleBlendMode, BlendState> BlendStates = new Dictionary<ParticleBlendMode, BlendState>
    {
      [ParticleBlendMode.Additive] = BlendState.Additive,
      [ParticleBlendMode.AlphaBlend] = BlendState.AlphaBlend,
      [ParticleBlendMode.NonPremultiplied] = BlendState.NonPremultiplied,
      [ParticleBlendMode.Opaque] = BlendState.Opaque
    };

    /// <summary>渲染的粒子数累计 (诊断).</summary>
    public long TotalDrawnParticles { get; private set; }

    /// <summary>渲染调用次数累计 (诊断).</summary>
    public long TotalDrawCalls { get; private set; }

    public ParticleRenderer(GraphicsDevice device)
    {
      _device = device;
      Shared = this;   // 引擎挂钩与刀光渲染器经此解析纹理/绘制.

      _effect = ParticleRenderEffect.GetOrCreate(device);

      // —— 四边形角点模板 (三角带 4 顶点) ——
      ParticleQuadVertex[] corners = new ParticleQuadVertex[4];
      corners[0].Corner = new Vector2(-0.5f, -0.5f);
      corners[0].TexCoord = new Vector2(0f, 0f);
      corners[1].Corner = new Vector2(0.5f, -0.5f);
      corners[1].TexCoord = new Vector2(1f, 0f);
      corners[2].Corner = new Vector2(-0.5f, 0.5f);
      corners[2].TexCoord = new Vector2(0f, 1f);
      corners[3].Corner = new Vector2(0.5f, 0.5f);
      corners[3].TexCoord = new Vector2(1f, 1f);
      _quadBuffer = new VertexBuffer(device, ParticleQuadVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
      _quadBuffer.SetData(corners);

      // —— 三角带索引 (实例化绘制必须绑定索引缓冲) ——
      _quadIndices = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, 4, BufferUsage.WriteOnly);
      _quadIndices.SetData(new[] { 0, 1, 2, 3 });
    }

    /// <summary>
    /// 绘制一批粒子.
    /// </summary>
    /// <param name="dataTexture">粒子数据纹理 (由更新策略提供, 宽 = 容量, 高 = 5 行).</param>
    /// <param name="instanceCount">实例数量 (含死亡槽位, 由着色器退化).</param>
    /// <param name="config">渲染配置 (混合模式/纹理/拉伸参数).</param>
    /// <param name="transform">相机变换 (世界 → 裁剪空间).</param>
    public void Draw(Texture2D dataTexture, int instanceCount, RenderConfig config, Matrix transform)
    {
      if (dataTexture is null || instanceCount <= 0)
        return;

      EnsureIdBuffer(instanceCount);

      BlendState blend = BlendStates.TryGetValue(config.Blend, out BlendState state) ? state : BlendState.Additive;
      Texture2D texture = ResolveTexture(config.Texture);

      _device.BlendState = blend;
      _device.RasterizerState = RasterizerState.CullNone;
      _device.DepthStencilState = DepthStencilState.None;
      _device.SamplerStates[0] = SamplerState.LinearClamp;
      _device.SamplerStates[1] = SamplerState.PointClamp;

      _device.SetVertexBuffers(
        new VertexBufferBinding(_quadBuffer),
        new VertexBufferBinding(_idBuffer, 0, 1));
      _device.Indices = _quadIndices;
      _device.Textures[0] = texture;
      _device.Textures[1] = dataTexture;
      _device.VertexTextures[1] = dataTexture;

      EffectParameterCollection parameters = _effect.Parameters;
      parameters["Transform"].SetValue(transform);
      parameters["Capacity"].SetValue((float)_idBuffer.VertexCount);
      parameters["StretchFactor"].SetValue(config.StretchFactor);
      parameters["MaxStretchLength"].SetValue(config.MaxStretchLength);
      parameters["SpriteTexture"]?.SetValue(texture);
      parameters["DataTexture"]?.SetValue(dataTexture);

      _effect.CurrentTechnique.Passes[0].Apply();
      _device.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2, instanceCount);

      TotalDrawnParticles += instanceCount;
      TotalDrawCalls++;
    }

    /// <summary>槽位 ID 实例缓冲 (静态内容, 按需扩容).</summary>
    private void EnsureIdBuffer(int instanceCount)
    {
      if (_idBuffer is not null && _idBuffer.VertexCount >= instanceCount)
        return;

      int capacity = Math.Max(256, Math.Max(instanceCount, _idBuffer?.VertexCount * 2 ?? 0));
      float[] ids = new float[capacity];
      for (int i = 0; i < capacity; i++)
        ids[i] = i;

      _idBuffer?.Dispose();
      _idBuffer = new VertexBuffer(_device, ParticleLayouts.IdInstanceVertexDeclaration, capacity, BufferUsage.WriteOnly);
      _idBuffer.SetData(ids);
    }

    /// <summary>
    /// 解析纹理: 内置程序化纹理名 ("white"/"glow"/"blade"/"spark"/"smoke") 或游戏资产路径.
    /// </summary>
    public Texture2D ResolveTexture(string name)
    {
      if (string.IsNullOrEmpty(name))
        name = "white";

      if (_textures.TryGetValue(name, out Texture2D cached))
        return cached;

      Texture2D texture = ParticleTextureFactory.Create(_device, name);
      if (texture is null)
        texture = ParticleTextureFactory.Create(_device, "white");
      _textures[name] = texture;
      return texture;
    }

    /// <summary>注册外部纹理 (编辑器/游戏可注入自定义刀光贴图).</summary>
    public void RegisterTexture(string name, Texture2D texture)
    {
      _textures[name] = texture;
    }

    /// <summary>释放渲染器持有的资源.</summary>
    public void Dispose()
    {
      _quadBuffer?.Dispose();
      _quadIndices?.Dispose();
      _idBuffer?.Dispose();
      _effect?.Dispose();
      foreach (Texture2D texture in _textures.Values)
        texture?.Dispose();
      _textures.Clear();
      if (Shared == this)
        Shared = null;
    }
  }
}
