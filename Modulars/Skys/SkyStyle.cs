using Colin.Core.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Modulars.Skys
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
                EngineInfo.SpriteBatch.Draw(SkySprite.Source, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, SkySprite.Depth);
            RenderSky();
        }
        public virtual void RenderSky()
        {

        }
        public void SetSkySprite(Texture2D texture2D) => SkySprite = new Sprite(texture2D);
    }
}