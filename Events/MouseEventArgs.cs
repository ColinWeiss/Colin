using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Events
{
    public class MouseEventArgs : BasicEventArgs
    {
        public readonly MouseState State;
        public readonly MouseState Last;
        public MouseEventArgs(string name) : base( name )
        {
            State = MouseResponder.State;
            Last = MouseResponder.StateLast;
        }
    }
}