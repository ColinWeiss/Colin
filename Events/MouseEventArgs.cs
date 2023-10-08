using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Events
{
    public class MouseEventArgs : BasicEventArgs
    {
        public readonly MouseState State;
        public readonly MouseState Last;
        public MouseEventArgs( string name ) : base( name ) 
        {
            State = MouseResponder.State;
            Last = MouseResponder.StateLast;
        }
    }
}