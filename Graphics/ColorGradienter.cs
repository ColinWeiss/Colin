using Colin.Core.Extensions;

namespace Colin.Core.Graphics
{
    public class ColorGradienter
    {
        public Color Default;
        public Color Current;
        public Color Target;
        public void Set( Color color )
        {
            Default = color;
            Current = color;
            Target = color;
        }
        public float Time;
        public float Timer;
        private bool _start;
        private float _currentValue;
        public GradientStyle GradientStyle = GradientStyle.Linear;
        public Color Update()
        {
            if(_start)
            {
                Timer += Colin.Core.Time.UnscaledDeltaTime;
                if(Timer <= Time)
                {
                    switch(GradientStyle)
                    {
                        case GradientStyle.Linear:
                            _currentValue = Timer / Time;
                            break;
                        case GradientStyle.EaseOutExpo:
                            _currentValue = 1f - MathF.Pow( 2, -10 * Timer / Time );
                            break;
                    };
                    Current.Closer( Target, _currentValue, 1f );
                }
            }
            if(Timer > Time)
            {
                Current = Target;
                _start = false;
                Timer = 0;
            }
            return Current;
        }
        public void Start()
        {
            Current = Default;
            Timer = 0;
            _start = true;
        }
        public void Stop()
        {
            _start = false;
            Timer = 0;
        }
    }
}