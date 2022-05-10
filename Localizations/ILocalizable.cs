namespace Colin.Localizations
{
    /// <summary>
    /// 表示一个可被本地化资源文件进行翻译的对象.
    /// </summary>
    public interface ILocalizable
    {
        /// <summary>
        /// 获取该对象的名称文本.
        /// </summary>
        /// <returns>对象名称文本.</returns>
        string? GetName { get; }
        /// <summary>
        /// 获取该对象的介绍文本.
        /// </summary>
        /// <returns>对象的介绍文本.</returns>
        string? GetInformation { get; }
    }
}