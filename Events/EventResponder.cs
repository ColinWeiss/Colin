using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Events
{
    public class EventResponder
    {
        public readonly string Name;

        public EventResponder Parent;

        public List<EventResponder> Children;

        public EventResponder( string name )
        {
            Name = name;
            Children = new List<EventResponder>();
        }
        /// <summary>
        /// 令其与其的子元素响应事件.
        /// </summary>
        public void Response( BasicEventArgs theEvent )
        {
            EventResponder child;
            for(int count = Children.Count - 1; count >= 0; count--)
            {
                child = Children[count];
                if(!theEvent.Captured)
                    child.Response( theEvent );
            }
            if(!theEvent.Captured)
                Handle( theEvent );
        }
        public virtual void Handle( BasicEventArgs theEvent ) { }
        public void Register( EventResponder responder )
        {
            responder.Parent = this;
            Children.Add( responder );
        }
    }
}