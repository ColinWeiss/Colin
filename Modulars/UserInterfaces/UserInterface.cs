using Colin.Core.Common.Debugs;
using Colin.Core.Modulars.UserInterfaces.Animation;
using Microsoft.Xna.Framework.Input;

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

    /// <summary>
    /// 指示是否启用键盘/手柄焦点导航.
    /// <br>Tab 于全部可选中元素间循环; 方向键/十字键仅在已有焦点时向最近方向移动, 避免抢占游戏键位.</br>
    /// <br>可选中元素即 <see cref="InteractStyle.IsSelectable"/> 为真的可见元素, 预制件 (Button/InputTextBox) 默认置真.</br>
    /// </summary>
    public static bool FocusNavigation = true;

    /// <summary>
    /// 挂起全部界面元素的交互响应 (层级编辑器编辑模式下防止拖拽误触游戏逻辑).
    /// <br>置真时所有 Div 事件节点命中判定直接失败, 界面只渲染不响应.</br>
    /// </summary>
    public static bool InteractionSuspended;

    private readonly List<Div> _focusables = new List<Div>(64);

    private DivRoot _contianer = new DivRoot("NomalContainer");

    public DivRoot Root => _contianer;

    public Camera UICamera;

    public override void DoInitialize()
    {
      UICamera = new Camera();
      UICamera.DoInitialize(CoreInfo.ViewWidth, CoreInfo.ViewHeight);
      Scene.Events.ClientSizeChanged += (s, e) =>
      {
        UICamera.Translate = CoreInfo.ViewCenter;
        UICamera.Projection = Matrix.CreateOrthographicOffCenter(0f, CoreInfo.Graphics.GraphicsDevice.Viewport.Width, CoreInfo.Graphics.GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
        UICamera.View = Matrix.Identity;
        UICamera.ResetCamera();
        UICamera.Position = CoreInfo.ViewCenter;
        UICamera.TargetPosition = CoreInfo.ViewCenter;
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
      UpdateStamp++;
      if(CoreInfo.ViewSizeF.X >= 1921 || CoreInfo.ViewSizeF.Y >= 1081)
        UICamera.TargetZoom = CoreInfo.ViewSizeF / new Vector2(1920, 1080);
      else
        UICamera.TargetZoom = Vector2.One;

      UICamera.DoUpdate(time);
        Root?.DoUpdate(time);
      if (FocusNavigation)
        UpdateFocusNavigation();
      //动画播放器: 于整树布局写入全部完成之后采样回写 —— 采样值当帧渲染即可见, 不会被样式/布局覆盖
      for (int index = AnimationPlayers.Count - 1; index >= 0; index--)
        AnimationPlayers[index].Tick(this, (float)time.ElapsedGameTime.TotalSeconds);
    }

    /// <summary>挂在本界面上的动画播放器 (编辑器预览与运行时播放共用); 于每帧 DoUpdate 末尾被驱动.</summary>
    public readonly List<AnimationPlayer> AnimationPlayers = new List<AnimationPlayer>();

    /// <summary>模块更新计数 (诊断用: 编辑器读数据此判断本界面模块是否仍在被驱动).</summary>
    public uint UpdateStamp;

    private void UpdateFocusNavigation()
    {
      if (KeyboardResponder.Clicked(Keys.Tab))
      {
        CollectFocusables(Root);
        if (_focusables.Count == 0)
          return;
        int index = _focusables.IndexOf(Focus);
        Focus = index < 0 ? _focusables[0] : _focusables[(index + 1) % _focusables.Count];
        return;
      }
      if (Focus is null)
        return;
      int dx = 0, dy = 0;
      if (KeyboardResponder.Clicked(Keys.Left) || PadClicked(0))
        dx = -1;
      else if (KeyboardResponder.Clicked(Keys.Right) || PadClicked(1))
        dx = 1;
      else if (KeyboardResponder.Clicked(Keys.Up) || PadClicked(2))
        dy = -1;
      else if (KeyboardResponder.Clicked(Keys.Down) || PadClicked(3))
        dy = 1;
      if (dx == 0 && dy == 0)
        return;
      CollectFocusables(Root);
      if (_focusables.Count == 0)
        return;
      _focusables.Remove(Focus);
      if (_focusables.Count == 0)
        return;
      Rectangle current = Focus.Layout.Bounds;
      Point center = current.Center;
      Div best = null;
      long bestScore = long.MaxValue;
      for (int index = 0; index < _focusables.Count; index++)
      {
        Div candidate = _focusables[index];
        Point candidateCenter = candidate.Layout.Bounds.Center;
        int offsetX = candidateCenter.X - center.X;
        int offsetY = candidateCenter.Y - center.Y;
        if (dx != 0)
        {
          if (Math.Sign(offsetX) != dx || Math.Abs(offsetX) < Math.Abs(offsetY) * 2)
            continue;
          long score = Math.Abs(offsetX) + Math.Abs(offsetY) * 4L;
          if (score < bestScore)
          {
            bestScore = score;
            best = candidate;
          }
        }
        else
        {
          if (Math.Sign(offsetY) != dy || Math.Abs(offsetY) < Math.Abs(offsetX) * 2)
            continue;
          long score = Math.Abs(offsetY) + Math.Abs(offsetX) * 4L;
          if (score < bestScore)
          {
            bestScore = score;
            best = candidate;
          }
        }
      }
      if (best is not null)
        Focus = best;
    }

    private static bool PadClicked(int direction)
    {
      GamePadDPad current = ControllerResponder.state.DPad;
      GamePadDPad last = ControllerResponder.stateLast.DPad;
      return direction switch
      {
        0 => current.Left == ButtonState.Pressed && last.Left == ButtonState.Released,
        1 => current.Right == ButtonState.Pressed && last.Right == ButtonState.Released,
        2 => current.Up == ButtonState.Pressed && last.Up == ButtonState.Released,
        _ => current.Down == ButtonState.Pressed && last.Down == ButtonState.Released,
      };
    }

    private void CollectFocusables(Div div)
    {
      _focusables.Clear();
      CollectFocusables(div, _focusables);
    }

    private static void CollectFocusables(Div div, List<Div> results)
    {
      if (div is null || div.IsVisible is false || div.IsCulled)
        return;
      if (div is not DivRoot && div.Interact.IsSelectable && div.Interact.IsInteractive)
        results.Add(div);
      List<Div> children = div.Children;
      for (int count = 0; count < children.Count; count++)
        CollectFocusables(children[count], results);
    }

    public void BatchNormalBegin(Div div, BlendState blendState)
    {
      //兼容入口; 实际批次已由 UIBatch 状态机接管, 状态一致时不产生提交.
      Matrix transform = div.UpperCanvas is not null ? Matrix.Identity : UICamera.View;
      UIBatch.Request(blendState, SamplerState.PointWrap, DepthStencilState.None, UIBatch.DefaultRasterizer, default, transform);
    }

    public override void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      {
       // device.Clear(Color.Black);
        Root?.DoRender(device, batch);
        //离开界面渲染阶段前提交批次, 避免其他模块使用批次时状态冲突.
        UIBatch.Flush();
      }
    }
    public override void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch)
    {
    }

    public void Register(DivRoot container) => Root?.Register(container);

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
      Scene.Events.Mouse.ScrollClicked.Register(root.Events.ScrollClicked);
      Scene.Events.Mouse.ScrollUp.Register(root.Events.ScrollUp);

      Scene.Events.Keys.KeysClicked.Register(root.Events.KeysClicked);
      Scene.Events.Keys.KeysClicking.Register(root.Events.KeysClicking);
      Scene.Events.Keys.KeysDown.Register(root.Events.KeysDown);
    }

    public override void Dispose()
    {
      Scene = null;
      Root.Dispose();
      base.Dispose();
    }
  }
}