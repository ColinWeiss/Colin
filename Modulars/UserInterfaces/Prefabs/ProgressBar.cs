namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
    public class ProgressBar : Division
    {
        public ProgressBar( string name ) : base( name )
        {
            Fill = new Division( "Fill" );
        }

        public Division Fill;

        public float Percentage;

        public Direction Direction;

        public Direction Toward;

        public Point FillOffset;

        public override void OnInit()
        {
            Register( Fill );
            base.OnInit();
        }
        public override void OnUpdate( GameTime time )
        {
            if(Direction == Direction.Portrait)
                Fill.Do( Portrait );
            else if(Direction == Direction.Transverse)
                Fill.Do( Transverse );
            base.OnUpdate( time );
        }
        private void Portrait( Division division )
        {
            if(Toward == Direction.Down)
                division.Layout.Height = (int)(Percentage * Layout.Height);
            else if(Toward == Direction.Up)
            {
                division.Layout.Top = Layout.Height - (int)(Percentage * Layout.Height) + FillOffset.Y;
                division.Layout.Height = (int)(Percentage * Layout.Height);
            }
        }
        private void Transverse( Division division )
        {
            if(Toward == Direction.Right)
                division.Layout.Width = (int)(Percentage * Layout.Width);
            else if(Toward == Direction.Left)
            {
                division.Layout.Left = Layout.Width - (int)(Percentage * Layout.Width) + FillOffset.Y;
                division.Layout.Width = (int)(Percentage * Layout.Width);
            }
        }
    }
}