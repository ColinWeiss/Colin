namespace Colin.Core.Graphics
{
    public class ColorTween
    {
        public Color Default;
        public Color Current;
        public Color Target;
        public void Set(Color color)
        {
            Default = color;
            Current = color;
            Target = color;
        }
        public float Time;
        private float _timer;
        private bool _start;
        private float _currentValue;
        public GradientStyle GradientStyle = GradientStyle.Linear;
        public Color Update()
        {
            if (_start)
            {
                _timer += Core.Time.UnscaledDeltaTime;
                if (_timer <= Time)
                {
                    switch (GradientStyle)
                    {
                        case GradientStyle.Linear:
                            _currentValue = _timer / Time;
                            break;
                        case GradientStyle.EaseOutExpo:
                            _currentValue = 1f - MathF.Pow(2, -10 * _timer / Time);
                            break;
                    };
                    Current.Closer(Target, _currentValue, 1f);
                }
            }
            if (_timer > Time)
            {
                Current = Target;
                _start = false;
                _timer = 0;
            }
            return Current;
        }
        public void Start()
        {
            Current = Default;
            _timer = 0;
            _start = true;
        }
        public void Stop()
        {
            _start = false;
            _timer = 0;
        }
    }
}