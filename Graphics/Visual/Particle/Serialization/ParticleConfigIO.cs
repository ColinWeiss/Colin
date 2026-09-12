using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using Colin.Core.Graphics.Visual.Particle;
using Colin.Core.Graphics.Visual.Particle.Slash;

namespace Colin.Core.Graphics.Visual.Particle.Serialization
{
  /// <summary>
  /// 粒子配置的 JSON 序列化器: 保存/读取 <see cref="ParticleEffectConfig"/>,
  /// 并提供配置树的深拷贝.
  /// <br>使用 System.Text.Json (无第三方依赖), 自带 XNA 向量类型转换器.</br>
  /// </summary>
  public static class ParticleConfigIO
  {
    /// <summary>共享序列化选项 (字段包含模式 + 向量转换器).</summary>
    public static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
      JsonSerializerOptions options = new JsonSerializerOptions
      {
        IncludeFields = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
      };
      options.Converters.Add(new Vector2Converter());
      options.Converters.Add(new Vector4Converter());
      return options;
    }

    /// <summary>保存效果配置到 JSON 文件 (UTF-8, 缩进).</summary>
    public static void Save(ParticleEffectConfig config, string path)
    {
      string json = JsonSerializer.Serialize(config, Options);
      string directory = Path.GetDirectoryName(Path.GetFullPath(path));
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        Directory.CreateDirectory(directory);
      File.WriteAllText(path, json, new System.Text.UTF8Encoding(false));
    }

    /// <summary>从 JSON 文件读取效果配置; 解析失败抛出异常.</summary>
    public static ParticleEffectConfig Load(string path)
    {
      string json = File.ReadAllText(path);
      return JsonSerializer.Deserialize<ParticleEffectConfig>(json, Options)
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

    /// <summary>
    /// 保存刀光配置到 JSON 文件 (UTF-8, 缩进).
    /// <br>嵌入纹理<b>用哪个存哪个</b>: 只写入层/刃花实际引用 ("embed:键") 的纹理,
    /// 编辑器里反复换图留下的孤儿纹理不落盘 —— 避免文件被历史导入撑爆.</br>
    /// </summary>
    public static void SaveSlash(SlashEffectConfig config, string path)
    {
      SlashEffectConfig snapshot = PruneEmbeddedTextures(config);
      string json = JsonSerializer.Serialize(snapshot, Options);
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

    /// <summary>尝试读取刀光配置 (失败返回 null 并输出日志).</summary>
    public static bool TryLoadSlash(string path, out SlashEffectConfig config)
    {
      try
      {
        config = JsonSerializer.Deserialize<SlashEffectConfig>(File.ReadAllText(path), Options)
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

    /// <summary>深拷贝任意配置对象 (经 JSON 往返, 一次分配, 编辑器复制粘贴用).</summary>
    public static T DeepCopy<T>(T source) where T : class =>
      JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source, Options), Options);

    /// <summary>Vector2 的 JSON 表示: [x, y].</summary>
    private class Vector2Converter : JsonConverter<Vector2>
    {
      public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
          float x = 0, y = 0;
          reader.Read(); x = reader.GetSingle();
          reader.Read(); y = reader.GetSingle();
          reader.Read(); // EndArray
          return new Vector2(x, y);
        }
        throw new JsonException("Vector2 需要 [x, y] 形式.");
      }

      public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
      {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
      }
    }

    /// <summary>Vector4 的 JSON 表示: [x, y, z, w].</summary>
    private class Vector4Converter : JsonConverter<Vector4>
    {
      public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
          float x = 0, y = 0, z = 0, w = 0;
          reader.Read(); x = reader.GetSingle();
          reader.Read(); y = reader.GetSingle();
          reader.Read(); z = reader.GetSingle();
          reader.Read(); w = reader.GetSingle();
          reader.Read(); // EndArray
          return new Vector4(x, y, z, w);
        }
        throw new JsonException("Vector4 需要 [x, y, z, w] 形式.");
      }

      public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
      {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteNumberValue(value.W);
        writer.WriteEndArray();
      }
    }
  }
}
