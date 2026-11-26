namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// PBR 场景模块: 独占一张带深度缓冲的渲染目标, 画完由场景合成链呈现.
  /// <br>模块只负责 3D 通道; 相机/光照/模型由外部驱动, 便于演示场景或游戏逻辑接管.</br>
  /// </summary>
  public class PbrRenderModule : SceneModule, IRenderableISceneModule, IPbrRenderScene
  {
    private readonly List<PbrModel> _models = new List<PbrModel>();

    public RenderTarget2D RawRt { get; set; }

    public bool RawRtVisible { get; set; } = true;

    public bool Presentation { get; set; } = true;

    public Color ClearColor = new Color(16, 18, 24, 255);

    public PbrCamera Camera { get; } = new PbrCamera();

    public PbrLighting Lighting { get; set; } = PbrLighting.CreateDefault();

    public PbrRenderer Renderer { get; private set; }

    public bool DrawSkybox { get; set; }

    public IReadOnlyList<PbrModel> Models => _models;

    /// <summary>Dispose 时是否一并释放 <see cref="Models"/>. 模型由 Leemo 资产管线持有时应设为 false.</summary>
    public bool DisposeModels = true;

    public void Add(PbrModel model)
    {
      if (model is null)
        throw new ArgumentNullException(nameof(model));
      _models.Add(model);
    }

    public bool Remove(PbrModel model) => _models.Remove(model);

    public override void DoInitialize()
    {
      Renderer = new PbrRenderer(CoreInfo.Graphics.GraphicsDevice);
      InitRenderTarget();
      if (Scene is not null)
        Scene.Events.ClientSizeChanged += OnClientSizeChanged;
    }

    public override void DoUpdate(GameTime time)
    {
      float delta = (float)time.ElapsedGameTime.TotalSeconds;
      for (int i = 0; i < _models.Count; i++)
        _models[i].Update(delta);

      if (RawRt is not null && RawRt.Height > 0)
        UpdateCamera(RawRt.Width / (float)RawRt.Height);
    }

    protected virtual void UpdateCamera(float aspect)
    {
      Camera.Update(aspect);
    }

    public virtual void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, ClearColor, 1f, 0);
      if (Renderer is null)
        return;

      Renderer.DrawSkybox = DrawSkybox;
      Renderer.Begin(Camera, Lighting);
      for (int i = 0; i < _models.Count; i++)
        Renderer.DrawModel(_models[i]);
      Renderer.End();
    }

    public void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch)
    {
    }

    public void InitRenderTarget()
    {
      RawRt?.Dispose();
      int width = Math.Max(1, CoreInfo.ViewWidth);
      int height = Math.Max(1, CoreInfo.ViewHeight);
      RawRt = new RenderTarget2D(
          CoreInfo.Graphics.GraphicsDevice,
          width,
          height,
          false,
          SurfaceFormat.Color,
          DepthFormat.Depth24,
          0,
          RenderTargetUsage.DiscardContents);
    }

    public void OnClientSizeChanged(object o, EventArgs e)
    {
      InitRenderTarget();
    }

    public override void Dispose()
    {
      if (Scene is not null)
        Scene.Events.ClientSizeChanged -= OnClientSizeChanged;

      if (DisposeModels)
      {
        for (int i = 0; i < _models.Count; i++)
          _models[i].Dispose();
      }
      _models.Clear();

      Renderer?.Dispose();
      Renderer = null;
      RawRt?.Dispose();
    }
  }
}
