using System.Text.Json;

namespace Colin.Core.Graphics.Visual.Serialization
{
  /// <summary>
  /// 粒子配置的 JSON 读写入口 (编辑器的文件对话框直读直存, 另存为本就该绕过缓存).
  /// <br>序列化内核 (共享选项/向量转换器/深拷贝) 在 <see cref="VFXConfigSerialization"/>;
  /// 刀光的读写入口在 <see cref="SlashConfigIO"/>; 运行时加载一律走 Leemo 管线的 Loader.</br>
  /// </summary>
  public static class ParticleConfigIO
  {
    /// <summary>保存效果配置到 JSON 文件 (UTF-8, 缩进).</summary>
    public static void Save(ParticleEffectConfig config, string path)
    {
      string json = JsonSerializer.Serialize(config, VFXConfigSerialization.Options);
      string directory = Path.GetDirectoryName(Path.GetFullPath(path));
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        Directory.CreateDirectory(directory);
      File.WriteAllText(path, json, new System.Text.UTF8Encoding(false));
    }

    /// <summary>从 JSON 文件读取效果配置; 解析失败抛出异常.</summary>
    public static ParticleEffectConfig Load(string path)
    {
      string json = File.ReadAllText(path);
      return JsonSerializer.Deserialize<ParticleEffectConfig>(json, VFXConfigSerialization.Options)
        ?? throw new InvalidDataException($"配置文件为空: {path}");
    }

    /// <summary>尝试读取配置 (失败返回 null 并输出日志).</summary>
    public static bool TryLoad(string path, out ParticleEffectConfig config)
    {
      try
      {
        config = Load(path);
        return true;
      }
      catch (Exception exception)
      {
        Console.Log(ConsoleTextType.Error, "Particle", $"读取粒子配置失败 ({path}): {exception.Message}");
        config = null;
        return false;
      }
    }
  }
}
