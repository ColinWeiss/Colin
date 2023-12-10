
/* 项目“DeltaMachine.Desktop”的未合并的更改
在此之前:
using Colin.Core.Graphics;
using Colin.Core.Assets;
在此之后:
using Colin.Core.Assets;
using Colin.Core.Graphics;
*/
namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
    public class DivPixelRenderer : DivisionRenderer
    {
        private Sprite _pixel;
        public override void RendererInit()
        {
            _pixel = Sprite.Get( "Pixel" );
        }
        public override void DoRender(SpriteBatch batch)
        {
            if (_pixel != null)
                batch.Draw(
                  _pixel.Source,
                  Division.Layout.TotalLocationF + Division.Design.Anchor,
                  Division.Layout.TotalHitBox,
                  Division.Design.Color,
                  Division.Design.Rotation,
                  Division.Design.Anchor,
                  Division.Design.Scale,
                  SpriteEffects.None,
                  _pixel.Depth );
        }
        public DivPixelRenderer SetDesignColor(Color color)
        {
            Division.Design.Color = color;
            return this;
        }
        public DivPixelRenderer SetDesignColor(Color color, int a = 255)
        {
            Division.Design.Color = new Color( color, a );
            return this;
        }
        public DivPixelRenderer SetDesignColor(int r, int g, int b, int a = 255)
        {
            SetDesignColor( new Color( r, g, b, a ) );
            return this;
        }
    }
}