using Colin.Core.Events;

namespace Colin.Core.Modulars.UserInterfaces.Events
{
  public class DivEvents : IDisposable
  {
    private Div _div;
    public Div Div => _div;

    public DivEventNode<MouseHoverArgs> MouseHover;

    /// <summary>
    /// 悬停开始:鼠标这一帧刚落到元素上时触发一次.
    /// <br>这是本地算出来的边缘事件,只触发本节点,不参与捕获/冒泡。</br>
    /// </summary>
    public DivEventNode<MouseHoverArgs> HoverBegan;

    /// <summary>
    /// 悬停结束:鼠标这一帧刚离开元素时触发一次;元素隐藏也会按离开处理.
    /// </summary>
    public DivEventNode<MouseHoverArgs> HoverEnded;

    /// <summary>
    /// 指示鼠标当前是否悬停在元素上;每帧随交互状态刷新.
    /// <br>做悬停反馈(变色/亮边)直接每帧看它;要"进/出那一刻"做事就挂 <see cref="HoverBegan"/> / <see cref="HoverEnded"/>.</br>
    /// </summary>
    public bool IsHovered;

    public DivEventNode<LeftClickedArgs> LeftClicked;
    public DivEventNode<LeftClickingArgs> LeftClicking;
    public DivEventNode<LeftDownArgs> LeftDown;
    public DivEventNode<LeftUpArgs> LeftUp;
    public DivEventNode<RightClickedArgs> RightClicked;
    public DivEventNode<RightClickingArgs> RightClicking;
    public DivEventNode<RightDownArgs> RightDown;
    public DivEventNode<RightUpArgs> RightUp;
    public DivEventNode<ScrollDownArgs> ScrollDown;
    public DivEventNode<ScrollClickedArgs> ScrollClicked;
    public DivEventNode<ScrollUpArgs> ScrollUp;

    public DivEventNode<KeysClickedArgs> KeysClicked;
    public DivEventNode<KeysClickingArgs> KeysClicking;
    public DivEventNode<KeysDownArgs> KeysDown;

    public DivEvents(Div div)
    {
      _div = div;
      MouseHover = new DivEventNode<MouseHoverArgs> { Div = div };
      HoverBegan = new DivEventNode<MouseHoverArgs> { Div = div };
      HoverEnded = new DivEventNode<MouseHoverArgs> { Div = div };
      LeftClicked = new DivEventNode<LeftClickedArgs> { Div = div };
      LeftClicking = new DivEventNode<LeftClickingArgs> { Div = div };
      LeftDown = new DivEventNode<LeftDownArgs> { Div = div };
      LeftUp = new DivEventNode<LeftUpArgs> { Div = div };
      RightClicked = new DivEventNode<RightClickedArgs> { Div = div };
      RightClicking = new DivEventNode<RightClickingArgs> { Div = div };
      RightDown = new DivEventNode<RightDownArgs> { Div = div };
      RightUp = new DivEventNode<RightUpArgs> { Div = div };
      ScrollDown = new DivEventNode<ScrollDownArgs> { Div = div };
      ScrollClicked = new DivEventNode<ScrollClickedArgs> { Div = div };
      ScrollUp = new DivEventNode<ScrollUpArgs> { Div = div };
      KeysClicked = new DivEventNode<KeysClickedArgs> { Div = div };
      KeysClicking = new DivEventNode<KeysClickingArgs> { Div = div };
      KeysDown = new DivEventNode<KeysDownArgs> { Div = div };
      //通道顺序即各批量方法的处理顺序, 须与上方节点一一对应.
      _channels = new IDivEventChannel[]
      {
        MouseHover, HoverBegan, HoverEnded,
        LeftClicked, LeftClicking, LeftDown, LeftUp,
        RightClicked, RightClicking, RightDown, RightUp,
        ScrollDown, ScrollClicked, ScrollUp,
        KeysClicked, KeysClicking, KeysDown,
      };
      //拖拽/锁定是元素自身的行为, 与"捕获/阻止冒泡"开关无关, 直接常驻.
      //[!] 捕获与阻止冒泡不再逐通道挂空处理器 (每个元素 17 个委托的空转):
      //    它们由 DivEventNode.OnBeforeInvoke 在派发前统一登记, 语义与顺序保持不变.
      LeftClicking += Drag;
      LeftClicked += DragEnd;
      RightClicking += Lock;
    }

    private readonly IDivEventChannel[] _channels;

    public bool MouseCapture;

    public bool MouseBubbling;

    public bool KeysCapture;

    public bool KeysBubbling;

    public bool DivLock = false;

    public bool DraggingState = false;

    private Vector2 _cachePos = new Vector2(-1, -1);

