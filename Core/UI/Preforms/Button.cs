using Colin.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.UI.Preforms
{
    /// <summary>
    /// 表示一个按钮.
    /// </summary>
    public class Button : Container
    {
        PrimitiveBatch PrimitiveBatch;
        protected override void InitializeContainer( )
        {
            PrimitiveBatch = new PrimitiveBatch( 4 );
            base.InitializeContainer( );
        }
        protected override void SetLayerout( ref ContainerElement containerElement )
        {
            containerElement.SetSize( 100 , 50 );
            base.SetLayerout( ref containerElement );
        }
        protected override void DrawSelf( )
        {
            PrimitiveBatch.DrawRectangle( Location , Size , Color.White );
            base.DrawSelf( );
        }
    }
}