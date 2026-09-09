using System.Text.Json;
using FontStashSharp;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Colin.Core.Assets;

/// <summary>纹理加载器: .png .jpg .jpeg .bmp .gif .tif .tga —— 直接 Texture2D.FromFile.</summary>
public sealed class TextureLoader : IAssetLoader<Texture2D>
{
  public IEnumerable<string> Extensions { get; } = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tga" };
  public Texture2D Load(AssetLoadContext ctx, Stream stream, string path)
  {
    var tex = Texture2D.FromStream(ctx.GraphicsDevice, stream);
    tex.Name = path;
    return tex;
  }
}

/// <summary>字体加载器: .ttf .otf .ttc —— FontStashSharp.FontSystem (可 AddFont 多文件做字重回退) .</summary>
public sealed class FontLoader : IAssetLoader<FontSystem>
{
  public IEnumerable<string> Extensions { get; } = new[] { ".ttf", ".otf", ".ttc" };
  public FontSystem Load(AssetLoadContext ctx, Stream stream, string path)
  {
    using var ms = new MemoryStream();
    stream.CopyTo(ms);
    var font = new FontSystem();
    font.AddFont(ms.ToArray());
    return font;
  }
}

/// <summary>音效加载器: .wav —— SoundEffect.FromStream.</summary>
public sealed class SoundEffectLoader : IAssetLoader<SoundEffect>
{
  public IEnumerable<string> Extensions { get; } = new[] { ".wav" };
  public SoundEffect Load(AssetLoadContext ctx, Stream stream, string path)
  {
    var sfx = SoundEffect.FromStream(stream);
    sfx.Name = path;
    return sfx;
  }
}

/// <summary>效果加载器: .mgfx / .cso —— 预编译 Effect 字节码 (离线用 mgfxc 编出, 运行时零编译) .</summary>
public sealed class EffectLoader : IAssetLoader<Effect>
{
  public IEnumerable<string> Extensions { get; } = new[] { ".mgfx", ".cso" };
  public Effect Load(AssetLoadContext ctx, Stream stream, string path)
  {
    using var ms = new MemoryStream();
    stream.CopyTo(ms);
    return new Effect(ctx.GraphicsDevice, ms.ToArray());
  }
}

/// <summary>数据表加载器: .json —— System.Text.Json 解析为 JsonDocument (表结构由调用方按需取用) .</summary>
public sealed class JsonDocumentLoader : IAssetLoader<JsonDocument>
{
  public IEnumerable<string> Extensions { get; } = new[] { ".json" };
  public JsonDocument Load(AssetLoadContext ctx, Stream stream, string path) =>
      JsonDocument.Parse(stream);
}

