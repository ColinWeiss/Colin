using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Colin.Core.Graphics.Visual.Serialization
{
  /// <summary>
  /// 粒子/刀光配置的 JSON 序列化内核: 共享选项 + XNA 向量类型转换器 + 通用深拷贝.
  /// <br>读写入口按资产分家: 粒子在 <see cref="ParticleConfigIO"/>, 刀光在 <see cref="SlashConfigIO"/>,
  /// 这里只放两边共用的公共件, 不做任何文件 IO.</br>
  /// </summary>
  public static class VFXConfigSerialization
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
          reader.Read(); // EndArray, 不消费它反序列化器会报 read too much or not enough
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
