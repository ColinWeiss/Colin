using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.UI
{
    /// <summary>
    /// 表示一个可以显示纹理的按钮.
    /// </summary>
    public abstract class TextureButton : Control, IAnimation
    {
        public abstract int CurrentFrameX { get; }

        public abstract int CurrentFrameY { get; }

        public abstract Rectangle Frame { get; }

        public Texture2D? Texture;

        /// <summary>
        /// 实例化一个可以显示纹理的按钮.
        /// </summary>
        /// <param name="texture">要在其上显示纹理的按钮.</param>
        public TextureButton( Texture2D texture )
        {
            Texture = texture;
        }

        public virtual Rectangle GetFrame( )
        {
            return new Rectangle( CurrentFrameX * Frame.Width, CurrentFrameY * Frame.Height, Frame.Width, Frame.Height );
        }

        protected override void Draw( SpriteBatch spriteBatch )
        {
            spriteBatch.Draw( Texture, Position - RotationCenter, GetFrame( ), ElementColor, Rotation, RotationCenter, Scale, SpriteEffects.None, 1f );
            base.Draw( spriteBatch );
        }
    }
}