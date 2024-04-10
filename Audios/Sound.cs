using Microsoft.Xna.Framework.Audio;

namespace Colin.Core.Audios
{
  /// <summary>
  /// 声音模块.
  /// </summary>
  public class Sound : ISceneModule
  {
    public bool Enable { get; set; } = true;

    public Scene Scene { get; set; }

    public void DoInitialize()
    {

    }
    public void Start()
    {

    }

    public void DoUpdate(GameTime time)
    {

    }

    /// <summary>
    /// 播放指定音效.
    /// </summary>
    /// <param name="soundEffect">音效.</param>
    public void Play(SoundEffect soundEffect, bool useInstance = false)
    {
      if (EngineInfo.Config.SoundEffect)
      {
        if (useInstance)
        {
          //TODO: 找到使用 SoundEffectInstance 不爆炸的办法.
          SoundEffectInstance _instance = soundEffect?.CreateInstance();
          if (_instance != null && !soundEffect.IsDisposed && !_instance.IsDisposed)
          {
            _instance.Volume = EngineInfo.Config.SoundEffectVolume;
            _instance.Play();
          }
        }
        else if (!soundEffect.IsDisposed)
          soundEffect?.Play();
      }
    }

    /// <summary>
    /// 播放指定音效.
    /// </summary>
    /// <param name="soundEffect">音效.</param>
    public void PlayInstance(SoundEffectInstance instance)
    {
      if (EngineInfo.Config.SoundEffect)
      {
        instance.Volume = EngineInfo.Config.SoundEffectVolume;
        instance.Play();
      }
    }

    public void Dispose()
    {
    }
  }
}