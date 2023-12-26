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
                Division.Layout.SizeF = Font.MeasureString(_text);
                Division.Design.Anchor = Division.Layout.HalfF;
            }
        }
        public override void RendererInit()
        {
            if (Font == null)
                Font = FontAssets.Get("Unifont").GetFont(16);
        }
        public override void DoRender(GraphicsDevice device, SpriteBatch batch)
        {
            batch.DrawString(Font, _text, Division.Layout.TotalLocationF + Division.Design.Anchor,
                Division.Design.Color, Division.Design.Rotation, Division.Design.Anchor, Division.Design.Scale,
                1f, 0f, 0f, TextStyle.None, FontSystemEffect.Stroked, 1);
        }
    }
}