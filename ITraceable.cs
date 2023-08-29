namespace Colin.Core
{
    /// <summary>
    /// 表示一个可被溯源的对象.
    /// </summary>
    public interface ITraceable
    {
        /// <summary>
        /// 对象名称.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 对象显示名称.
        /// </summary>
        public string DisplayName { get; }

    }
}