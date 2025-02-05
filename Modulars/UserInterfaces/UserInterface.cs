using Colin.Core.Common.Debugs;
using Colin.Core.Events;

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

    private DivThreshold _contianer = new DivThreshold("NomalContainer");

    public DivThreshold Container => _contianer;

    public RenderTarget2D RawRt { get; set; }

    public bool Enable { get; set; }

    public bool RawRtVisible { get; set; }

    public bool Presentation { get; set; }

    public Scene Scene { get; set; }

    public EventResponder Events;

    public void DoInitialize()
    {
      Events = new EventResponder("UserInterface.EventResponder");
      Scene.Events.Reset += () => LastFocus = Focus;
      Scene.Events.Mouse.Register(Events);
      Scene.Events.KeysEvent.Register(Events);
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

    public void Register(DivThreshold container) => Container?.Register(container);

    public void Remove(DivThreshold container, bool dispose) => Container?.Remove(container);

    public void SetContainer(DivThreshold container)
    {
      container.userInterface = this;
      _contianer = container;
      container.DoInitialize();
      container.Events.RegisterTo(Events);
    }

    public void Dispose()
    {
      Scene = null;
      Container.Dispose();
    }
  }
}