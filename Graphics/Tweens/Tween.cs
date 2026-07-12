using System;

namespace Colin.Core.Graphics.Tweens
{
  public abstract class Tween<T, TSelf> : ITween where TSelf : Tween<T, TSelf>
  {
    public T From { get; set; }
    public T To { get; set; }
    public float Duration { get; set; } = 1f;
    public EaseFunction Easing { get; set; } = Ease.Linear;
    public LoopMode Loop { get; set; } = LoopMode.None;
    public bool TimeAffected { get; set; } = false;

    public bool IsPlaying { get; private set; }
    public bool IsComplete { get; private set; }
    public float Progress { get; private set; }
    public T CurrentValue { get; private set; }

    public Action<T> OnUpdate { get; set; }
    public Action OnComplete { get; set; }

    private float _elapsed;
    private bool _registered;

    protected abstract T Lerp(T a, T b, float t);

    public void Update()
    {
      if (!IsPlaying)
        return;

      float dt = TimeAffected ? Time.DeltaTime : Time.UnscaledDeltaTime;
      _elapsed += dt;

      if (Duration <= 0f)
      {
        Progress = 1f;
        CurrentValue = To;
        OnUpdate?.Invoke(CurrentValue);
        Complete();
        return;
      }

      Progress = _elapsed / Duration;
      if (Progress >= 1f)
      {
        Progress = 1f;
        CurrentValue = Lerp(From, To, Easing(1f));
        OnUpdate?.Invoke(CurrentValue);
        Complete();
      }
      else
      {
        CurrentValue = Lerp(From, To, Easing(Progress));
        OnUpdate?.Invoke(CurrentValue);
      }
    }

    private void Complete()
    {
      switch (Loop)
      {
        case LoopMode.None:
          IsPlaying = false;
          IsComplete = true;
          _registered = false;
          OnComplete?.Invoke();
          break;
        case LoopMode.Restart:
          _elapsed = 0f;
          Progress = 0f;
          OnComplete?.Invoke();
          break;
        case LoopMode.PingPong:
          _elapsed = 0f;
          Progress = 0f;
          (From, To) = (To, From);
          OnComplete?.Invoke();
          break;
      }
    }

    void ITween.Play() => Play();

    public TSelf Play()
    {
      IsPlaying = true;
      IsComplete = false;
      _elapsed = 0f;
      Progress = 0f;
      CurrentValue = From;
      if (!_registered)
      {
        TweenManager.Register(this);
        _registered = true;
      }
      return (TSelf)this;
    }

    public void Pause()
    {
      IsPlaying = false;
    }

    public void Stop()
    {
      IsPlaying = false;
      IsComplete = true;
      _elapsed = 0f;
      Progress = 0f;
      _registered = false;
    }

    public void Reset()
    {
      IsPlaying = false;
      IsComplete = false;
      _elapsed = 0f;
      Progress = 0f;
      CurrentValue = From;
      _registered = false;
    }

    // Fluent API
    public TSelf SetFrom(T value) { From = value; return (TSelf)this; }
    public TSelf SetTo(T value) { To = value; return (TSelf)this; }
    public TSelf SetCurrentValue(T value) { CurrentValue = value; return (TSelf)this; }
    public TSelf SetDuration(float duration) { Duration = duration; return (TSelf)this; }
    public TSelf SetEase(EaseFunction ease) { Easing = ease; return (TSelf)this; }
    public TSelf SetLoop(LoopMode mode) { Loop = mode; return (TSelf)this; }
    public TSelf SetTimeAffected(bool affected) { TimeAffected = affected; return (TSelf)this; }
    public TSelf SetOnUpdate(Action<T> callback) { OnUpdate = callback; return (TSelf)this; }
    public TSelf SetOnComplete(Action callback) { OnComplete = callback; return (TSelf)this; }
  }
}
