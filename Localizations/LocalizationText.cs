namespace Colin.Core.Localizations
{
    public class LocalizationText
    {
        public readonly string Name;
        private string _value;
        public string Value => _value;
        public LocalizationText(string name)
        {
            Name = name;
            GameLanguages.OnLanguageChanged += () =>
            {
                if (GameLanguages.Current.Texts.TryGetValue( Name, out string value ))
                    _value = value;
                else
                    _value = "[文本错误]";
            };
        }
    }
}
