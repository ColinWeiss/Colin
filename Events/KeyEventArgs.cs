namespace Colin.Core.Events
{
    public class KeyEventArgs : BasicEventArgs
    {
        public Keys Key;
        public bool ClickBefore;
        public bool Down;
        public bool ClickAfter;
        public KeyEventArgs(string name) : base( name ) { }
    }
}