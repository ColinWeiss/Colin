namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 划分元素事件.
    /// </summary>
    public class DivisionEvent : EventArgs
    {
        public string Name;

        public Div Division;

        public DivisionEvent(Div container)
        {
            Division = container;
        }
    }
}