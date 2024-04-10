using Microsoft.Xna.Framework.Audio;

namespace Colin.Core.Audios
{
  public class GameSoundEffect<T> where T : SoundEffectInstance
  {
    public T SoundInstance;
    public float Time;
    public float Timer;
    public bool Stoped;
    public bool Gradually;
    public float GraduallyTime;
    public float Volume;

    public void FromInstance(SoundEffectInstance soundEffectInstance) => SoundInstance = (T)soundEffectInstance;

    public void DoUpdate()
    {
      Timer += Core.Time.DeltaTime;
      if (Timer >= Time && Stoped is false)
      {
        SoundInstance.Play();
        Timer = Timer - Time;
      }

      if (Stoped is true)
        Volume -= Core.Time.DeltaTime / GraduallyTime;
      else
        Volume += Core.Time.DeltaTime / GraduallyTime;

      Volume = Math.Clamp(Volume, 0, 1);
      SoundInstance.Volume = Volume;
    }
    public void Stop()
    {
      Stoped = true;
      Timer = 0;
      SoundInstance.Stop(true);
    }
    public void Play()
    {
      Stoped = false;
    }
  }
}