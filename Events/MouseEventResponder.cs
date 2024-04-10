namespace Colin.Core.Events
{
  public class MouseEventResponder : EventResponder
  {
    public MouseEventResponder(string name) : base(name) { }
    public event EventHandler<MouseEventArgs> Hover;
    public event EventHandler<MouseEventArgs> LeftClickBefore;
    public event EventHandler<MouseEventArgs> LeftDown;
    public event EventHandler<MouseEventArgs> LeftClickAfter;
    public event EventHandler<MouseEventArgs> LeftUp;
    public event EventHandler<MouseEventArgs> ScrollUp;
    public event EventHandler<MouseEventArgs> ScrollDown;

    public override void Handle(IEvent theEvent)
    {
      if (theEvent is MouseEventArgs mouseEvent)
      {
        Hover?.Invoke(this, mouseEvent);
        if (MouseResponder.LeftClickBefore)
          LeftClickBefore?.Invoke(this, mouseEvent);
        if (MouseResponder.LeftDown)
          LeftDown?.Invoke(this, mouseEvent);
        if (MouseResponder.LeftClickAfter)
          LeftClickAfter?.Invoke(this, mouseEvent);
        if (MouseResponder.LeftUp)
          LeftUp?.Invoke(this, mouseEvent);
        if (MouseResponder.ScrollUp)
          ScrollUp?.Invoke(this, mouseEvent);
        if (MouseResponder.ScrollDown)
          ScrollDown?.Invoke(this, mouseEvent);
      }
    }
  }
}