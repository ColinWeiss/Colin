namespace Colin.Core.Preparation
{
    /// <summary>
    /// 表示一个预工作项.
    /// </summary>
    public interface IPreExecution
    {
        /// <summary>
        /// 指示该预工作项的名称.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 指示该预工作项的当前进度.
        /// </summary>
        public float Progress { get; set; }

        public void Prepare() { }
    }
}