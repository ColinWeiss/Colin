namespace Colin.Core.Events
{
    public class MouseEventResponder : EventResponder
    {
        public MouseEventResponder(string name) : base( name ) { }
        public EventHandler<MouseEventArgs> Hover;
        public EventHandler<MouseEventArgs> LeftClickBefore;
        public EventHandler<MouseEventArgs> LeftDown;
        public EventHandler<MouseEventArgs> LeftClickAfter;
        public EventHandler<MouseEventArgs> LeftUp;

        public override void Handle(BasicEventArgs theEvent)
        {
            if (theEvent is MouseEventArgs mouseEvent)
            {
                Hover?.Invoke( this, mouseEvent );
                if (MouseResponder.LeftClickBefore)
                    LeftClickBefore?.Invoke( this, mouseEvent );
                if (MouseResponder.LeftDown)
                    LeftDown?.Invoke( this, mouseEvent );
                if (MouseResponder.LeftClickAfter)
                    LeftClickAfter?.Invoke( this, mouseEvent );
                if (MouseResponder.LeftUp)
                    LeftUp?.Invoke( this, mouseEvent );
            }
        }
    }
}