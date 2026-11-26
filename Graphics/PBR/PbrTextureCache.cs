using Assimp;

namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// 模型贴图缓存: glTF 的嵌入贴图 (*0/*1 引用) 与外部相对路径贴图都在这里解码.
  /// <br>生命周期跟随模型, 材质只持有贴图引用, 由本缓存统一释放.</br>
  /// </summary>
  internal sealed class PbrTextureCache : IDisposable
  {
    private readonly GraphicsDevice _device;
    private readonly Assimp.Scene _scene;
    private readonly string _modelDirectory;
    private readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
    private readonly HashSet<string> _warned = new HashSet<string>(StringComparer.Ordinal);

    public PbrTextureCache(GraphicsDevice device, Assimp.Scene scene, string modelPath)
    {
      _device = device;
      _scene = scene;
      _modelDirectory = Path.GetDirectoryName(Path.GetFullPath(modelPath)) ?? AppContext.BaseDirectory;
    }

    public Texture2D Load(string reference)
    {
      if (string.IsNullOrEmpty(reference))
        return null;

      if (_cache.TryGetValue(reference, out Texture2D cached))
        return cached;

      Texture2D texture = null;
      try
      {
        texture = reference[0] == '*'
            ? LoadEmbedded(reference)
            : LoadExternal(reference);
      }
      catch (Exception exception)
      {
        Warn($"贴图加载失败 '{reference}': {exception.Message}");
      }

      if (texture is null)
        Warn($"贴图缺失, 将使用默认贴图: '{reference}'");
      _cache[reference] = texture;
      return texture;
    }

    private Texture2D LoadEmbedded(string reference)
    {
      if (int.TryParse(reference.AsSpan(1), out int index) is false
          || index < 0 || index >= _scene.TextureCount)
      {
        Warn($"嵌入贴图引用越界: '{reference}'");
        return null;
      }

      EmbeddedTexture embedded = _scene.Textures[index];
      if (embedded.IsCompressed)
      {
        using MemoryStream stream = new MemoryStream(embedded.CompressedData);
        Texture2D texture = Texture2D.FromStream(_device, stream);
        texture.Name = reference;
        return texture;
      }

      Texel[] texels = embedded.NonCompressedData;
      if (texels is null || texels.Length == 0 || embedded.Width <= 0 || embedded.Height <= 0)
        return null;

      Color[] pixels = new Color[texels.Length];
      for (int i = 0; i < texels.Length; i++)
        pixels[i] = new Color(texels[i].R, texels[i].G, texels[i].B, texels[i].A);

      Texture2D raw = new Texture2D(_device, embedded.Width, embedded.Height, false, SurfaceFormat.Color);
      raw.SetData(pixels);
      raw.Name = reference;
      return raw;
    }

    private Texture2D LoadExternal(string reference)
    {
      string decoded = Uri.UnescapeDataString(reference).Replace('\\', Path.DirectorySeparatorChar);
      string path = Path.IsPathRooted(decoded) ? decoded : Path.Combine(_modelDirectory, decoded);
      if (File.Exists(path) is false)
      {
        string byName = Path.Combine(_modelDirectory, Path.GetFileName(decoded));
        if (File.Exists(byName) is false)
          return null;
        path = byName;
      }

      using FileStream stream = File.OpenRead(path);
      Texture2D texture = Texture2D.FromStream(_device, stream);
      texture.Name = path;
      return texture;
    }

    private void Warn(string message)
    {
      if (_warned.Add(message))
        Console.Log(ConsoleTextType.Warning, "PBR", message);
    }

    public void Dispose()
    {
      foreach (Texture2D texture in _cache.Values)
        texture?.Dispose();
      _cache.Clear();
    }
  }
}
