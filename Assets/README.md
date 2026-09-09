# Leemo.Assets

MonoGame 的**运行时资产管线**独立类库：直接加载原始资源文件，完全绕开 MGCB / Content Pipeline 编译产物。自带统一 API、按扩展名的可插拔加载器、以及主线程安全的热重载。

从 Colin.Core.Asset 解耦重构而来——原有静态门面手感保留，硬依赖（Colin 的 Console / CoreInfo / ShaderCompiler）全部移除。

- 目标框架：`net8.0-windows`
- 依赖：`MonoGame.Framework.WindowsDX 3.8.5.1`、`FontStashSharp.MonoGame 1.3.9`
- 已通过冒烟测试：离屏 GraphicsDevice 下纹理/字体/音效真实加载、预加载、热重换入、异常路径

## 集成到你的游戏

1. 把 `Leemo.Assets/` 文件夹拷进仓库（或引用编译出的 dll）：

   ```xml
   <ItemGroup>
     <ProjectReference Include="..\Leemo.Assets\Leemo.Assets.csproj" />
   </ItemGroup>
   ```

2. Game 初始化时绑定一次：

   ```csharp
   using Leemo.Assets;

   protected override void Initialize()
   {
       // rootDir 缺省为 <exe目录>/Assets；hotReload 打开即启用文件监视
       Assets.Init(GraphicsDevice, hotReload: true);
   }

   protected override void Update(GameTime gameTime)
   {
       Assets.Manager.PumpReloads(); // 热重载落地必须在主线程
       base.Update(gameTime);
   }
   ```

3. 用起来（路径相对 rootDir，`/` `\` 均可，大小写不敏感）：

   ```csharp
   var tex   = Assets.Texture("Textures/player.png");   // Texture2D
   var font  = Assets.Font("Fonts/GlowSansMedium.ttf"); // FontStashSharp.FontSystem
   var sfx   = Assets.Sound("Sounds/hit.wav");          // SoundEffect
   var fx    = Assets.Effect("Effects/Particle.mgfx");  // Effect（预编译字节码）
   var table = Assets.Manager.Load<JsonDocument>("Tables/items.json");

   // 预加载：开局把整个目录灌进内存（缓存过的直取）
   Assets.Manager.LoadDirectory();
   ```

   资产全局缓存：同一 `(类型, 路径)` 只加载一次；`TryGet` / `IsLoaded` / `Unload` / `UnloadAll` 管缓存。

## 与 Colin.Core.Asset 的对照

| 原来 | 现在 |
|---|---|
| `Asset.LoadAssets()` 启动全量加载 | `Assets.Manager.LoadDirectory()`（或按需 `Load`） |
| `Asset.GetTexture("Pixel")` | `Assets.Texture("Pixel.png")`（或保留无扩展名习惯：自己包一层） |
| `Asset.GetFont / GetSoundEffect / GetEffect` | `Assets.Font / Assets.Sound / Assets.Effect` |
| `Asset.TextureDir` 等六个目录常量 | 单一根目录 `RootDir`，子目录随意组织 |
| 隐式全目录扫描 | 显式扫描 + 按需加载并存 |
| `Console.WriteLine` 打日志 | 无依赖；失败抛带路径的异常，可选事件回调 |

路径键不再依赖「把各目录前缀 Replace 掉」的 `OrganizePath` 小技巧——直接以根目录相对路径为键，取用与目录组织解耦。

## 添加新资产类型

实现 `IAssetLoader<T>`，注册，即刻生效：

```csharp
public sealed class CsvLoader : IAssetLoader<string[][]>
{
    public IEnumerable<string> Extensions { get; } = new[] { ".csv" };
    public string[][] Load(AssetLoadContext ctx, Stream stream, string path)
    {
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries)
                   .Select(l => l.Split(',')).ToArray();
    }
}

Assets.Manager.RegisterLoader(new CsvLoader());
var grid = Assets.Manager.Load<string[][]>("Tables/drops.csv");
```

内置加载器：`TextureLoader`（.png .jpg .jpeg .bmp .gif .tif .tga）、`FontLoader`（.ttf .otf .ttc）、`SoundEffectLoader`（.wav）、`EffectLoader`（.mgfx .cso，用 `mgfxc` 离线编译）、`JsonDocumentLoader`（.json）。

Colin.Core 额外注册两个项目专属加载器（源码在 `Loaders/`，均进程内编译、无外部工具依赖）：

- `EffectSourceLoader`（.fx）——经 MonoGame 内容管线的 `EffectProcessor` 在当前进程内把 HLSL 源码编译为 MGFX 字节码（复用 `ParticleEffectCompiler.CompileFromFile`），`#include` 相对源文件目录解析；
- `ComputeShaderLoader`（.hlsl .cso）——.hlsl 经 SharpDX.D3DCompiler 进程内编译为 cs_5_0，.cso 直读预编译字节码。

`AssetLoadContext` 携带 `PhysicalPath`（本次加载文件的绝对路径），供需要定位源文件邻近资源（着色器 #include 等）的加载器使用。

## 热重载

- `EnableHotReload()` 开 FileSystemWatcher；改动进队列，主线程 `PumpReloads()` 时重建并换入缓存，`AssetReloaded` 事件带出新实例。GPU 资源始终在主线程重建，符合 MonoGame 线程约定。
- 重载失败不炸游戏：保留旧资产，`ReloadFailed` 事件报异常。
- 网络共享盘 / 容器挂载卷上 watcher 不发事件——提供 `NotifyChange(path)` 手动通道，宿主自己做变更检测后调它即可（本库在 VM 共享文件夹上开发，此通道已实测）。

## 设计要点

- **缓存键 = (资产类型, 规范化虚拟路径)**：同路径不同类型互不干扰；扩展名决定加载器。
- **`LoadAsync<T>`**：文件 IO 在后台线程，字节流回主线程构造（GPU 上传不跨线程）。
- **无静态可变状态泄漏**：静态门面 `Assets` 只是对一台 `AssetManager` 的转发；想多根目录/隔离缓存，自己 new `AssetManager`，`Assets.RegisterDefaults(mgr)` 照样给全套内置加载器。
- 释放：实现 `IDisposable` 的资产在 `Unload` / `UnloadAll` / `Dispose` 时一并释放（主线程调用）。
