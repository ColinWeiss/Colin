using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Extensions
{
    public static class GraphicExtensions
    {
        /// <summary>
        /// 九片式绘制
        /// </summary>
        /// <param name="spriteBatch">纹理绘制管道.</param>
        /// <param name="image">纹理.</param>
        /// <param name="x">绘制纹理左上角的 X 坐标.</param>
        /// <param name="y">绘制纹理左上角的 Y 坐标.</param>
        /// <param name="width">宽度.</param>
        /// <param name="height">高度.</param>
        /// <param name="borderSize">裁区范围.</param>
        public static void NinePiece( this SpriteBatch spriteBatch, Texture2D image, int x, int y, int width, int height, int borderSize )
        {
            spriteBatch.NinePiece( image, x, y, width, height, borderSize, Color.White );
        }

        /// <summary>
        /// 九片式绘制
        /// </summary>
        /// <param name="spriteBatch">纹理绘制管道.</param>
        /// <param name="image">纹理.</param>
        /// <param name="x">绘制纹理左上角的 X 坐标.</param>
        /// <param name="y">绘制纹理左上角的 Y 坐标.</param>
        /// <param name="width">宽度.</param>
        /// <param name="height">高度.</param>
        /// <param name="borderSize">裁区范围.</param>
        /// <param name="color">颜色.</param>
        public static void NinePiece( this SpriteBatch spriteBatch, Texture2D image, int x, int y, int width, int height, int borderSize, Color color )
        {
            Vector2 rightTopStartPoting = new Vector2( x + width - borderSize, y );
            Vector2 leftBottomStartPoting = new Vector2( x, y + height - borderSize );
            Vector2 rightBottomStartPoting = new Vector2( x + width - borderSize, y + height - borderSize );
            Rectangle rightTopIntercept = new Rectangle( image.Width - borderSize, 0, borderSize, borderSize );
            Rectangle leftBottomIntercept = new Rectangle( 0, image.Height - borderSize, borderSize, borderSize );
            Rectangle rightBottomIntercept = new Rectangle( image.Width - borderSize, image.Height - borderSize, borderSize, borderSize );
            SpriteBatch.Draw( image, new Vector2( x, y ), new Rectangle( 0, 0, borderSize, borderSize ), color );
            SpriteBatch.Draw( image, rightTopStartPoting, rightTopIntercept, color );
            SpriteBatch.Draw( image, leftBottomStartPoting, leftBottomIntercept, color );
            SpriteBatch.Draw( image, rightBottomStartPoting, rightBottomIntercept, color );
            SpriteBatch.Draw( image, new Rectangle( x + borderSize, y, width - borderSize * 2, borderSize ), new Rectangle( borderSize, 0, 2, borderSize ), color );
            SpriteBatch.Draw( image, new Rectangle( x + width - borderSize, y + borderSize, borderSize, height - borderSize * 2 ), new Rectangle( image.Width - borderSize, borderSize, borderSize, 2 ), color );
            SpriteBatch.Draw( image, new Rectangle( x + borderSize, y + height - borderSize, width - borderSize * 2, borderSize ), new Rectangle( borderSize, image.Height - borderSize, 2, borderSize ), color );
            SpriteBatch.Draw( image, new Rectangle( x, y + borderSize, borderSize, height - borderSize * 2 ), new Rectangle( 0, borderSize, borderSize, 2 ), color );
            SpriteBatch.Draw( image, new Rectangle( x + borderSize, y + borderSize, width - borderSize * 2, height - borderSize * 2 ), new Rectangle( borderSize, borderSize, 2, 2 ), color );
        }

    }
}