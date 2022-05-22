using Colin.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.UI.Preforms
{
    /// <summary>
    /// 一块可拖动的底板, 九片式绘制.
    /// <para>[!] 若你想关闭它的拖动功能, </para>
    /// <para>[!] 请在||| 初始化后 |||将 <seealso cref="ContainerEvents.Drop"/> 设置为 <see href="false"/>.</para>
    /// </summary>
    public class Panel : Container
    {
        private int _borderSize;
        public Texture2D Image { get; private set; }
        public Panel( Texture2D img, int borderSize )
        {
            Image = img;
            _borderSize = borderSize;
        }

        protected override void InitializeContainer( )
        {
            Events.Drop = true;
            Events.CanGetForPointer = false;
            base.InitializeContainer( );
        }

        protected override void DrawSelf( )
        {
            HardwareInfo.SpriteBatch.End( );
            RasterizerState OverflowHiddenRasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                ScissorTestEnable = true
            };
            Rectangle clippingRectangle = BaseRectangle;
            HardwareInfo.SpriteBatch.GraphicsDevice.ScissorRectangle = clippingRectangle;
            HardwareInfo.SpriteBatch.GraphicsDevice.RasterizerState = OverflowHiddenRasterizerState;
            HardwareInfo.SpriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, OverflowHiddenRasterizerState, null );
            if ( ScaleFunction != null )
                HardwareInfo.SpriteBatch.NinePiece( Image, (int)Location.X, (int)Location.Y, (int)( Size.X * ScaleFunction.Scale ), (int)( Size.Y * ScaleFunction.Scale ), _borderSize );
            base.DrawSelf( );
            HardwareInfo.SpriteBatch.End( );
            HardwareInfo.SpriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap );
        }
    }
}