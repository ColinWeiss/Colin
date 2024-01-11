namespace Colin.Core.Localizations
{
    public class Chinese : GameLanguagePackage
    {
        public override void SetDefaultTexts()
        {
            Texts.Add("Normal.Interactive.ON", "开");
            Texts.Add("Normal.Interactive.OFF", "关");
            Texts.Add("Normal.Interactive.Back", "返回");
            Texts.Add("Normal.Interactive.Exit", "退出");
            Texts.Add("Normal.Interactive.Load", "加载");
            Texts.Add("Normal.Interactive.Save", "保存");
            Texts.Add("Normal.Interactive.Delete", "删除");

            base.SetDefaultTexts();
        }
    }
}