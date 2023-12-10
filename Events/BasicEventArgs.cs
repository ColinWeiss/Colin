namespace Colin.Core.Events
{
    /// <summary>
    /// <br>事件冒泡系统中的事件对象.</br>
    /// </summary>
    public class BasicEventArgs : EventArgs
    {
        public readonly string Name;
        /// <summary>
        /// 指示该事件是否被捕获.
        /// </summary>
        public bool Captured;
        public BasicEventArgs(string name)
        {
            Name = name;
        }
    }
}