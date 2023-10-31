using Colin.Core.Modulars.UserInterfaces.Renderers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
    public class Slider : Division
    {
        public Slider( string name ) : base( name ) { }

        public Division Block;

        /// <summary>
        /// 指示滑动条的方向.
        /// <br>仅判断 <see cref="Direction.Transverse"/> 与 <see cref="Direction.Portrait"/>.</br>
        /// </summary>
        public Direction Direction = Direction.Portrait;

        public override void OnInit()
        {
            if( Block is null )
            {
                Block = new Division( "Block" );
                Block.BindRenderer<DivPixelRenderer>();
                if(Direction is Direction.Portrait)
                {
                    Block.Layout.Width = Layout.Width;
                    Block.Layout.Height = 24;
                }
                if(Direction is Direction.Transverse)
                {
                    Block.Layout.Width = 24;
                    Block.Layout.Height = Layout.Height;
                }
            }
            if( Renderer is null )
            {
                BindRenderer<DivPixelRenderer>();
                Design.Color = Color.Gray;
            }
            Block.Interact.IsDraggable = true;
            Register( Block );
            base.OnInit();
        }
        public override void OnUpdate( GameTime time )
        {
            Block.Layout.Left = Math.Clamp( Block.Layout.Left , 0 , Layout.Width - Block.Layout.Width );
            Block.Layout.Top = Math.Clamp( Block.Layout.Top, 0, Layout.Height - Block.Layout.Height );
            base.OnUpdate( time );
        }
    }
}