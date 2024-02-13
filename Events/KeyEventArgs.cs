namespace Colin.Core.Events
{
    public class KeyEventArgs : IEvent
    {
        public Keys Key;
        public bool ClickBefore;
        public bool Down;
        public bool ClickAfter;
        public bool Captured { get; set; }
        public string Name { get; set; }
        public bool Postorder { get; set; }

        public KeyEventArgs(string name)
        {
            Name = name;
            Postorder = true;
        }
    }
}