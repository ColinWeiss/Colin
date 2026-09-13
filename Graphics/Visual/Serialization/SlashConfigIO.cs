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
    /// <br>嵌入纹理<b>用哪个存哪个</b>是保存侧的职责, 读取端原样反序列化.</br>
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
    /// <br>嵌入纹理<b>用哪个存哪个</b>: 只写入层/刃花实际引用 ("embed:键") 的纹理,
    /// 编辑器里反复换图留下的孤儿纹理不落盘 —— 避免文件被历史导入撑爆.</br>
    /// </summary>
    public static void Save(SlashEffectConfig config, string path)
    {
      SlashEffectConfig snapshot = PruneEmbeddedTextures(config);
      string json = JsonSerializer.Serialize(snapshot, VFXConfigSerialization.Options);
      string directory = Path.GetDirectoryName(Path.GetFullPath(path));
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        Directory.CreateDirectory(directory);
      File.WriteAllText(path, json, new System.Text.UTF8Encoding(false));
    }

    /// <summary>
    /// 裁剪嵌入纹理表: 收集层与刃花实际引用的 "embed:键", 只保留命中的条目.
    /// <br>无孤儿时原样返回 (不克隆不拷贝); 有孤儿时在深拷贝上裁剪, 不改动调用方的原件.</br>
    /// </summary>
    private static SlashEffectConfig PruneEmbeddedTextures(SlashEffectConfig config)
    {
      if (config.EmbeddedTextures is null || config.EmbeddedTextures.Count == 0)
        return config;

      HashSet<string> used = new HashSet<string>(System.StringComparer.Ordinal);
      if (config.Arc?.Layers is not null)
        foreach (SlashTextureLayer layer in config.Arc.Layers)
          if (layer?.Texture is not null && layer.Texture.StartsWith("embed:", System.StringComparison.Ordinal))
            used.Add(layer.Texture.Substring(6));
      if (config.Sparks?.Texture is not null && config.Sparks.Texture.StartsWith("embed:", System.StringComparison.Ordinal))
        used.Add(config.Sparks.Texture.Substring(6));

      if (used.Count == 0)
      {
        // —— 一张都不引用: 不写任何嵌入纹理. ——
        SlashEffectConfig empty = config.Clone();
        empty.EmbeddedTextures = new Dictionary<string, string>();
        return empty;
      }

      List<string> orphans = config.EmbeddedTextures.Keys.Where(key => !used.Contains(key)).ToList();
      if (orphans.Count == 0)
        return config;

      SlashEffectConfig pruned = config.Clone();
      foreach (string orphan in orphans)
        pruned.EmbeddedTextures.Remove(orphan);
      return pruned;
    }
  }
}
