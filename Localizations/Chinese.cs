namespace Colin.Core.Localizations
{
    public class Chinese : GameLanguagePackage
    {
        public override void SetDefaultTexts()
        {
            Texts.Add("ON", "开");
            Texts.Add("OFF", "关");
            base.SetDefaultTexts();
        }
    }
}