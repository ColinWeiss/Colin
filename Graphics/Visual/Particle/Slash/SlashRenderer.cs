namespace Colin.Core.Graphics.Visual.Particle.Slash
{
  /// <summary>
  /// 刀光渲染器 (静态共享): 管理所有活跃的 <see cref="SlashArc"/> / <see cref="SlashTrail"/>,
  /// 每帧推进动画并把条带 Mesh 绘制到当前渲染目标 (加法混合, DrawUserIndexedPrimitives).
  /// </summary>
  public sealed class SlashRenderer
  {
    /// <summary>共享渲染器实例 (首次访问时创建).</summary>
    public static SlashRenderer Shared { get; private set; }

    private readonly GraphicsDevice _device;
    private readonly Effect _effect;
    private readonly List<SlashArc> _arcs = new List<SlashArc>();
    private readonly List<SlashTrail> _trails = new List<SlashTrail>();

    private SlashVertex[] _vertices = new SlashVertex[512];
    private short[] _indices = new short[1536];

    /// <summary>当前活跃刀光数 (诊断).</summary>
    public int ActiveCount => _arcs.Count + _trails.Count;

    private SlashRenderer(GraphicsDevice device)
    {
      _device = device;
      _effect = SlashRenderEffect.GetOrCreate(device);
    }

    /// <summary>获取/创建共享渲染器 (图形设备取自粒子管理器; 未初始化时直接用引擎设备).</summary>
    public static SlashRenderer GetOrCreate()
    {
      if (Shared is null)
      {
        GraphicsDevice device = ParticleManager.Instance.IsInitialized
          ? ParticleManager.Instance.GraphicsDevice
          : CoreInfo.Graphics.GraphicsDevice;
        Shared = new SlashRenderer(device);
      }
      return Shared;
    }

    /// <summary>获取共享渲染器 (未初始化时返回 null).</summary>
    public static SlashRenderer GetOrDefault() => Shared;

    internal void Register(SlashArc arc)
    {
      if (!_arcs.Contains(arc))
        _arcs.Add(arc);
    }

    internal void Register(SlashTrail trail)
    {
      if (!_trails.Contains(trail))
        _trails.Add(trail);
    }

    /// <summary>移除实例.</summary>
    public void Remove(SlashArc arc) => _arcs.Remove(arc);

    /// <summary>移除实例.</summary>
    public void Remove(SlashTrail trail) => _trails.Remove(trail);

    /// <summary>推进全部刀光动画 (每帧一次, 引擎挂钩或编辑器调用).</summary>
    public void TickAll(float dt)
    {
      for (int i = _arcs.Count - 1; i >= 0; i--)
      {
        _arcs[i].Update(dt);
        if (_arcs[i].IsFinished)
          _arcs.RemoveAt(i);
      }
      // 轨迹不自动移除: 挥砍间歇会短暂少于 2 点, 移除会让仍被持有的实例永远脱离渲染;
      // 由持有方调用 Remove 显式回收.
      for (int i = _trails.Count - 1; i >= 0; i--)
        _trails[i].Update(dt);
    }

    /// <summary>绘制全部活跃刀光 (可在不同相机下多次调用).
    /// 弧形刀光顶点在模型空间生成, 经自身"相机矩阵"成像; 轨迹点为世界坐标, 用单位矩阵.</summary>
    public void DrawAll(Matrix transform)
    {
      if (_arcs.Count == 0 && _trails.Count == 0)
        return;

      BeginPass();
      foreach (SlashArc arc in _arcs)
        DrawArc(arc, transform);
      foreach (SlashTrail trail in _trails)
        DrawTrail(trail, transform);
    }

    /// <summary>绘制单个弧形刀光 (编辑器预览用, 不受实例列表影响).</summary>
    public void DrawOne(SlashArc arc, Matrix transform)
    {
      BeginPass();
      DrawArc(arc, transform);
    }

    /// <summary>管线状态覆盖 (诊断/宿主管线适配): 非空时替代默认加法混合绘制条带 Mesh.</summary>
    public BlendState BlendOverride;

    /// <summary>统一的加法混合渲染状态.</summary>
    private void BeginPass()
    {
      _device.BlendState = BlendOverride ?? BlendState.Additive;
      _device.RasterizerState = RasterizerState.CullNone;
      _device.DepthStencilState = DepthStencilState.None;
      _device.SamplerStates[0] = SamplerState.LinearClamp;
    }

    /// <summary>
    /// 弧形刀光: 几何每帧只构建一次, 逐个启用的纹理层单独成 pass
    /// (各自纹理/色调/UV 平铺偏移滚动) —— 纹理层是刀光的叠加修饰器.
    /// </summary>
    private void DrawArc(SlashArc arc, Matrix camera)
    {
      int pointCount = arc.Config.Segments + 2;
      EnsureCapacity(pointCount);

      int segments = arc.Build(_vertices, _indices);
      if (segments <= 0)
        return;

      Matrix world = arc.TransformMatrix();
      List<SlashTextureLayer> layers = arc.Config.Layers;
      bool drewAny = false;
      if (layers is not null)
      {
        for (int i = 0; i < layers.Count; i++)
        {
          SlashTextureLayer layer = layers[i];
          if (layer is null || !layer.Enabled)
            continue;
          drewAny = true;
          bool wrap = layer.UTiling != 1f || layer.UOffset != 0f || layer.ScrollSpeed != 0f;
          Vector4 uvTransform = new Vector4(layer.UTiling, layer.UOffset + arc.GetLayerScroll(i), layer.VScale, layer.Intensity);
          DrawPass(segments, layer.Texture, uvTransform, layer.Tint, world, camera, wrap);
        }
      }

      // —— 无层配置: 回退到旧单纹理字段 (等价单层) ——
      if (!drewAny)
        DrawPass(segments, arc.Config.Texture, new Vector4(1f, 0f, 1f, 1f), Vector4.One, world, camera, wrap: false);
    }

    /// <summary>轨迹刀光 (世界坐标点列, 单纹理).</summary>
    private void DrawTrail(SlashTrail trail, Matrix camera)
    {
      EnsureCapacity(64);
      int segments = trail.Build(_vertices, _indices);
      if (segments <= 0)
        return;
      DrawPass(segments, trail.Texture, new Vector4(1f, 0f, 1f, 1f), Vector4.One, Matrix.Identity, camera, wrap: false);
    }

    private void EnsureCapacity(int pointCount)
    {
      int needed = pointCount * 2;
      if (_vertices.Length < needed)
      {
        _vertices = new SlashVertex[needed * 2];
        _indices = new short[needed * 6];
      }
    }

    /// <summary>以给定层参数绘制一段条带 Mesh.</summary>
    private void DrawPass(int segments, string textureName, Vector4 uvTransform, Vector4 tint, Matrix world, Matrix camera, bool wrap)
    {
      Texture2D texture = Colin.Core.Graphics.Visual.Particle.Rendering.ParticleRenderer.Shared?.ResolveTexture(textureName)
        ?? Colin.Core.Graphics.Visual.Particle.Rendering.ParticleTextureFactory.Create(_device, textureName)
        ?? Colin.Core.Graphics.Visual.Particle.Rendering.ParticleTextureFactory.Create(_device, "white");
      _effect.Parameters["SpriteTexture"]?.SetValue(texture);
      _effect.Parameters["UvTransform"]?.SetValue(uvTransform);   // x=U平铺, y=U偏移, z=V缩放, w=层强度.
      _effect.Parameters["LayerTint"]?.SetValue(tint);
      _device.SamplerStates[0] = wrap ? SamplerState.LinearWrap : SamplerState.LinearClamp;
      // 模型空间 → 世界 (弧自身的相机矩阵) → 裁剪空间 (外部相机).
      _effect.Parameters["Transform"]?.SetValue(world * camera);
      _effect.CurrentTechnique.Passes[0].Apply();

      int vertexCount = (segments + 1) * 2;
      _device.DrawUserIndexedPrimitives(
        PrimitiveType.TriangleList, _vertices, 0, vertexCount, _indices, 0, segments * 2);
    }
  }
}
