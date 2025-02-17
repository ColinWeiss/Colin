namespace Colin.Core.Graphics.Tweens
{
  public abstract class Tweener<T>
  {
    public T Start;
    public T Target;
    public T Current;
    public void Set(T defaultT)
    {
      Start = defaultT;
      Current = defaultT;
      Target = defaultT;
    }
    public bool IsLoop;
    private bool _isPlay;
    public bool IsPlay => _isPlay;
    public float Time;
    private float _timer;

    private float _percentage;
    public float Percentage => _percentage;

    public bool TimeAffected = false;

    public GradientStyle GradientStyle = GradientStyle.Linear;
    public T DoUpdate()
    {
      if (_isPlay)
      {
        _timer += TimeAffected ? Colin.Core.Time.DeltaTime : Colin.Core.Time.UnscaledDeltaTime;
      }
      switch (GradientStyle)
      {
        case GradientStyle.Linear:
          _percentage = _timer / Time;
          break;
        case GradientStyle.EaseOutExpo:
          _percentage = 1f - MathF.Pow(2, -10 * _timer / Time);
          break;
      };
      Current = Calculate();
      if (_timer > Time)
      {
        Current = Target;
        _isPlay = IsLoop;
      }
      return Current;
    }
    /// <summary>
    /// 用于计算每帧缓动过程中当前值的变化.
    /// </summary>
    /// <returns></returns>
    public abstract T Calculate();

    public void Play()
    {
      _isPlay = true;
      _timer = 0;
    }
    public void Pause()
    {
      _isPlay = false;
    }
    public void Stop()
    {
      _isPlay = false;
      _timer = 0;
    }
  }
}