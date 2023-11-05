namespace Colin.Core.Localizations
{
    public class GameLanguagePackage
    {
        public Dictionary<string, string> Texts = new Dictionary<string, string>();

        /// <summary>
        /// <br>使用 <see cref="Common.Add"/> 方法将本地化文本硬编码至语言包中.</br>
        /// </summary>
        public virtual void SetDefaultTexts() { }

        /// <summary>
        /// 保存语言包至指定路径.
        /// <br>[!] 语言包文件类型为 JSON.</br>
        /// </summary>
        public void Save( string path )
        {

        }
    }
}