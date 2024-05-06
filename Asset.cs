using Colin.Core.Graphics.Shaders;
using FontStashSharp;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Diagnostics;

namespace Colin.Core
{
  /// <summary>
  /// 抛弃晚餐 MGCB-Editor, 走向美好未来.
  /// </summary>
  public class Asset
  {
    private static string _dir = "";
    public static string Dir
    {
      get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _dir);
      set => _dir = value;
    }

    private static string _textureDir = "Textures";
    public static string TextureDir
    {
      get => Path.Combine(Dir, "Sources", _textureDir);
      set => _textureDir = value;
    }

    private static string _fontDir = "Fonts";
    public static string FontDir
    {
      get => Path.Combine(Dir, "Sources", _fontDir);
      set => _fontDir = value;
    }

    private static string _effectDir = "Effects";
    public static string EffectDir
    {
      get => Path.Combine(Dir, "Sources", _effectDir);
      set => _fontDir = value;
    }

    public static bool UseMgcbEditor = false;

    private static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
    private static Dictionary<string, FontSystem> _fonts = new Dictionary<string, FontSystem>();
    private static Dictionary<string, Effect> _effects = new Dictionary<string, Effect>();
    private static Dictionary<string, ComputeShader> _computeShaders = new Dictionary<string, ComputeShader>();
    private static Dictionary<string, SoundEffect> _soundEffects = new Dictionary<string, SoundEffect>();
    private static Dictionary<string, Song> _songs = new Dictionary<string, Song>();

    public static void LoadAssets()
    {
      LoadTextures();
      LoadFonts();
      LoadEffects();
    }
    public static void LoadTextures()
    {
      if (Directory.Exists(TextureDir) is false)
      {
        Console.WriteLine(ConsoleTextType.Error, "未检查到纹理文件夹.");
        return;
      }
      string[] fileFullName = Directory.GetFiles(TextureDir, "*.png*", SearchOption.AllDirectories);
      string fileName;
      Texture2D target;
      for (int count = 0; count < fileFullName.Length; count++)
      {
        fileName = fileFullName[count];
        target = Texture2D.FromFile(CoreInfo.Graphics.GraphicsDevice, fileName);
        fileName = OrganizePath(fileName);
        target.Name = fileName;
        _textures.Add(fileName, target);
      }
    }
    public static void LoadFonts()
    {
      if (Directory.Exists(FontDir) is false)
      {
        Console.WriteLine(ConsoleTextType.Error, "未检查到字体文件夹.");
        return;
      }
      string[] fileFullName = Directory.GetFiles(FontDir, "*.ttf*", SearchOption.AllDirectories);
      string fileName;
      FontSystem target;
      for (int count = 0; count < fileFullName.Length; count++)
      {
        fileName = fileFullName[count];
        target = new FontSystem();
        target.AddFont(File.ReadAllBytes(fileName));
        fileName = OrganizePath(fileName);
        _fonts.Add(fileName, target);
      }
    }
    public static void LoadEffects()
    {
      if (Directory.Exists(EffectDir) is false)
      {
        Console.WriteLine(ConsoleTextType.Error, "未检查到着色器文件夹.");
        return;
      }
      string[] fileFullName = Directory.GetFiles(EffectDir, "*.fx*", SearchOption.AllDirectories);
      string fileName;
      string sourceFile;
      string targetFile;
      List<string> cmdLine = new List<string>();
      for (int count = 0; count < fileFullName.Length; count++)
      {
        fileName = fileFullName[count];
        sourceFile = Path.Combine(EffectDir, fileName);
        targetFile = Path.ChangeExtension(Path.Combine(EffectDir, fileName), ".rShader");
        if (Path.GetExtension(sourceFile) is not ".fx" || File.Exists(targetFile))
          continue;
        cmdLine.Add($"mgfxc {sourceFile} {targetFile} /Profile:DirectX_11");
      }
      Console.Execute(cmdLine);

      Effect rShader;
      fileFullName = Directory.GetFiles(EffectDir, "*.rShader", SearchOption.AllDirectories);
      for (int count = 0; count < fileFullName.Length; count++)
      {
        fileName = fileFullName[count];
        targetFile = Path.Combine(EffectDir, fileName);
        if (Path.GetExtension(targetFile) is ".rShader")
        {
          rShader = new Effect(CoreInfo.Graphics.GraphicsDevice, File.ReadAllBytes(targetFile));
          fileName = OrganizePath(fileName);
          Console.WriteLine(fileName);
          _effects.Add(fileName, rShader);
        }
      }
    }

    public static Texture2D GetTexture(string path)
    {
      path = path.Replace("/", Path.DirectorySeparatorChar.ToString());
      if (_textures.TryGetValue(path, out Texture2D result))
        return result;
      else
        return null;
    }
    public static Texture2D GetTexture(params string[] path)
    {
      return GetTexture(Path.Combine(path));
    }

    public static FontSystem GetFont(string path)
    {
      path = path.Replace("/", Path.DirectorySeparatorChar.ToString());
      if (_fonts.TryGetValue(path, out FontSystem result))
        return result;
      else
        return null;
    }

    private static string OrganizePath(string path)
    {
      StringBuilder sb = new StringBuilder(path);
      sb.Replace(string.Concat(TextureDir, Path.DirectorySeparatorChar), "");
      sb.Replace(string.Concat(FontDir, Path.DirectorySeparatorChar), "");
      sb.Replace(string.Concat(EffectDir, Path.DirectorySeparatorChar), "");
      sb.Replace(Path.GetExtension(sb.ToString()), "");
      return sb.ToString();
    }
  }
}