using FontStashSharp;

namespace Colin.Core.Assets
{
  public class FontAssets : IGameAsset
  {
    public string Name => "字体资源";

    public float Progress { get; set; }

    private static Dictionary<string, FontSystem> _fonts = new Dictionary<string, FontSystem>();
    public static Dictionary<string, FontSystem> Fonts => _fonts;

    public static FontSystem MiSansNormal => Get("MiSansNormal");
    public static FontSystem GlowSansBook => Get("GlowSansBook");
    public static FontSystem GlowSansMedium => Get("GlowSansMedium");
    public static FontSystem Unifont => Get("Unifont");

    public void LoadResource()
    {
      FontSystem _font;
      string _fileName;
      string[] _fontFileNames = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EngineInfo.Engine.Content.RootDirectory, "Fonts"), "*.*", SearchOption.AllDirectories);
      for (int count = 0; count < _fontFileNames.Length; count++)
      {
        Progress = count / _fontFileNames.Length + 1 / _fontFileNames.Length;
        _font = new FontSystem();
        _font.AddFont(File.ReadAllBytes(_fontFileNames[count]));
        _fileName = IGameAsset.ArrangementPath(_fontFileNames[count]);
        _fileName = _fileName.Replace(Path.Combine(AppDomain.CurrentDomain.BaseDirectory), "");
        Console.WriteLine(_fileName);
        _fonts.Add(_fileName, _font);
      }
    }

    /// <summary>
    /// 根据路径获取字体.
    /// </summary>
    /// <param name="path">路径.</param>
    /// <returns>字体.</returns>
    public static FontSystem Get(string path)
    {
      FontSystem _font;
      if (_fonts.TryGetValue(Path.Combine("Fonts", path), out _font))
        return _font;
      else
        return _font;
    }
  }
}