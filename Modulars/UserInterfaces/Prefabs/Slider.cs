using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
    public class Slider : Division
    {
        public Slider(string name) : base(name) { }

        public Division Block;

        /// <summary>
        /// 指示滑动条的方向.
        /// <br>仅判断 <see cref="Direction.Transverse"/> 与 <see cref="Direction.Portrait"/>.</br>
        /// </summary>
        public Direction Direction = Direction.Portrait;

        public Vector2 Precent;

        private Division Content;
        private Division ContentContainer;
        private Division Response;
        public void Bind(Division content, Division contentContainer, Division response)
        {
            if (Response is not null)
                Response.Events.Hover -= WheelEvent;
            Content = content;
            ContentContainer = contentContainer;
            Response = response;
            Response.Events.Hover += WheelEvent;
        }
        private void WheelEvent()
        {
            if (MouseResponder.ScrollDown)
                Block.Layout.Top -= 2;
            else if (MouseResponder.ScrollUp)
                Block.Layout.Top += 2;
        }
        public override void OnInit()
        {
            if (Block is null)
            {
                Block = new Division("Block");
                Block.BindRenderer<DivPixelRenderer>();
                Block.Design.Color = new Color(255, 223, 135);
                if (Direction is Direction.Portrait)
                {
                    Block.Layout.Width = Layout.Width;
                    Block.Layout.Height = 24;
                }
                if (Direction is Direction.Transverse)
                {
                    Block.Layout.Width = 24;
                    Block.Layout.Height = Layout.Height;
                }
            }
            if (Renderer is null)
            {
                BindRenderer<DivNinecutRenderer>().
                    Bind(Sprite.Get("UserInterfaces/Deltas/Slider")).Cut = new Point(2, 8);
            }
            Block.Interact.IsDraggable = true;
            Register(Block);
            base.OnInit();
        }
        public override void OnUpdate(GameTime time)
        {
            Block.Layout.Left = Math.Clamp(Block.Layout.Left, 0, Layout.Width - Block.Layout.Width);
            Block.Layout.Top = Math.Clamp(Block.Layout.Top, 0, Layout.Height - Block.Layout.Height);

            Precent = Block.Layout.LocationF / (Layout.SizeF - Block.Layout.SizeF);

            if (Content is not null && ContentContainer is not null)
            {
                if (Content.Layout.Width > ContentContainer.Layout.Width)
                    Content.Layout.Left = (int)-(Precent.X * (Content.Layout.Width - ContentContainer.Layout.Width));
                else
                    Content.Layout.Left = 0;

                if (Content.Layout.Height > ContentContainer.Layout.Height)
                    Content.Layout.Top = (int)-(Precent.Y * (Content.Layout.Height - ContentContainer.Layout.Height));
                else
                    Content.Layout.Top = 0;
            }
            base.OnUpdate(time);
        }
    }
}