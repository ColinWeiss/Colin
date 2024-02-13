using TextInputEventArgs = MonoGame.IMEHelper.TextInputEventArgs;

namespace Colin.Core.Events
{
    public class KeysEventResponder : EventResponder
    {
        public KeysEventResponder(string name) : base(name) { }
        public event EventHandler<KeyEventArgs> ClickBefore;
        public event EventHandler<KeyEventArgs> Down;
        public event EventHandler<KeyEventArgs> ClickAfter;

        public override void Handle(IEvent theEvent)
        {
            if (theEvent is KeyEventArgs keysEvent)
            {
                if (keysEvent.ClickBefore)
                    ClickBefore?.Invoke(this, keysEvent);
                if (keysEvent.Down)
                    Down?.Invoke(this, keysEvent);
                if (keysEvent.ClickAfter)
                    ClickAfter?.Invoke(this, keysEvent);
            }
        }
    }
}