    private void Drag(object sender, MouseArgs args)
    {
      UserInterface module = Div.Module;
      if (module is null)
        return;
      Vector2 mousePos = module.UICamera.ConvertToWorld(MouseResponder.Position);
      if (!DivLock)
        DivLock = true;
      module.Focus = Div;
      if (!Div.Interact.IsDraggable)
        return;
      DraggingState = true;
      if (Div.Parent != null)
      {
        Vector2 mouseForParentLocation = mousePos - Div.Parent.Layout.Location;
        _cachePos = mouseForParentLocation - Div.Layout.Location;
      }
      else
      {
        _cachePos = mousePos - Div.Layout.Location;
      }
    }
    private void DragEnd(object sender, MouseArgs args)
    {
      if (DivLock)
      {
        if (!Div.Interact.IsDraggable)
          return;
        DraggingState = false;
        _cachePos = new Vector2(-1, -1);
      }
    }
    private void Lock(object sender, MouseArgs args)
    {
      if (!DivLock)
        DivLock = true;
    }

    public void DoUpdate()
    {
      UserInterface module = Div.Module;
      if (module is null)
        return;
      Vector2 mousePos = module.UICamera.ConvertToWorld(MouseResponder.Position);
      Div.Interact.InteractionLast = Div.Interact.Interaction;
      if (Div.ContainsScreenPoint(mousePos.ToPoint()) && Div.Interact.IsInteractive)
        Div.Interact.Interaction = true;
      else
        Div.Interact.Interaction = false;
      //悬停状态与进出边缘:反馈变色每帧看 IsHovered 就行,进出那一刻靠这对边缘事件.
      IsHovered = Div.Interact.Interaction;
      if (IsHovered && Div.Interact.InteractionLast is false)
        HoverBegan.TriggerSelf(new MouseHoverArgs { Sender = Div, MousePos = mousePos });
      else if (IsHovered is false && Div.Interact.InteractionLast)
        HoverEnded.TriggerSelf(new MouseHoverArgs { Sender = Div, MousePos = mousePos });
      if (MouseResponder.LeftUp)
        DraggingState = false;
      if (DraggingState && Div.Interact.IsDraggable)
      {
        if (!Div.Interact.IsDraggable)
          return;
        if (Div.Parent != null)
        {
          Vector2 _resultLocation = mousePos - Div.Parent.Layout.Location - _cachePos;
          Div.Layout.Left = _resultLocation.X;
          Div.Layout.Top = _resultLocation.Y;
        }
        else
        {
          Vector2 _resultLocation = mousePos - _cachePos;
          Div.Layout.Left = _resultLocation.X;
          Div.Layout.Top = _resultLocation.Y;
        }
        if (Div.Interact.DragLimit != Rectangle.Empty)
        {
          //限制矩形是"可摆放区域"的绝对坐标, 原点必须参与钳制 (旧实现把 X/Y 当 0, 限定区整体偏移).
          Rectangle limit = Div.Interact.DragLimit;
          float maxLeft = Math.Max(limit.X, limit.X + limit.Width - Div.Layout.Width);
          float maxTop = Math.Max(limit.Y, limit.Y + limit.Height - Div.Layout.Height);
          Div.Layout.Left = Math.Clamp(Div.Layout.Left, limit.X, maxLeft);
          Div.Layout.Top = Math.Clamp(Div.Layout.Top, limit.Y, maxTop);
        }
      }
      if (Div.Module.Focus == Div && Div.Module.LastFocus != Div)
      {
        DivLock = true;
        //     GetFocus?.Invoke();
      }
      if (Div.Module.Focus != Div && Div.Module.LastFocus == Div)
      {
        DivLock = false;
        //      LoseFocus?.Invoke();
      }
    }

    public void Append(DivEvents node)
    {
      for (int index = 0; index < _channels.Length; index++)
        _channels[index].ChannelAppend(node._channels[index]);
    }

    public void Insert(int index, DivEvents node)
    {
      for (int channel = 0; channel < _channels.Length; channel++)
        _channels[channel].ChannelInsert(index, node._channels[channel]);
    }

    public void Register(DivEvents node)
    {
      for (int index = 0; index < _channels.Length; index++)
        _channels[index].ChannelRegister(node._channels[index]);
    }

    public void Remove(DivEvents node)
    {
      for (int index = 0; index < _channels.Length; index++)
        _channels[index].ChannelRemove(node._channels[index]);
    }

    public void Dispose()
    {
      _div = null;
      for (int index = 0; index < _channels.Length; index++)
      {
        _channels[index].ChannelDispose();
        _channels[index].Div = null;
      }
      LeftClicking -= Drag;
      LeftClicked -= DragEnd;
      RightClicking -= Lock;
    }
  }
}