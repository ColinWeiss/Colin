using Colin.Core.Common.Debugs;

namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 场景模块: 用户交互界面.
  /// <br>用以构建应用程序中的用户交互界面.</br>
  /// </summary>
  public class UserInterface : ISceneModule, IRenderableISceneModule
  {
    public Div Focus;

    public Div LastFocus;

    private DivRoot _contianer = new DivRoot("NomalContainer");

    public DivRoot Container => _contianer;

    public RenderTarget2D RawRt { get; set; }

    public bool Enable { get; set; }

    public bool RawRtVisible { get; set; }

    public bool Presentation { get; set; }

    public Scene Scene { get; set; }

    public void DoInitialize()
    {
    }

    public void Start() { }

    public void DoUpdate(GameTime time)
    {
      Container?.DoUpdate(time);
    }

    public static void BatchNormalBegin(SpriteBatch batch)
    {
      batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
    }

    public void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      using (DebugProfiler.Tag("UI"))
      {
        device.Clear(Color.Transparent);
        BatchNormalBegin(batch);
        Container?.DoRender(device, batch);
        batch.End();
      }
    }
    public void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch) { }

    public void Register(DivRoot container) => Container?.Register(container);

    public void Remove(DivRoot container, bool dispose) => Container?.Remove(container);

    public void SetContainer(DivRoot root)
    {
      root.userInterface = this;
      _contianer = root;
      root.DoInitialize();

      Scene.Events.Mouse.LeftClicked.Register(root.Events.LeftClicked);
      Scene.Events.Mouse.LeftClicking.Register(root.Events.LeftClicking);
      Scene.Events.Mouse.LeftDown.Register(root.Events.LeftDown);
      Scene.Events.Mouse.LeftUp.Register(root.Events.LeftUp);
      Scene.Events.Mouse.RightClicked.Register(root.Events.RightClicked);
      Scene.Events.Mouse.RightClicking.Register(root.Events.RightClicking);
      Scene.Events.Mouse.RightDown.Register(root.Events.RightDown);
      Scene.Events.Mouse.RightUp.Register(root.Events.RightUp);
      Scene.Events.Mouse.ScrollDown.Register(root.Events.ScrollDown);
      Scene.Events.Mouse.ScrollUp.Register(root.Events.ScrollUp);

      Scene.Events.Keys.KeysClicked.Register(root.Events.KeysClicked);
      Scene.Events.Keys.KeysClicking.Register(root.Events.KeysClicking);
      Scene.Events.Keys.KeysDown.Register(root.Events.KeysDown);
    }

    public void Dispose()
    {
      Scene = null;
      Container.Dispose();
    }
  }
}