using Colin.Core.Extensions;

namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
    public class DivNinecutRenderer : DivisionRenderer
    {
        private Sprite _sprite;
        public Sprite Sprite => _sprite;
        public Point Cut;
        public override void RendererInit() { }
        public override void DoRender( SpriteBatch batch )
        {
            batch.DrawNineCut(
                _sprite.Source,
                Division.Design.Color,
                Division.Layout.TotalLocation,
                Division.Layout.Size,
                Cut,
                _sprite.Depth );
        }
        public DivNinecutRenderer Bind( Sprite sprite )
        {
            _sprite = sprite;
            return this;
        }
        public DivNinecutRenderer Bind( Texture2D texture )
        {
            _sprite = new Sprite( texture );
            return this;
        }
    }
}