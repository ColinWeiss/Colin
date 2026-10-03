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

    public DivEvents(Div div) : base()
    {
      _div = div;
      MouseHover = new DivEventNode<MouseHoverArgs>();
      MouseHover.Div = div;
      HoverBegan = new DivEventNode<MouseHoverArgs>();
      HoverBegan.Div = div;
      HoverEnded = new DivEventNode<MouseHoverArgs>();
      HoverEnded.Div = div;
      LeftClicked = new DivEventNode<LeftClickedArgs>();
      LeftClicked.Div = div;
      LeftClicking = new DivEventNode<LeftClickingArgs>();
      LeftClicking.Div = div;
      LeftDown = new DivEventNode<LeftDownArgs>();
      LeftDown.Div = div;
      LeftUp = new DivEventNode<LeftUpArgs>();
      LeftUp.Div = div;
      RightClicked = new DivEventNode<RightClickedArgs>();
      RightClicked.Div = div;
      RightClicking = new DivEventNode<RightClickingArgs>();
      RightClicking.Div = div;
      RightDown = new DivEventNode<RightDownArgs>();
      RightDown.Div = div;
      RightUp = new DivEventNode<RightUpArgs>();
      RightUp.Div = div;
      ScrollDown = new DivEventNode<ScrollDownArgs>();
      ScrollDown.Div = div;
      ScrollClicked = new DivEventNode<ScrollClickedArgs>();
      ScrollClicked.Div = div;
      ScrollUp = new DivEventNode<ScrollUpArgs>();
      ScrollUp.Div = div;
      KeysClicked = new DivEventNode<KeysClickedArgs>();
      KeysClicked.Div = div;
      KeysClicking = new DivEventNode<KeysClickingArgs>();
      KeysClicking.Div = div;
      KeysDown = new DivEventNode<KeysDownArgs>();
      KeysDown.Div = div;
    }

    public bool MouseCapture;

    public bool MouseBubbling;

    public bool KeysCapture;

    public bool KeysBubbling;

    public bool DivLock = false;

    public bool DraggingState = false;

    private Vector2 _cachePos = new Vector2(-1, -1);

    private void Drag(object sender, MouseArgs args)
    {
      Vector2 mousePos = Div.Module.UICamera.ConvertToWorld(MouseResponder.Position);
      if (!DivLock)
        DivLock = true;
      Div.Module.Focus = Div;
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
    public void DoBlockOut()
    {
      MouseHover += MouseBlockOutEvent;
      LeftClicking += MouseBlockOutEvent;
      LeftClicking += Drag;
      LeftDown += MouseBlockOutEvent;
      LeftClicked += MouseBlockOutEvent;
      LeftClicked += DragEnd;
      LeftUp += MouseBlockOutEvent;
      RightClicked += MouseBlockOutEvent;
      RightClicking += MouseBlockOutEvent;
      RightClicking += Lock;
      RightDown += MouseBlockOutEvent;
      RightUp += MouseBlockOutEvent;
      ScrollDown += MouseBlockOutEvent;
      ScrollClicked += MouseBlockOutEvent;
      ScrollUp += MouseBlockOutEvent;
      KeysClicked += KeysBlockOutEvent;
      KeysClicking += KeysBlockOutEvent;
      KeysDown += KeysBlockOutEvent;
    }
    private void MouseBlockOutEvent(object sender, MouseArgs args)
    {
      if (MouseCapture)
        args.IsCapture = true;
      if (MouseBubbling)
        args.StopBubbling = true;
    }
    private void KeysBlockOutEvent(object sender, KeysArgs args)
    {
      if (KeysCapture)
        args.IsCapture = true;
      if (KeysBubbling)
        args.StopBubbling = true;
    }

    public void DoUpdate()
    {
      Vector2 mousePos = Div.Module.UICamera.ConvertToWorld(MouseResponder.Position);
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
        if (Div.Interact.IsDraggable && Div.Interact.DragLimit != Rectangle.Empty)
        {
          //限制区域小于元素自身时钳制区间退化为空, 收拢到原点, 避免 ArgumentException.
          Div.Layout.Left = Math.Clamp(Div.Layout.Left, 0, Math.Max(0, Div.Interact.DragLimit.Width - Div.Layout.Width));
          Div.Layout.Top = Math.Clamp(Div.Layout.Top, 0, Math.Max(0, Div.Interact.DragLimit.Height - Div.Layout.Height));
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
      MouseHover.Append(node.MouseHover);
      HoverBegan.Append(node.HoverBegan);
      HoverEnded.Append(node.HoverEnded);
      LeftClicked.Append(node.LeftClicked);
      LeftClicking.Append(node.LeftClicking);
      LeftDown.Append(node.LeftDown);
      LeftUp.Append(node.LeftUp);
      RightClicked.Append(node.RightClicked);
      RightClicking.Append(node.RightClicking);
      RightDown.Append(node.RightDown);
      RightUp.Append(node.RightUp);
      ScrollDown.Append(node.ScrollDown);
      ScrollClicked.Append(node.ScrollClicked);
      ScrollUp.Append(node.ScrollUp);
      KeysClicking.Append(node.KeysClicking);
      KeysDown.Append(node.KeysDown);
      KeysClicked.Append(node.KeysClicked);
    }

    public void Insert(int index, DivEvents node)
    {
      MouseHover.Insert(index, node.MouseHover);
      HoverBegan.Insert(index, node.HoverBegan);
      HoverEnded.Insert(index, node.HoverEnded);
      LeftClicked.Insert(index, node.LeftClicked);
      LeftClicking.Insert(index, node.LeftClicking);
      LeftDown.Insert(index, node.LeftDown);
      LeftUp.Insert(index, node.LeftUp);
      RightClicked.Insert(index, node.RightClicked);
      RightClicking.Insert(index, node.RightClicking);
      RightDown.Insert(index, node.RightDown);
      RightUp.Insert(index, node.RightUp);
      ScrollDown.Insert(index, node.ScrollDown);
      ScrollClicked.Insert(index, node.ScrollClicked);
      ScrollUp.Insert(index, node.ScrollUp);
      KeysClicking.Insert(index, node.KeysClicking);
      KeysDown.Insert(index, node.KeysDown);
      KeysClicked.Insert(index, node.KeysClicked);
    }

    public void Register(DivEvents node)
    {
      MouseHover.Register(node.MouseHover);
      HoverBegan.Register(node.HoverBegan);
      HoverEnded.Register(node.HoverEnded);
      LeftClicked.Register(node.LeftClicked);
      LeftClicking.Register(node.LeftClicking);
      LeftDown.Register(node.LeftDown);
      LeftUp.Register(node.LeftUp);
      RightClicked.Register(node.RightClicked);
      RightClicking.Register(node.RightClicking);
      RightDown.Register(node.RightDown);
      RightUp.Register(node.RightUp);
      ScrollDown.Register(node.ScrollDown);
      ScrollClicked.Register(node.ScrollClicked);
      ScrollUp.Register(node.ScrollUp);
      KeysClicking.Register(node.KeysClicking);
      KeysDown.Register(node.KeysDown);
      KeysClicked.Register(node.KeysClicked);
    }

    public void Remove(DivEvents node)
    {
      MouseHover.Remove(node.MouseHover);
      HoverBegan.Remove(node.HoverBegan);
      HoverEnded.Remove(node.HoverEnded);
      LeftClicked.Remove(node.LeftClicked);
      LeftClicking.Remove(node.LeftClicking);
      LeftDown.Remove(node.LeftDown);
      LeftUp.Remove(node.LeftUp);
      RightClicked.Remove(node.RightClicked);
      RightClicking.Remove(node.RightClicking);
      RightDown.Remove(node.RightDown);
      RightUp.Remove(node.RightUp);
      ScrollDown.Remove(node.ScrollDown);
      ScrollClicked.Remove(node.ScrollClicked);
      ScrollUp.Remove(node.ScrollUp);
      KeysClicking.Remove(node.KeysClicking);
      KeysDown.Remove(node.KeysDown);
      KeysClicked.Remove(node.KeysClicked);
    }

    public void Dispose()
    {
      _div = null;
      MouseHover.Dispose();
      HoverBegan.Dispose();
      HoverEnded.Dispose();
      LeftClicked.Dispose();
      LeftClicking.Dispose();
      LeftDown.Dispose();
      LeftUp.Dispose();
      RightClicked.Dispose();
      RightClicking.Dispose();
      RightDown.Dispose();
      RightUp.Dispose();
      ScrollDown.Dispose();
      ScrollClicked.Dispose();
      ScrollUp.Dispose();
      KeysClicked.Dispose();
      KeysClicking.Dispose();
      KeysDown.Dispose();
      MouseHover.Div = null;
      LeftClicked.Div = null;
      LeftClicking.Div = null;
      LeftDown.Div = null;
      LeftUp.Div = null;
      RightClicked.Div = null;
      RightClicking.Div = null;
      RightDown.Div = null;
      RightUp.Div = null;
      ScrollDown.Div = null;
      ScrollClicked.Div = null;
      ScrollUp.Div = null;
      KeysClicked.Div = null;
      KeysClicking.Div = null;
      KeysDown.Div = null;
      MouseHover -= MouseBlockOutEvent;
      LeftClicking -= MouseBlockOutEvent;
      LeftClicking -= Drag;
      LeftDown -= MouseBlockOutEvent;
      LeftClicked -= MouseBlockOutEvent;
      LeftClicked -= DragEnd;
      LeftUp -= MouseBlockOutEvent;
      RightClicked -= MouseBlockOutEvent;
      RightClicking -= MouseBlockOutEvent;
      RightClicking -= Lock;
      RightDown -= MouseBlockOutEvent;
      RightUp -= MouseBlockOutEvent;
      ScrollDown -= MouseBlockOutEvent;
      ScrollClicked -= MouseBlockOutEvent;
      ScrollUp -= MouseBlockOutEvent;
      KeysClicked -= KeysBlockOutEvent;
      KeysClicking -= KeysBlockOutEvent;
      KeysDown -= KeysBlockOutEvent;
    }
  }
}