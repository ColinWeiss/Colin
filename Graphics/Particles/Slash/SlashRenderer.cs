namespace Particle.Slash
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

    /// <summary>获取/创建共享渲染器 (图形设备取自粒子管理器, 需已初始化).</summary>
    public static SlashRenderer GetOrCreate()
    {
      if (Shared is null)
        Shared = new SlashRenderer(Core.ParticleManager.Instance.GraphicsDevice);
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
      for (int i = _trails.Count - 1; i >= 0; i--)
      {
        _trails[i].Update(dt);
        if (_trails[i].IsEmpty)
          _trails.RemoveAt(i);
      }
    }

    /// <summary>绘制全部活跃刀光 (可在不同相机下多次调用).</summary>
    public void DrawAll(Matrix transform)
    {
      if (_arcs.Count == 0 && _trails.Count == 0)
        return;

      _device.BlendState = BlendState.Additive;
      _device.RasterizerState = RasterizerState.CullNone;
      _device.DepthStencilState = DepthStencilState.None;
      _device.SamplerStates[0] = SamplerState.LinearClamp;
      _effect.Parameters["Transform"].SetValue(transform);

      foreach (SlashArc arc in _arcs)
        DrawMesh(arc.Config.Segments + 2, arc.Config.Texture, arc.Build);
      foreach (SlashTrail trail in _trails)
        DrawMesh(64, trail.Texture, trail.Build);
    }

    /// <summary>绘制单个弧形刀光 (编辑器预览用, 不受实例列表影响).</summary>
    public void DrawOne(SlashArc arc, Matrix transform)
    {
      _device.BlendState = BlendState.Additive;
      _device.RasterizerState = RasterizerState.CullNone;
      _device.DepthStencilState = DepthStencilState.None;
      _device.SamplerStates[0] = SamplerState.LinearClamp;
      _effect.Parameters["Transform"].SetValue(transform);
      DrawMesh(arc.Config.Segments + 2, arc.Config.Texture, arc.Build);
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

    private void DrawMesh(int pointCount, string textureName, Func<SlashVertex[], short[], int> build)
    {
      EnsureCapacity(pointCount);

      int segments = build(_vertices, _indices);
      if (segments <= 0)
        return;

      Texture2D texture = Particle.Rendering.ParticleRenderer.Shared?.ResolveTexture(textureName)
        ?? Particle.Rendering.ParticleTextureFactory.Create(_device, textureName);
      _effect.Parameters["SpriteTexture"]?.SetValue(texture);
      _effect.CurrentTechnique.Passes[0].Apply();

      int vertexCount = (segments + 1) * 2;
      _device.DrawUserIndexedPrimitives(
        PrimitiveType.TriangleList, _vertices, 0, vertexCount, _indices, 0, segments * 2);
    }
  }
}
