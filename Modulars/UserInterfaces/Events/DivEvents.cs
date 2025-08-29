using Colin.Core.Events;

namespace Colin.Core.Modulars.UserInterfaces.Events
{
  public class DivEvents : IDisposable
  {
    private Div _div;
    public Div Div => _div;

    public DivEventNode<MouseHoverArgs> MouseHover;

    public DivEventNode<LeftClickedArgs> LeftClicked;
    public DivEventNode<LeftClickingArgs> LeftClicking;
    public DivEventNode<LeftDownArgs> LeftDown;
    public DivEventNode<LeftUpArgs> LeftUp;
    public DivEventNode<RightClickedArgs> RightClicked;
    public DivEventNode<RightClickingArgs> RightClicking;
    public DivEventNode<RightDownArgs> RightDown;
    public DivEventNode<RightUpArgs> RightUp;
    public DivEventNode<ScrollDownArgs> ScrollDown;
    public DivEventNode<ScrollUpArgs> ScrollUp;

    public DivEventNode<KeysClickedArgs> KeysClicked;
    public DivEventNode<KeysClickingArgs> KeysClicking;
    public DivEventNode<KeysDownArgs> KeysDown;

    public DivEvents(Div div) : base()
    {
      _div = div;
      MouseHover = new DivEventNode<MouseHoverArgs>();
      MouseHover.Div = div;
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

    void Drag(object sender, MouseArgs args)
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
    void DragEnd(object sender, MouseArgs args)
    {
      if (DivLock)
      {
        if (!Div.Interact.IsDraggable)
          return;
        DraggingState = false;
        _cachePos = new Vector2(-1, -1);
      }
    }
    void Lock(object sender, MouseArgs args)
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
      ScrollUp += MouseBlockOutEvent;
      KeysClicked += KeysBlockOutEvent;
      KeysClicking += KeysBlockOutEvent;
      KeysDown += KeysBlockOutEvent;
    }
    void MouseBlockOutEvent(object sender, MouseArgs args)
    {
      if (MouseCapture)
        args.IsCapture = true;
      if (MouseBubbling)
        args.StopBubbling = true;
    }
    void KeysBlockOutEvent(object sender, KeysArgs args)
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
          Div.Layout.Left = Math.Clamp(Div.Layout.Left, 0, Div.Interact.DragLimit.Width - Div.Layout.Width);
          Div.Layout.Top = Math.Clamp(Div.Layout.Top, 0, Div.Interact.DragLimit.Height - Div.Layout.Height);
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
      LeftClicked.Append(node.LeftClicked);
      LeftClicking.Append(node.LeftClicking);
      LeftDown.Append(node.LeftDown);
      LeftUp.Append(node.LeftUp);
      RightClicked.Append(node.RightClicked);
      RightClicking.Append(node.RightClicking);
      RightDown.Append(node.RightDown);
      RightUp.Append(node.RightUp);
      ScrollDown.Append(node.ScrollDown);
      ScrollUp.Append(node.ScrollUp);
      KeysClicking.Append(node.KeysClicking);
      KeysDown.Append(node.KeysDown);
      KeysClicked.Append(node.KeysClicked);
    }

    public void Insert(int index, DivEvents node)
    {
      MouseHover.Insert(index, node.MouseHover);
      LeftClicked.Insert(index, node.LeftClicked);
      LeftClicking.Insert(index, node.LeftClicking);
      LeftDown.Insert(index, node.LeftDown);
      LeftUp.Insert(index, node.LeftUp);
      RightClicked.Insert(index, node.RightClicked);
      RightClicking.Insert(index, node.RightClicking);
      RightDown.Insert(index, node.RightDown);
      RightUp.Insert(index, node.RightUp);
      KeysClicking.Insert(index, node.KeysClicking);
      KeysDown.Insert(index, node.KeysDown);
      KeysClicked.Insert(index, node.KeysClicked);
    }

    public void Register(DivEvents node)
    {
      MouseHover.Register(node.MouseHover);
      LeftClicked.Register(node.LeftClicked);
      LeftClicking.Register(node.LeftClicking);
      LeftDown.Register(node.LeftDown);
      LeftUp.Register(node.LeftUp);
      RightClicked.Register(node.RightClicked);
      RightClicking.Register(node.RightClicking);
      RightDown.Register(node.RightDown);
      RightUp.Register(node.RightUp);
      ScrollDown.Register(node.ScrollDown);
      ScrollUp.Register(node.ScrollUp);
      KeysClicking.Register(node.KeysClicking);
      KeysDown.Register(node.KeysDown);
      KeysClicked.Register(node.KeysClicked);
    }

    public void Remove(DivEvents node)
    {
      MouseHover.Remove(node.MouseHover);
      LeftClicked.Remove(node.LeftClicked);
      LeftClicking.Remove(node.LeftClicking);
      LeftDown.Remove(node.LeftDown);
      LeftUp.Remove(node.LeftUp);
      RightClicked.Remove(node.RightClicked);
      RightClicking.Remove(node.RightClicking);
      RightDown.Remove(node.RightDown);
      RightUp.Remove(node.RightUp);
      ScrollDown.Remove(node.ScrollDown);
      ScrollUp.Remove(node.ScrollUp);
      KeysClicking.Remove(node.KeysClicking);
      KeysDown.Remove(node.KeysDown);
      KeysClicked.Remove(node.KeysClicked);
    }

    public void Dispose()
    {
      _div = null;
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
      ScrollUp -= MouseBlockOutEvent;
      KeysClicked -= KeysBlockOutEvent;
      KeysClicking -= KeysBlockOutEvent;
      KeysDown -= KeysBlockOutEvent;
    }
  }
}