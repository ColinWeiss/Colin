using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using Particle.Core;
using Particle.Slash;

namespace Particle.Serialization
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
        Console.WriteLine("Error", $"读取粒子配置失败 ({path}): {exception.Message}");
        config = null;
        return false;
      }
    }

    /// <summary>保存刀光配置到 JSON 文件 (UTF-8, 缩进).</summary>
    public static void SaveSlash(SlashEffectConfig config, string path)
    {
      string json = JsonSerializer.Serialize(config, Options);
      string directory = Path.GetDirectoryName(Path.GetFullPath(path));
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        Directory.CreateDirectory(directory);
      File.WriteAllText(path, json, new System.Text.UTF8Encoding(false));
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
        Console.WriteLine("Error", $"读取刀光配置失败 ({path}): {exception.Message}");
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
