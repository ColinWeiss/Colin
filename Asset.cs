using Colin.Core.Graphics.Shaders;
using Colin.Core.IO;
using FontStashSharp;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Colin.Core
{
  /// <summary>
  /// 抛弃晚餐 MGCB-Editor, 走向美好未来.
  /// </summary>
  public class Asset
  {
    public static string AssetsDirName = "Assets";

    private static string _dir = "";
    public static string Dir
    {
      get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _dir);
      set => _dir = value;
    }

    private static string _textureDir = "Textures";
    public static string TextureDir
    {
      get => Path.Combine(Dir, AssetsDirName, _textureDir);
      set => _textureDir = value;
    }

    private static string _fontDir = "Fonts";
    public static string FontDir
    {
      get => Path.Combine(Dir, AssetsDirName, _fontDir);
      set => _fontDir = value;
    }

    private static string _effectDir = "Effects";
    public static string EffectDir
    {
      get => Path.Combine(Dir, AssetsDirName, _effectDir);
      set => _effectDir = value;
    }

    private static string _shaderDir = "Shaders";
    public static string ShaderDir
    {
      get => Path.Combine(Dir, AssetsDirName, _shaderDir);
      set => _shaderDir = value;
    }

    private static string _soundEffectDir = "Sounds";
    public static string SoundEffectDir
    {
      get => Path.Combine(Dir, AssetsDirName, _soundEffectDir);
      set => _soundEffectDir = value;
    }

    private static string _tableDir = "Tables";
    public static string TableDir
    {
      get => Path.Combine(Dir, AssetsDirName, _tableDir);
      set => _tableDir = value;
    }

    private static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
    private static Dictionary<string, FontSystem> _fonts = new Dictionary<string, FontSystem>();
    private static Dictionary<string, Effect> _effects = new Dictionary<string, Effect>();
    private static Dictionary<string, ComputeShader> _computeShaders = new Dictionary<string, ComputeShader>();
    private static Dictionary<string, SoundEffect> _soundEffects = new Dictionary<string, SoundEffect>();
    private static Dictionary<string, Song> _songs = new Dictionary<string, Song>();
    private static Dictionary<string, CsvFile> _tables = new Dictionary<string, CsvFile>();

    public static void LoadAssets()
    {
      LoadTextures();
      LoadFonts();
      LoadEffects();
      LoadComputeShaders();
      LoadSoundEffects();
      LoadCsvFiles();
    }
    public static void LoadTextures()
    {
      if (Directory.Exists(TextureDir) is false)
      {
        Console.WriteLine("Error", "未检查到纹理文件夹.");
        return;
      }
      else
        Console.WriteLine("正在加载纹理资源.");

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
        Console.WriteLine("Error", "未检查到字体文件夹.");
        return;
      }
      else
        Console.WriteLine("正在加载字体资源.");

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
        Console.WriteLine("Error", "未检查到着色器文件夹.");
        return;
      }
      else
        Console.WriteLine("正在加载着色器资源.");

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
          _effects.Add(fileName, rShader);
        }
      }
    }
    public static void LoadComputeShaders()
    {
      if (Directory.Exists(ShaderDir) is false)
      {
        Console.WriteLine("Error", "未检查到计算着色器文件夹.");
        return;
      }
      else
        Console.WriteLine("正在加载计算着色器资源.");

      if (CoreInfo.Debug)
        CompileShaders();
      ComputeShader effect;
      string fileName;
      string[] fileFullName = Directory.GetFiles(ShaderDir, "*.hlsl*", SearchOption.AllDirectories);

      for (int count = 0; count < fileFullName.Length; count++)
      {
        fileName = Path.ChangeExtension(fileFullName[count], ComputeShader.FileExtension);
        if (File.Exists(fileName) is false)
          CompileShaders();
        effect = new ComputeShader(CoreInfo.Graphics.GraphicsDevice, File.ReadAllBytes(fileName));
        fileName = OrganizePath(fileName);
        _computeShaders.Add(fileName, effect);
      }
    }
    private static void CompileShaders()
    {
      if (Directory.Exists(EffectDir) is false)
      {
        Console.WriteLine("Error", "未检查到着色器文件夹.");
        return;
      }
      string[] fileFullName = Directory.GetFiles(ShaderDir, "*.hlsl*", SearchOption.AllDirectories);
      string fileName;
      for (int count = 0; count < fileFullName.Length; count++)
      {
        fileName = fileFullName[count];
        ShaderCompiler.Compile(fileName, ShaderCompiler.CompileProfile.Compute);
      }
    }
    public static void LoadSoundEffects()
    {
      if (Directory.Exists(SoundEffectDir) is false)
      {
        Console.WriteLine("Error", "未检查到音效文件夹.");
        return;
      }
      else
        Console.WriteLine("正在加载音效资源.");
      string[] fileFullName = Directory.GetFiles(SoundEffectDir, "*.wav*", SearchOption.AllDirectories);
      string fileName;
      SoundEffect target;
      for (int count = 0; count < fileFullName.Length; count++)
      {
        fileName = fileFullName[count];
        target = SoundEffect.FromFile(fileName);
        fileName = OrganizePath(fileName);
        target.Name = fileName;
        _soundEffects.Add(fileName, target);
      }
    }
    public static void LoadCsvFiles()
    {
      if (Directory.Exists(TableDir) is false)
      {
        Console.WriteLine("Error", "未检查到配表文件夹.");
        return;
      }
      else
        Console.WriteLine("正在加载配表资源.");
      string[] fileFullName = Directory.GetFiles(TableDir, "*.csv*", SearchOption.AllDirectories);
      string fileName;
      CsvFile target;
      for (int count = 0; count < fileFullName.Length; count++)
      {
        fileName = fileFullName[count];
        target = new CsvFile(fileName);
        fileName = OrganizePath(fileName);
        _tables.Add(fileName, target);
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

    public static Effect GetEffect(string path)
    {
      path = path.Replace("/", Path.DirectorySeparatorChar.ToString());
      if (_effects.TryGetValue(path, out Effect result))
        return result;
      else
        return null;
    }

    public static ComputeShader GetComputeShader(string path)
    {
      path = path.Replace("/", Path.DirectorySeparatorChar.ToString());
      ComputeShader shader;
      if (_computeShaders.TryGetValue(path, out shader))
        return shader;
      else
        return null;
    }

    public static FontSystem GetFont(string path)
    {
      path = path.Replace("/", Path.DirectorySeparatorChar.ToString());
      if (_fonts.TryGetValue(path, out FontSystem result))
        return result;
      else
        return null;
    }

    public static SoundEffect GetSoundEffect(string path)
    {
      path = path.Replace("/", Path.DirectorySeparatorChar.ToString());
      if (_soundEffects.TryGetValue(path, out SoundEffect result))
        return result;
      else
        return null;
    }

    public static CsvFile GetCsv(string path)
    {
      path = path.Replace("/", Path.DirectorySeparatorChar.ToString());
      if (_tables.TryGetValue(path, out CsvFile result))
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
      sb.Replace(string.Concat(ShaderDir, Path.DirectorySeparatorChar), "");
      sb.Replace(string.Concat(SoundEffectDir, Path.DirectorySeparatorChar), "");
      sb.Replace(string.Concat(TableDir, Path.DirectorySeparatorChar), "");
      sb.Replace(Path.GetExtension(sb.ToString()), "");
      return sb.ToString();
    }
  }
}