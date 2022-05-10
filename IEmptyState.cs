namespace Colin
{
    /// <summary>
    /// 表示一个可以使用非空状态进行判断的对象.
    /// </summary>
    public interface IEmptyState
    {
        /// <summary>
        /// 指示对象的非空状态.
        /// </summary>
        bool Empty { get; set; }
    }
}