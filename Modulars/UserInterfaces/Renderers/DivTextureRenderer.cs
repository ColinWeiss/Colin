namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
    public class DivTextureRenderer : DivisionRenderer
    {
        private Sprite _sprite;
        public Sprite Sprite => _sprite;
        public override void OnDivInitialize() { }
        public override void DoRender(GraphicsDevice device, SpriteBatch batch)
        {
            if (_sprite is not null)
            {
                Frame currentFrame = _sprite.Frame;
                batch.Draw(
                  _sprite.Source,
                  Div.Layout.RenderTargetLocation
                  + Div.Layout.Half - currentFrame.HalfF,
                  currentFrame.GetFrame(), Div.Design.Color,
                  Div.Layout.Rotation,
                  Vector2.Zero,
                  Div.Layout.Scale,
                  SpriteEffects.None,
                  _sprite.Depth);
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
    }
}