using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Events
{
  public class MouseEventArgs : IEvent
  {
    public readonly MouseState State;
    public readonly MouseState Last;
    public bool Captured { get; set; }
    public string Name { get; set; }
    public bool Postorder { get; set; }

    public MouseEventArgs(string name)
    {
      Name = name;
      State = MouseResponder.State;
      Last = MouseResponder.StateLast;
      Postorder = true;
    }
  }
}