namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
    public class ProgressBar : Div
    {
        public ProgressBar(string name) : base(name)
        {
            Fill = new Div("Fill");
        }

        public Div Fill;

        public float Percentage;

        public Direction Direction;

        public Direction Toward;

        public Point FillOffset;

        public override void DivInit()
        {
            Register(Fill);
            base.DivInit();
        }
        public override void OnUpdate(GameTime time)
        {
            if (Direction == Direction.Portrait)
                Fill.Do(Portrait);
            else if (Direction == Direction.Transverse)
                Fill.Do(Transverse);
            base.OnUpdate(time);
        }
        private void Portrait(Div division)
        {
            if (Toward == Direction.Down)
                division.Layout.Height = (int)(Percentage * Layout.Height);
            else if (Toward == Direction.Up)
            {
                division.Layout.Top = Layout.Height - (int)(Percentage * Layout.Height) + FillOffset.Y;
                division.Layout.Height = (int)(Percentage * Layout.Height);
            }
        }
        private void Transverse(Div division)
        {
            if (Toward == Direction.Right)
                division.Layout.Width = (int)(Percentage * Layout.Width);
            else if (Toward == Direction.Left)
            {
                division.Layout.Left = Layout.Width - (int)(Percentage * Layout.Width) + FillOffset.Y;
                division.Layout.Width = (int)(Percentage * Layout.Width);
            }
        }
    }
}