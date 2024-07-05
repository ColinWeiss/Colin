using Colin.Core.IO;
using System.Text.Json;

namespace Colin.Core
{
  /// <summary>
  /// 程序设置.
  /// </summary>
  public sealed class Config
  {
    /// <summary>
    /// 指示游戏配置文件的位置及其本身.
    /// <br>包含文件扩展名.</br>
    /// </summary>
    public static string ConfigPath => Path.Combine(BasicsDirectory.ProgramDir, "Configs.json");

    /// <summary>
    /// 指示是否全屏.
    /// </summary>
    public bool IsFullScreen
    {
      get
      {
        return CoreInfo.Graphics.IsFullScreen;
      }
      set
      {
        CoreInfo.Graphics.IsFullScreen = value;
        CoreInfo.Graphics.ApplyChanges();
      }
    }

    /// <summary>
    /// 指示是否启用音效.
    /// </summary>
    public bool SoundEffect { get; set; } = true;

    /// <summary>
    /// 指示音效音量百分比.
    /// </summary>
    public float SoundEffectVolume { get; set; } = 1f;

    /// <summary>
    /// 指示程序目标帧率.
    /// </summary>
    public int TargetFrame
    {
      get
      {
        return CoreInfo.Engine.TargetFrame;
      }
      set
      {
        CoreInfo.Engine.TargetFrame = value;
      }
    }

    /// <summary>
    /// 指示图形质量.
    /// </summary>
    public PictureQuality PictureQuality { get; set; }

    /// <summary>
    /// 指示鼠标是否可见.
    /// </summary>
    public bool IsMouseVisiable
    {
      get
      {
        return CoreInfo.Engine.IsMouseVisible;
      }
      set
      {
        CoreInfo.Engine.IsMouseVisible = value;
      }
    }

    public void Load()
    {
      if (File.Exists(ConfigPath))
      {
        Config result = JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigPath));
        IsFullScreen = result.IsFullScreen;
        SoundEffect = result.SoundEffect;
        SoundEffectVolume = result.SoundEffectVolume;
        PictureQuality = result.PictureQuality;
        IsMouseVisiable = result.IsMouseVisiable;
        TargetFrame = result.TargetFrame;
      }
      //   Save( );
    }
    public void Save()
    {
      JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
      serializerOptions.WriteIndented = true;
      string config = JsonSerializer.Serialize(this, serializerOptions);
      File.WriteAllText(ConfigPath, config);
    }
  }
}