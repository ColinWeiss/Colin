using System.Text.Json;

namespace Colin.Core.Graphics.Visual.Serialization
{
  /// <summary>
  /// 刀光配置的读写入口.
  /// <br>运行时经 <see cref="SlashConfigLoader"/> (Leemo 管线) 走 <see cref="TryLoadSlash"/>;
  /// 编辑器的文件对话框直读直存走 <see cref="SaveSlash"/>, 另存为本就该绕过缓存.</br>
  /// </summary>
  public static class SlashConfigIO
  {
    /// <summary>
    /// 尝试读取刀光配置 (失败返回 null 并输出日志).
    /// </summary>
    public static bool TryLoad(string path, out SlashEffectConfig config)
    {
      try
      {
        config = JsonSerializer.Deserialize<SlashEffectConfig>(File.ReadAllText(path), VFXConfigSerialization.Options)
          ?? throw new InvalidDataException($"配置文件为空: {path}");
        return true;
      }
      catch (Exception exception)
      {
        Console.Log(ConsoleTextType.Error, "Particle", $"读取刀光配置失败 ({path}): {exception.Message}");
        config = null;
        return false;
      }
    }

    /// <summary>
    /// 保存刀光配置到 JSON 文件 (UTF-8, 缩进).
    /// </summary>
    public static void Save(SlashEffectConfig config, string path)
    {
      string json = JsonSerializer.Serialize(config, VFXConfigSerialization.Options);
      string directory = Path.GetDirectoryName(Path.GetFullPath(path));
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        Directory.CreateDirectory(directory);
      File.WriteAllText(path, json, new System.Text.UTF8Encoding(false));
    }
  }
}
