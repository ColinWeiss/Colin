namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
  public class DivNinecutRenderer : DivRenderer
  {
    private Sprite _sprite;
    public Sprite Sprite => _sprite;
    public Point Cut;
    public override void OnDivInitialize() { }
    public override void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      batch.DrawNineCut(
          _sprite.Source,
          Div.Design.Color,
          Div.Layout.RenderTargetLocation,
          Div.Layout.Size,
          Cut,
          _sprite.Depth);
    }
    public DivNinecutRenderer Bind(Sprite sprite)
    {
      _sprite = sprite;
      return this;
    }
    public DivNinecutRenderer Bind(Texture2D texture)
    {
      _sprite = new Sprite(texture);
      return this;
    }
  }
}