﻿namespace Colin.Core.Modulars.UserInterfaces.Renderers
{
    public class DivTextureRenderer : DivisionRenderer
    {
        private Sprite _sprite;
        public Sprite Sprite => _sprite;
        public override void RendererInit() { }
        public override void DoRender(GraphicsDevice device, SpriteBatch batch)
        {
            if (_sprite is not null)
            {
                Frame currentFrame = _sprite.Frame;
                batch.Draw(
                  _sprite.Source,
                  Division.Layout.TotalLocationF + Division.Design.Anchor
                  + Division.Layout.HalfF - currentFrame.HalfF,
                  currentFrame.GetFrame(), Division.Design.Color,
                  Division.Design.Rotation,
                  Division.Design.Anchor,
                  Division.Design.Scale,
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