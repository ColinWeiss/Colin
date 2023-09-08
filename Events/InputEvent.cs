namespace Colin.Core.Events
{
    public class InputEvent : BasicEvent
    {
        public KeyboardResponder Keyboard { get; set; }

        public MouseResponder Mouse { get; set; }

    }
}