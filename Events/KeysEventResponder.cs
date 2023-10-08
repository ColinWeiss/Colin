using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Events
{
    public class KeysEventResponder : EventResponder
    {
        public KeysEventResponder( string name ) : base( name ) { }
        public event EventHandler<KeysEventArgs> ClickBefore;
        public event EventHandler<KeysEventArgs> Down;
        public event EventHandler<KeysEventArgs> ClickAfter;

        public override void Handle( BasicEventArgs theEvent )
        {
            if(theEvent is KeysEventArgs keysEvent)
            {
                Keys[] lasts = KeyboardResponder.StateLast.GetPressedKeys();
                Keys[] current = KeyboardResponder.State.GetPressedKeys();
                foreach(var key in lasts)
                {
                    if(!current.Contains( key ))
                    {
                        keysEvent.Key = key;
                        ClickAfter?.Invoke( this, keysEvent );
                    }
                }
                foreach(var key in current)
                {
                    if(!lasts.Contains( key ))
                    {
                        keysEvent.Key = key;
                        ClickBefore?.Invoke( this, keysEvent );
                    }
                    if(lasts.Contains( key ))
                    {
                        keysEvent.Key = key;
                        Down?.Invoke( this, keysEvent );
                    }
                }
            }
        }
    }
}