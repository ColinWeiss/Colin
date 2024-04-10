namespace Colin.Core.Graphics
{
  public class VectorTween
  {
    public Vector2 Default;
    public Vector2 Current;
    public Vector2 Target;
    public void Set(Vector2 vector2)
    {
      Default = vector2;
      Current = vector2;
      Target = vector2;
    }
    public float Time;
    private float _timer;
    private bool _start;
    private float _currentValue;
    public GradientStyle GradientStyle = GradientStyle.Linear;
    public Vector2 Update()
    {
      if (_start)
      {
        _timer += Colin.Core.Time.UnscaledDeltaTime;
        if (_timer <= Time)
        {
          switch (GradientStyle)
          {
            case GradientStyle.Linear:
              _currentValue = _timer / Time;
              break;
            case GradientStyle.EaseOutExpo:
              _currentValue = Easing.EaseOutExpo(_timer / Time);
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