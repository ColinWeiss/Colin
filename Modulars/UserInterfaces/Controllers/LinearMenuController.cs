namespace Colin.Core.Modulars.UserInterfaces.Controllers
{
    public class LinearMenuController : DivisionController
    {
        /// <summary>
        /// 指示项间隔.
        /// </summary>
        public int DivInterval = 0;
        /// <summary>
        /// 方向.
        /// </summary>
        public Direction Direction = Direction.Portrait;
        /// <summary>
        /// 对齐方式.
        /// </summary>
        public Direction Alignment = Direction.Center;
        /// <summary>
        /// 朝向.
        /// </summary>
        public Direction Toward = Direction.Down;
        public Vector2 Scroll;
        public Point TotalSize;
        public LinearMenuController(Division division) : base( division ) { }
        public override void Layout(ref LayoutStyle layout)
        {
            TotalSize = Point.Zero;
            Division.ForEach( CalculateLayout );
            if (Direction == Direction.Portrait)
                Division.ForEach( Portrait );
            else if (Direction == Direction.Transverse)
                Division.ForEach( Transverse );
            layout.Size = TotalSize;
            lastDiv = null;
            base.Layout( ref layout );
        }
        private Division lastDiv;
        private void CalculateLayout(Division division)
        {
            switch (Direction)
            {
                case Direction.Portrait:
                    if (TotalSize.X < division.Layout.Width)
                        TotalSize.X = division.Layout.Width;
                    TotalSize.Y += division.Layout.Height + DivInterval;
                    break;
                case Direction.Transverse:
                    if (TotalSize.Y < division.Layout.Height)
                        TotalSize.Y = division.Layout.Height;
                    TotalSize.X += division.Layout.Width + DivInterval;
                    break;
            }
        }
        private void Portrait(Division division)
        {
            if (lastDiv != null)
            {
                switch (Toward)
                {
                    case Direction.Down:
                        division.Layout.Top = lastDiv.Layout.Top + lastDiv.Layout.Height + DivInterval;
                        break;
                    case Direction.Up:
                        division.Layout.Top = lastDiv.Layout.Top - division.Layout.Height - DivInterval;
                        break;
                }
            }
            else if (Toward == Direction.Up)
                division.Layout.Top = TotalSize.Y - division.Layout.Height;
            else if (Toward == Direction.Down)
                division.Layout.Top = 0;
            switch (Alignment)
            {
                case Direction.Left:
                    division.Layout.Left = (int)Scroll.X;
                    break;
                case Direction.Right:
                    division.Layout.Left = division.Parent.Layout.Width - division.Layout.Width;
                    break;
                case Direction.Center:
                    division.Layout.Left = TotalSize.X / 2 - division.Layout.Width / 2;
                    break;
            }
            lastDiv = division;
        }
        private void Transverse(Division division)
        {
            if (lastDiv != null)
            {
                switch (Toward)
                {
                    case Direction.Right:
                        division.Layout.Left = lastDiv.Layout.Left + lastDiv.Layout.Width + DivInterval;
                        break;
                    case Direction.Left:
                        division.Layout.Left = lastDiv.Layout.Left - division.Layout.Width - DivInterval;
                        break;
                }
            }
            else if (Toward == Direction.Left)
                division.Layout.Left = Division.Layout.Width - division.Layout.Width;
            switch (Alignment)
            {
                case Direction.Up:
                    division.Layout.Top = (int)Scroll.Y;
                    break;
                case Direction.Down:
                    division.Layout.Top = division.Parent.Layout.Height - division.Layout.Height;
                    break;
                case Direction.Center:
                    division.Layout.Top = Division.Layout.Height / 2 - division.Layout.Height / 2;
                    break;
            }
            lastDiv = division;
        }
    }
}