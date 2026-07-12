using System;

namespace Colin.Core.Graphics.Tweens
{
  public class Parallel : ITween
  {
    private readonly ITween[] _tweens;

    public bool IsPlaying { get; private set; }
    public bool IsComplete { get; private set; }
    public Action OnComplete { get; set; }

    public Parallel(params ITween[] tweens)
    {
      _tweens = tweens ?? Array.Empty<ITween>();
    }

    public void Update()
    {
      if (!IsPlaying || IsComplete)
        return;

      bool allComplete = true;
      for (int i = 0; i < _tweens.Length; i++)
      {
        _tweens[i].Update();
        if (!_tweens[i].IsComplete)
          allComplete = false;
      }

      if (allComplete && _tweens.Length > 0)
      {
        IsPlaying = false;
        IsComplete = true;
        OnComplete?.Invoke();
      }
    }

    public void Play()
    {
      IsPlaying = true;
      IsComplete = false;
      for (int i = 0; i < _tweens.Length; i++)
        _tweens[i].Play();
      TweenManager.Register(this);
    }

    public void Pause()
    {
      IsPlaying = false;
      for (int i = 0; i < _tweens.Length; i++)
        _tweens[i].Pause();
    }

    public void Stop()
    {
      IsPlaying = false;
      IsComplete = true;
      for (int i = 0; i < _tweens.Length; i++)
        _tweens[i].Stop();
    }

    public void Reset()
    {
      IsPlaying = false;
      IsComplete = false;
      for (int i = 0; i < _tweens.Length; i++)
        _tweens[i].Reset();
    }
  }
}
