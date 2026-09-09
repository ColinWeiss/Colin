using FontStashSharp;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Leemo.Assets;

/// <summary>
/// 静态门面：单例式的熟悉手感（从 Colin.Core.Asset 迁移几乎零成本），底层仍是一台 AssetManager。
/// Game 启动时 Assets.Init(manager) 一次即可全局使用。
/// </summary>
public static class Assets
{
    private static AssetManager? _manager;
    public static AssetManager Manager =>
        _manager ?? throw new InvalidOperationException("Leemo.Assets: 请先在 Game 初始化时调用 Assets.Init(...)。");

    /// <summary>绑定全局实例并注册全套内置加载器（纹理/字体/音效/效果/Json）。</summary>
    public static AssetManager Init(GraphicsDevice graphicsDevice, string? rootDir = null,
                                    IServiceProvider? services = null, bool hotReload = false)
    {
        _manager?.Dispose();
        var manager = new AssetManager(graphicsDevice, rootDir, services, hotReload);
        RegisterDefaults(manager);
        return _manager = manager;
    }

    /// <summary>把内置加载器注册到任意 AssetManager（想自管实例时用这个）。</summary>
    public static void RegisterDefaults(AssetManager manager)
    {
        manager.RegisterLoader(new TextureLoader());
        manager.RegisterLoader(new FontLoader());
        manager.RegisterLoader(new SoundEffectLoader());
        manager.RegisterLoader(new EffectLoader());
        manager.RegisterLoader(new JsonDocumentLoader());
    }

    public static void Shutdown()
    {
        _manager?.Dispose();
        _manager = null;
    }

    // ---- 迁移友好：对应 Colin.Core.Asset 的 Get* 手感 ----
    public static Texture2D Texture(string path) => Manager.Load<Texture2D>(path);
    public static FontSystem Font(string path) => Manager.Load<FontSystem>(path);
    public static SoundEffect Sound(string path) => Manager.Load<SoundEffect>(path);
    public static Effect Effect(string path) => Manager.Load<Effect>(path);
}

