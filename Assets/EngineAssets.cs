namespace Leemo.Assets;

/// <summary>
/// 引擎侧资产根 (Assets/Engine): 编辑器特效、引擎调色数据等由 Colin.Editor 提供的资产从这里走.
/// <br>开发期向上搜索源码树里的 Colin.Editor/Assets/Engine (可版本管理, 存档直接落源码目录);
/// 未命中 (独立部署/异构布局) 回退 exe 旁 Assets/Engine (由构建拷贝).</br>
/// </summary>
public static class EngineAssets
{
  private static AssetManager _manager;

  private static string _root;

  public static string RootDir => _root ??= ResolveRoot();

  private static string ResolveRoot()
  {
    DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
    for (int depth = 0; dir is not null && depth < 8; depth++, dir = dir.Parent)
    {
      string candidate = Path.Combine(dir.FullName, "Colin.Editor", "Assets", "Engine");
      if (Directory.Exists(Path.Combine(candidate, "Effects")))
        return candidate;
    }
    return Path.Combine(AppContext.BaseDirectory, "Assets", "Engine");
  }

  public static AssetManager Manager
  {
    get
    {
      if (_manager is null)
      {
        _manager = new AssetManager(CoreInfo.Graphics.GraphicsDevice, RootDir);
        Assets.RegisterDefaults(_manager);
        _manager.RegisterLoader(new EffectSourceLoader());
      }
      return _manager;
    }
  }

  public static Effect Effect(string path) => Manager.Load<Effect>(path);
}
