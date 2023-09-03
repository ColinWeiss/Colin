using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Localizations
{
    public class GameLanguagePackage
    {
        public Dictionary<string, string> Common = new Dictionary<string, string>( );

        /// <summary>
        /// 该方法平常不会被调用.
        /// <br>[!] 但你可以使用 <see cref="Common.Add"/> 方法将本地化文本硬编码至语言包中.</br>
        /// </summary>
        public virtual void SetDefaultTexts( )
        {

        }
    }
}