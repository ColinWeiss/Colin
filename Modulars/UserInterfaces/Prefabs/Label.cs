using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
  public class Label : Div
  {
    public Label(string name) : base(name) { }
    public DivFontRenderer FontRenderer;
    public override void DivInit()
    {
      if (FontRenderer == null)
        FontRenderer = BindRenderer<DivFontRenderer>();
      base.DivInit();
    }
    public void SetText(string text)
    {
      if (FontRenderer == null)
        FontRenderer = BindRenderer<DivFontRenderer>();
      FontRenderer.Text = text;
    }
  }
}