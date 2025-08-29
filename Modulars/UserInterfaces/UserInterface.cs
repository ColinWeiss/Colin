using Colin.Core.Common.Debugs;

namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 场景模块: 用户交互界面.
  /// <br>用以构建应用程序中的用户交互界面.</br>
  /// </summary>
  public class UserInterface : SceneRenderModule
  {
    public Div Focus;

    public Div LastFocus;

    private DivRoot _contianer = new DivRoot("NomalContainer");

    public DivRoot Container => _contianer;

    public Camera UICamera;

    public override void DoInitialize()
    {
      UICamera = new Camera();
      UICamera.DoInitialize(CoreInfo.ViewWidth, CoreInfo.ViewHeight);
      UICamera.Translate = CoreInfo.ViewCenter;
      Scene.Events.ClientSizeChanged += (s, e) =>
      {
        UICamera.SetWidth(CoreInfo.ViewWidth);
        UICamera.SetHeight(CoreInfo.ViewHeight);
        UICamera.Projection = Matrix.CreateOrthographicOffCenter(0f, CoreInfo.ViewWidth, CoreInfo.ViewHeight, 0f, 0f, 1f);
        UICamera.View = Matrix.Identity;
        UICamera.ResetCamera();
      };

      base.DoInitialize();
    }

    private Vector2 tar;

    public override void Start()
    {
      UICamera.Position = CoreInfo.ViewCenter;
      UICamera.TargetPosition = CoreInfo.ViewCenter;
      base.Start();
    }
    public override void DoUpdate(GameTime time)
    {
      if (KeyboardResponder.IsKeyClickBefore(Keys.F11))
        UICamera.TargetPosition = CoreInfo.ViewCenter + Vector2.UnitY * 2000;
      if (KeyboardResponder.IsKeyClickBefore(Keys.F12))
        UICamera.TargetPosition = CoreInfo.ViewCenter;


      if (KeyboardResponder.IsKeyClickBefore(Keys.F8))
        UICamera.TargetZoom = Vector2.One * 0.5f;
      if (KeyboardResponder.IsKeyClickBefore(Keys.F9))
        UICamera.TargetZoom = Vector2.One;

      if (KeyboardResponder.IsKeyClickBefore(Keys.F6))
        UICamera.TargetRotation = 3.14f;
      if (KeyboardResponder.IsKeyClickBefore(Keys.F7))
        UICamera.TargetRotation = 0f;

      UICamera.DoUpdate(time);
      Container?.DoUpdate(time);
    }

    public void BatchNormalBegin(Div div, BlendState blendState)
    {
      CoreInfo.Batch.Begin(
        SpriteSortMode.Deferred,
        blendState,
        SamplerState.PointClamp,
        transformMatrix: div.UpperCanvas is not null ? null : UICamera.View);
    }

    public override void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      using (DebugProfiler.Tag("UI"))
      {
        device.Clear(Color.Transparent);
        //      CoreInfo.Batch.Begin(
        //        SpriteSortMode.Deferred,
        //        BlendState.AlphaBlend,
        //        SamplerState.PointClamp,
        //        transformMatrix: UICamera.View);
        //      Container?.DoRender(device, batch);
        //      batch.End();

        Container?.DoRender(device, batch);
      }
    }
    public override void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch) { }

    public void Register(DivRoot container) => Container?.Register(container);

    public void SetRoot(DivRoot root)
    {
      root._module = this;
      _contianer = root;
      root.DoInitialize();

      Scene.Events.Mouse.MouseHover.Register(root.Events.MouseHover);
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

    public override void Dispose()
    {
      Scene = null;
      Container.Dispose();
      base.Dispose();
    }
  }
}