using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
  public class Button : Div
  {
    public Label Label;
    public Button(string name, string text = "") : base(name)
    {
      Label = new Label("ButtonText");
      Label.FontRenderer = Label.BindRenderer<DivFontRenderer>();
      Label.FontRenderer.Font = Asset.GetFont("GlowSansMedium").GetFont(20);
      Label.Design.Color = Color.Black;
      Label.Interact.IsInteractive = false;
      Label.SetText(text);
    }
    public override void DivInit()
    {
      if(Renderer is null)
        BindRenderer<DivPixelRenderer>();
      Register(Label);
      base.DivInit();
    }
    public override void OnUpdate(GameTime time)
    {
      Label.Layout.Left = Layout.Width / 2 - Label.Layout.Width / 2;
      Label.Layout.Top = Layout.Height / 2 - Label.FontRenderer.Font.LineHeight / 2 - 2;
      base.OnUpdate(time);
    }
  }
}