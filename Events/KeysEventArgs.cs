﻿using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Events
{
    public class KeysEventArgs : BasicEventArgs
    {
        public Keys Key;
        public bool ClickBefore;
        public bool Down;
        public bool ClickAfter;
        public KeysEventArgs( string name ) : base( name ) { }
    }
}