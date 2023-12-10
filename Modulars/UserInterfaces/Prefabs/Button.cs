using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
    public class Button : Division
    {
        public Label Label;
        public Button(string name, string text = "") : base(name)
        {
            Label = new Label("ButtonText");
            Label.FontRenderer = Label.BindRenderer<DivFontRenderer>();
            Label.FontRenderer.Font = FontAssets.Get("GlowSans").GetFont(20);
            Label.Design.Color = new Color(255, 223, 135);
            Label.Interact.IsInteractive = false;
            Label.SetText(text);
        }
        public override void OnInit()
        {
            BindRenderer<DivNinecutRenderer>().Bind(Sprite.Get("UserInterfaces/Forms/Button")).Cut = new Point(8, 8);
            Register(Label);
            base.OnInit();
        }
        public override void OnUpdate(GameTime time)
        {
            Label.Layout.Left = Layout.Width / 2 - Label.Layout.Width / 2;
            Label.Layout.Top = Layout.Height / 2 - Label.FontRenderer.Font.LineHeight / 2 - 2;
            base.OnUpdate(time);
        }
    }
}