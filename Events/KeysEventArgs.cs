using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Events
{
    public class KeysEventArgs : BasicEventArgs
    {
        public Keys Key;
        public readonly KeyboardState State;
        public readonly KeyboardState Last;
        public KeysEventArgs( string name ) : base( name ) 
        {
            State = KeyboardResponder.State;
            Last = KeyboardResponder.StateLast;
        }
    }
}