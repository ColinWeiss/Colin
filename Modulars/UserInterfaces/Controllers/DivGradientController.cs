namespace Colin.Core.Modulars.UserInterfaces.Controllers
{
    public class DivGradientController : DivisionController
    {
        public DivGradientController(Division div) : base(div) { }

        private bool _openState = false;
        private bool _closeState = false;

        public ColorGradienter OpenColor;
        public ColorGradienter CloseColor;

        public VectorGradienter OpenScale;
        public VectorGradienter CloseScale;

        public event Action OnClosed;

        public override void OnInit()
        {
            OpenColor = new ColorGradienter();
            OpenColor.Set(Color.Transparent);
            OpenColor.Target = new Color(255, 255, 255, 255);
            OpenColor.Time = 0.08f;

            CloseColor = new ColorGradienter();
            CloseColor.Set(Color.White);
            CloseColor.Target = Color.Transparent;
            CloseColor.Time = 0.12f;

            OpenScale = new VectorGradienter();
            OpenScale.GradientStyle = GradientStyle.EaseOutExpo;
            OpenScale.Set(Vector2.One * 0.7f);
            OpenScale.Target = Vector2.One;
            OpenScale.Time = 0.4f;

            CloseScale = new VectorGradienter();
            CloseScale.GradientStyle = GradientStyle.EaseOutExpo;
            CloseScale.Set(Vector2.One);
            CloseScale.Target = Vector2.One * 0.7f;
            CloseScale.Time = 2f;

            base.OnInit();
        }
        public override void Design(ref DesignStyle design)
        {
            if (_openState)
            {
                design.Color = OpenColor.Update();
                design.Scale = OpenScale.Update();
            }
            if (_closeState)
            {
                design.Color = CloseColor.Update();
                design.Scale = CloseScale.Update();
                if (design.Color.A <= 0)
                {
                    Division.IsVisible = false;
                    OnClosed?.Invoke();
                }
            }
            base.Design(ref design);
        }
        public void Open()
        {
            if (!Division.IsVisible)
            {
                OpenColor.Start();
                OpenScale.Start();
                _openState = true;
                _closeState = false;
                Division.IsVisible = true;
            }
        }
        public void Close()
        {
            CloseColor.Start();
            CloseScale.Start();
            _closeState = true;
            _openState = false;
        }
    }
}