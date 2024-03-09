using FontStashSharp;

namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
    public class DivFontRenderer : DivisionRenderer
    {
        public DynamicSpriteFont Font;

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                Div.Layout.SetSize(Font.MeasureString(_text));
                Div.Layout.Anchor = Div.Layout.Half;
            }
        }
        public override void OnBinded()
        {
            if (Font == null)
                Font = FontAssets.Get("Unifont").GetFont(16);
            base.OnBinded();
        }
        public override void DoRender(GraphicsDevice device, SpriteBatch batch)
        {
            batch.DrawString(
                Font, 
                _text, 
                Div.Layout.RenderTargetLocation + Div.Layout.Anchor,
                Div.Design.Color, 
                Div.Layout.Rotation, 
                Div.Layout.Anchor, 
                Div.Layout.Scale,
                1f, 0f, 0f, TextStyle.None, FontSystemEffect.Stroked, 1);
        }
    }
}