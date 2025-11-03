
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
  public class DivPixelRenderer : DivRenderer
  {
    private Sprite _pixel;
    public override void OnDivInitialize()
    {
      _pixel = Sprite.Get("Pixel");
    }
    public override void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      if (_pixel != null)
        batch.Draw(
          _pixel.Source,
          Div.Layout.RenderTargetLocation + div.Layout.Anchor,
          new Rectangle(0, 0, (int)div.Layout.Width, (int)div.Layout.Height),
          Div.Design.Color,
          Div.Layout.Rotation,
          div.Layout.Anchor,
          Div.Layout.Scale,
          SpriteEffects.None,
          _pixel.Depth);
    }
    public DivPixelRenderer SetDesignColor(Color color)
    {
      Div.Design.Color = color;
      return this;
    }
    public DivPixelRenderer SetDesignColor(Color color, int a = 255)
    {
      Div.Design.Color = new Color(color, a);
      return this;
    }
    public DivPixelRenderer SetDesignColor(int r, int g, int b, int a = 255)
    {
      SetDesignColor(new Color(r, g, b, a));
      return this;
    }
  }
}