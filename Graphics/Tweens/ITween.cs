using System;

namespace Colin.Core.Graphics.Tweens
{
  public interface ITween
  {
    bool IsPlaying { get; }
    bool IsComplete { get; }
    void Update();
    void Play();
    void Pause();
    void Stop();
    void Reset();
    Action OnComplete { get; set; }
  }
}
