using System;

namespace Colin.Core.Graphics.Tweens
{
  public class Sequence : ITween
  {
    private readonly ITween[] _tweens;
    private int _index;
    private bool _started;

    public bool IsPlaying { get; private set; }
    public bool IsComplete { get; private set; }
    public Action OnComplete { get; set; }

    public Sequence(params ITween[] tweens)
    {
      _tweens = tweens ?? Array.Empty<ITween>();
    }

    public void Update()
    {
      if (!IsPlaying || IsComplete)
        return;

      if (_index >= _tweens.Length)
      {
        IsPlaying = false;
        IsComplete = true;
        OnComplete?.Invoke();
        return;
      }

      var current = _tweens[_index];

      if (!_started)
      {
        current.Play();
        _started = true;
      }

      current.Update();

      if (current.IsComplete)
      {
        _index++;
        _started = false;
        if (_index >= _tweens.Length)
        {
          IsPlaying = false;
          IsComplete = true;
          OnComplete?.Invoke();
        }
      }
    }

    public void Play()
    {
      IsPlaying = true;
      IsComplete = false;
      _index = 0;
      _started = false;
      TweenManager.Register(this);
    }

    public void Pause()
    {
      IsPlaying = false;
      if (_index < _tweens.Length)
        _tweens[_index].Pause();
    }

    public void Stop()
    {
      IsPlaying = false;
      IsComplete = true;
      _index = 0;
      _started = false;
    }

    public void Reset()
    {
      IsPlaying = false;
      IsComplete = false;
      _index = 0;
      _started = false;
      foreach (var t in _tweens)
        t.Reset();
    }
  }
}
