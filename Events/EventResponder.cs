namespace Colin.Core.Events
{
    public class EventResponder
    {
        public readonly string Name;

        public EventResponder Parent;

        public List<EventResponder> Children;

        public EventResponder(string name)
        {
            Name = name;
            Children = new List<EventResponder>();
        }
        /// <summary>
        /// 令其与其的子元素响应事件.
        /// </summary>
        public void Response(IEvent theEvent)
        {
            EventResponder child;
            for (int count = Children.Count - 1 ; count >= 0 ; count--)
            {
                child = Children[count];
                if (!theEvent.Captured && theEvent.Postorder)
                    child.Response(theEvent);
            }
            if (!theEvent.Captured)
                Handle(theEvent);
            for (int count = 0; count < Children.Count; count++)
            {
                child = Children[count];
                if (!theEvent.Captured )
                    child.Response(theEvent);
            }
        }
        public virtual void Handle(IEvent theEvent) { }
        public void Register(EventResponder responder)
        {
            responder.Parent = this;
            Children.Add(responder);
        }
    }
}