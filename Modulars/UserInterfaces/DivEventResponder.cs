using Colin.Core.Events;

namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 划分元素事件响应器.
  /// </summary>
  public class DivEventResponder
  {
    public Div Div;

    public DivEventResponder(Div division) => Div = division;

    private MouseEventResponder Mouse = new MouseEventResponder("MouseEvents");

    private KeysEventResponder Keys = new KeysEventResponder("KeysEvents");

    public void Register(Div div)
    {
      Mouse.Register(div.Events.Mouse);
      Keys.Register(div.Events.Keys);
    }
    public void RegisterTo( EventResponder eventResponder )
    {
      eventResponder.Register(Mouse);
      eventResponder.Register(Keys);
    }

    public void Remove(Div div)
    {
      Mouse.Remove(div.Events.Mouse);
      Keys.Remove(div.Events.Keys);
    }

    public event Action<MouseEventArgs> HoverStart;
    public event Action<MouseEventArgs> Hover;
    public event Action<MouseEventArgs> HoverOver;

    public event Action<MouseEventArgs> LeftClickBefore;
    public event Action<MouseEventArgs> LeftDown;
    public event Action<MouseEventArgs> LeftClickAfter;
    public event Action<MouseEventArgs> LeftUp;

    public event Action<MouseEventArgs> RightClickBefore;
    public event Action<MouseEventArgs> RightDown;
    public event Action<MouseEventArgs> RightClickAfter;
    public event Action<MouseEventArgs> RightUp;

    public event Action DragStart;
    public event Action Dragging;
    public event Action DragOver;

    public event Action<MouseEventArgs> ScrollUp;
    public event Action<MouseEventArgs> ScrollDown;


    public event Action GetFocus;
    public event Action LoseFocus;

    public event EventHandler<KeyEventArgs> KeyClickBefore;

    public event EventHandler<KeyEventArgs> KeyDown;

    public event EventHandler<KeyEventArgs> KeyClickAfter;

    /// <summary>
    /// 指示拖拽状态.
    /// </summary>
    public bool DraggingState = false;

    public bool DivLock = false;

    public void DoInitialize()
    {
      Mouse.Hover += (s, e) =>
          {
            if (Div.Interact.Interaction && !Div.Interact.InteractionLast)
              HoverStart?.Invoke(e);
            Invoke(e, Hover);
            if (!Div.Interact.Interaction && Div.Interact.InteractionLast)
              HoverOver?.Invoke(e);
          };
      Mouse.LeftClickBefore += (s, e) =>
      {
        if (Div.IsVisible && Div.ContainsScreenPoint(MouseResponder.State.Position) && Div.Interact.IsInteractive)
        {
          if (!DivLock)
            DivLock = true;
        }
        Invoke(e, LeftClickBefore);
        Invoke(e, (e) =>
              {
                Div.UserInterface.Focus = Div;
                if (!Div.Interact.IsDraggable)
                  return;
                DragStart?.Invoke();
                DraggingState = true;
                if (Div.Parent != null)
                {
                  Vector2 mouseForParentLocation = MouseResponder.Position - Div.Parent.Layout.Location;
                  _cachePos = mouseForParentLocation - Div.Layout.Location;
                }
                else
                {
                  _cachePos = MouseResponder.Position - Div.Layout.Location;
                }
              });
      };
      Mouse.LeftDown += (s, e) =>
      {
        Invoke(e, LeftDown);
      };
      Mouse.LeftClickAfter += (s, e) =>
      {
        Invoke(e, LeftClickAfter);
        if (DivLock)
        {
          DragOver?.Invoke();
          if (!Div.Interact.IsDraggable)
            return;
          DraggingState = false;
          _cachePos = new Vector2(-1, -1);
        }
      };
      Mouse.LeftUp += (s, e) => Invoke(e, LeftUp);
      Mouse.RightClickBefore += (s,e) => Invoke(e, RightClickBefore);
      Mouse.RightDown += (s, e) => Invoke(e, RightDown);
      Mouse.RightClickAfter += (s, e) => Invoke(e, RightClickAfter);
      Mouse.RightUp += (s, e) => Invoke(e, RightUp);
      Mouse.ScrollUp += (s, e) => Invoke(e, ScrollUp, true);
      Mouse.ScrollDown += (s, e) => Invoke(e, ScrollDown, true);
      Keys.ClickBefore += KeyClickBefore;
      Keys.Down += KeyDown;
      Keys.ClickAfter += KeyClickAfter;
    }
    private void Invoke(MouseEventArgs e, Action<MouseEventArgs> action, bool divLock = false)
    {
      if (Div.IsVisible &&
          Div.ContainsScreenPoint(MouseResponder.State.Position) &&
          Div.Interact.IsInteractive &&
          (DivLock || divLock))
      {
        if (Div.Interact.IsBubbling)
          e.Captured = true;
        action?.Invoke(e);
      }
    }
    private Vector2 _cachePos = new Vector2(-1, -1);
    public void DoUpdate()
    {
      Div.Interact.InteractionLast = Div.Interact.Interaction;
      if (Div.ContainsScreenPoint(MouseResponder.State.Position) && Div.Interact.IsInteractive)
        Div.Interact.Interaction = true;
      else
        Div.Interact.Interaction = false;
      if (DraggingState && Div.Interact.IsDraggable)
      {
        if (!Div.Interact.IsDraggable)
          return;
        if (Div.Parent != null)
        {
          Vector2 _resultLocation = MouseResponder.Position - Div.Parent.Layout.Location - _cachePos;
          Div.Layout.Left = _resultLocation.X;
          Div.Layout.Top = _resultLocation.Y;
        }
        else
        {
          Vector2 _resultLocation = MouseResponder.Position - _cachePos;
          Div.Layout.Left = _resultLocation.X;
          Div.Layout.Top = _resultLocation.Y;
        }
        if (Div.Interact.IsDraggable && Div.Interact.DragLimit != Rectangle.Empty)
        {
          Div.Layout.Left = Math.Clamp(Div.Layout.Left, 0, Div.Interact.DragLimit.Width - Div.Layout.Width);
          Div.Layout.Top = Math.Clamp(Div.Layout.Top, 0, Div.Interact.DragLimit.Height - Div.Layout.Height);
        }
        Dragging?.Invoke();
      }
      if (Div.UserInterface.Focus == Div && Div.UserInterface.LastFocus != Div)
      {
        DivLock = true;
        GetFocus?.Invoke();
      }
      if (Div.UserInterface.Focus != Div && Div.UserInterface.LastFocus == Div)
      {
        DivLock = false;
        LoseFocus?.Invoke();
      }
    }
  }
}