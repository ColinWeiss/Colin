namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
  public class DivTextureRenderer : DivRenderer
  {
    private Sprite _sprite;
    public Sprite Sprite => _sprite;

    public override void OnDivInitialize() { }

    public override void RenderStep(GraphicsDevice device, SpriteBatch batch)
    {
      if (_sprite is not null)
      {
        if (Adaptive is false)
        {
          Frame currentFrame = _sprite.Frame;
          batch.Draw(
            _sprite.Source,
            Div.Layout.RenderTargetLocation + div.Layout.Anchor,
            currentFrame.GetFrame(),
            Div.Design.Color,
            Div.Layout.Rotation,
            div.Layout.Anchor,
            Div.Layout.Scale,
            SpriteEffects.None,
            _sprite.Depth);
        }
        else
        {
          Vector2 t = Div.Layout.RenderTargetLocation + div.Layout.Anchor;
          batch.Draw(
            _sprite.Source,
            new Rectangle(t.ToPoint(), div.Layout.SizeP),
            new Rectangle(0, 0, _sprite.Width, _sprite.Height),
            Div.Design.Color,
            Div.Layout.Rotation,
            div.Layout.Anchor,
            SpriteEffects.None,
            _sprite.Depth);
        }
      }
    }

    public DivTextureRenderer Bind(Sprite sprite)
    {
      _sprite = sprite;
      return this;
    }
    public DivTextureRenderer Bind(Texture2D texture)
    {
      _sprite = new Sprite(texture);
      return this;
    }
    public DivTextureRenderer Bind(string path)
    {
      _sprite = Sprite.Get(path);
      return this;
    }
    public DivTextureRenderer Bind(params string[] path)
    {
      _sprite = Sprite.Get(path);
      return this;
    }
  }
}