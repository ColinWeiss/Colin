﻿namespace Colin.Core.Modulars.Skys
{
  public class SkyStyle
  {
    public Sprite SkySprite { get; private set; }

    public int Alpha = 255;

    public virtual void DoInitialize()
    {

    }
    public void DoUpdate(GameTime gameTime)
    {
      SkyUpdate(gameTime);
    }
    public virtual void SkyUpdate(GameTime gameTime)
    {

    }
    public void DoRender()
    {
      if (SkySprite != null)
        CoreInfo.Batch.Draw(SkySprite.Source, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, SkySprite.Depth);
      RenderSky();
    }
    public virtual void RenderSky()
    {

    }
    public void SetSkySprite(Texture2D texture2D) => SkySprite = new Sprite(texture2D);
  }
}