namespace Colin.Core.Events
{
    public class EventResponder
    {
        public readonly string Name;

        public EventResponder Parent;

        public List<EventResponder> Children;

        public bool Postorder = true;

        public EventResponder(string name)
        {
            Name = name;
            Children = new List<EventResponder>();
        }
        /// <summary>
        /// 令其与其的子元素响应事件.
        /// </summary>
        public void Response(BasicEventArgs theEvent)
        {
            EventResponder child;
            for (int count = Postorder ? Children.Count - 1 : 0; Postorder ? count >= 0 : count < Children.Count; count += Postorder ? -1 : 1)
            {
                child = Children[count];
                if (!theEvent.Captured)
                    child.Response(theEvent);
            }
            if (!theEvent.Captured)
                Handle(theEvent);
        }
        public virtual void Handle(BasicEventArgs theEvent) { }
        public void Register(EventResponder responder)
        {
            responder.Parent = this;
            Children.Add(responder);
        }
    }
}