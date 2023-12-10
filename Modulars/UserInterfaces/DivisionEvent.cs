namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 划分元素事件.
    /// </summary>
    public class DivisionEvent : EventArgs
    {
        public string Name;

        public Division Division;

        public DivisionEvent(Division container)
        {
            Division = container;
        }
    }
}