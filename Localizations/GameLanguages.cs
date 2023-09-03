using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Localizations
{
    public class GameLanguages
    {
        public static Dictionary<string, GameLanguagePackage> Languages = new Dictionary<string, GameLanguagePackage>( );

        private static GameLanguagePackage _current;
        public static GameLanguagePackage Current => _current;

        /// <summary>
        /// 当语言包切换时引发事件.
        /// </summary>
        public static Action OnLanguageChanged;

        public void ChangeLanguage( string language )
        {
            if(Languages.TryGetValue( language, out GameLanguagePackage package ))
            {
                _current = package;
                OnLanguageChanged?.Invoke( );
            }
        }
    }
}