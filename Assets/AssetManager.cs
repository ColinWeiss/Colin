using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Leemo.Assets;

/// <summary>
/// 统一的运行时资产管道(MonoGame / WindowsDX).
/// 特性：
///   - 直接加载原始资源文件, 完全绕开 MGCB / Content Pipeline 编译产物；
///   - 统一的 Load / LoadAsync / TryGet / Unload API, 按 (资产类型, 虚拟路径) 缓存；
///   - 通过 IAssetLoader&lt;T&gt; 按扩展名注册加载器, 新资产类型零侵入接入；
///   - 内置 FileSystemWatcher 热重载：文件一改即排队, 主线程 PumpReloads() 时重建并换入缓存, 
///     GPU 资源始终在主线程重建, 重载失败保留旧资产并回调.
/// 线程约定：Load 的文件 IO 可在任意线程, 但构造 GPU 资源的 Load/Async 收尾与
/// PumpReloads 必须在主线程调用(MonoGame 图形设备惯例).
/// </summary>
public sealed class AssetManager : IDisposable
{
  private readonly record struct CacheKey(Type AssetType, string Path);

  // 非泛型内衬：注册表内部统一调度
  private interface ILoaderBox
  {
    Type AssetType { get; }
    IEnumerable<string> Extensions { get; }
    object LoadUntyped(AssetLoadContext context, Stream stream, string path);
  }

  private sealed class LoaderBox<T> : ILoaderBox
  {
    private readonly IAssetLoader<T> _inner;
    public LoaderBox(IAssetLoader<T> inner) => _inner = inner;
    public Type AssetType => typeof(T);
    public IEnumerable<string> Extensions => _inner.Extensions;
    public object LoadUntyped(AssetLoadContext ctx, Stream stream, string path) => _inner.Load(ctx, stream, path)!;
  }

  private readonly GraphicsDevice _graphicsDevice;
  private readonly IServiceProvider? _services;
  // 缓存会被"后台预加载线程"与"主线程按需加载"并发读写, 必须线程安全.
  private readonly ConcurrentDictionary<CacheKey, object> _cache = new();
  // 加载器注册表: 仅在主线程初始化阶段写入, 之后只读.
  private readonly Dictionary<(Type, string), ILoaderBox> _loaders = new();

  private FileSystemWatcher? _watcher;
  private readonly ConcurrentDictionary<string, byte> _pendingReloads = new();

  public string RootDir { get; }

  /// <summary>热重载是否处于监听状态.</summary>
  public bool HotReloadEnabled { get; private set; }

  /// <summary>(虚拟路径, 资产类型, 新资产实例).在主线程 PumpReloads 内触发.</summary>
  public event Action<string, Type, object>? AssetReloaded;

  /// <summary>(虚拟路径, 异常).热重载失败时触发, 缓存保留旧资产.</summary>
  public event Action<string, Exception>? ReloadFailed;

  public AssetManager(GraphicsDevice graphicsDevice, string? rootDir = null,
                      IServiceProvider? services = null, bool hotReload = false)
  {
    _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
    _services = services;
    RootDir = Path.GetFullPath(rootDir ?? Path.Combine(AppContext.BaseDirectory, "Assets"));
    if (!Directory.Exists(RootDir))
      Directory.CreateDirectory(RootDir);
    if (hotReload)
      EnableHotReload();
  }

  /// <summary>按本次加载的文件构造宿主上下文(加载器可经 PhysicalPath 定位源文件邻近资源).</summary>
  private AssetLoadContext ContextFor(string physical) =>
      new(_graphicsDevice, _services, physical);

  // ---------- 加载器注册 ----------

  /// <summary>为资产类型 T 注册加载器(按扩展名).同扩展名后注册者覆盖先注册者.</summary>
  public void RegisterLoader<T>(IAssetLoader<T> loader)
  {
    ArgumentNullException.ThrowIfNull(loader);
    var box = new LoaderBox<T>(loader);
    foreach (var raw in loader.Extensions)
    {
      var ext = NormalizeExtension(raw);
      _loaders[(typeof(T), ext)] = box;
    }
  }

  /// <summary>已注册的 (资产类型, 扩展名) 清单, 诊断用.</summary>
  public IEnumerable<(Type AssetType, string Extension)> RegisteredLoaders =>
      _loaders.Keys.Select(k => (k.Item1, k.Item2));

  // ---------- 统一取用 API ----------

  /// <summary>同步加载.已缓存则直取, 否则经注册加载器从原始文件构造并缓存.</summary>
  public T Load<T>(string path)
  {
    var key = NormalizePath(path);
    var cacheKey = new CacheKey(typeof(T), key);
    if (_cache.TryGetValue(cacheKey, out var cached))
      return (T)cached;

    var physical = PhysicalPath(key);
    if (!File.Exists(physical))
      throw new FileNotFoundException($"Leemo.Assets: 未找到资产文件 '{physical}'", physical);

    var loader = FindLoader<T>(key);
    var asset = loader.LoadUntyped(ContextFor(physical), ReadBuffered(physical), key);
    _cache[cacheKey] = asset;
    return (T)asset;
  }

  /// <summary>
  /// 异步加载：文件读取在后台线程, 资产构造(含 GPU 上传)回到调用方线程完成——
  /// 符合 MonoGame 图形设备的线程约定.缓存命中时等价于同步直取.
  /// </summary>
  public async Task<T> LoadAsync<T>(string path, CancellationToken cancellationToken = default)
  {
    var key = NormalizePath(path);
    var cacheKey = new CacheKey(typeof(T), key);
    if (_cache.TryGetValue(cacheKey, out var cached))
      return (T)cached;

    var physical = PhysicalPath(key);
    if (!File.Exists(physical))
      throw new FileNotFoundException($"Leemo.Assets: 未找到资产文件 '{physical}'", physical);

    var loader = FindLoader<T>(key);
    var bytes = await Task.Run(() => File.ReadAllBytes(physical), cancellationToken).ConfigureAwait(false);
    using var stream = new MemoryStream(bytes);
    var asset = loader.LoadUntyped(ContextFor(physical), stream, key);
    _cache[cacheKey] = asset;
    return (T)asset;
  }

  /// <summary>仅查缓存, 不触发加载.</summary>
  public bool TryGet<T>(string path, out T? asset)
  {
    if (_cache.TryGetValue(new CacheKey(typeof(T), NormalizePath(path)), out var hit))
    {
      asset = (T)hit;
      return true;
    }
    asset = default;
    return false;
  }

  /// <summary>缓存中是否存在该资产.</summary>
  public bool IsLoaded<T>(string path) =>
      _cache.ContainsKey(new CacheKey(typeof(T), NormalizePath(path)));

  /// <summary>卸载单个资产；实现 IDisposable 的资产会被释放(请在主线程调用).</summary>
  public bool Unload<T>(string path, bool dispose = true)
  {
    if (_cache.Remove(new CacheKey(typeof(T), NormalizePath(path)), out var asset))
    {
      if (dispose) (asset as IDisposable)?.Dispose();
      return true;
    }
    return false;
  }

  /// <summary>清空缓存并(可选)释放全部资产；监听器保持原状(请在主线程调用).</summary>
  public void UnloadAll(bool dispose = true)
  {
    if (dispose)
      foreach (var asset in _cache.Values)
        (asset as IDisposable)?.Dispose();
    _cache.Clear();
  }

  // ---------- 热重载 ----------

  /// <summary>开启文件监视.变更事件进入排队, 等待主线程 PumpReloads() 落地.</summary>
  public void EnableHotReload()
  {
    if (_watcher is not null) return;
    _watcher = new FileSystemWatcher(RootDir, "*.*")
    {
      IncludeSubdirectories = true,
      NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
    };
    _watcher.Changed += QueueReload;
    _watcher.Created += QueueReload;
    _watcher.Renamed += QueueReload;
    _watcher.EnableRaisingEvents = true;
    HotReloadEnabled = true;
  }

  public void DisableHotReload()
  {
    _watcher?.Dispose();
    _watcher = null;
    HotReloadEnabled = false;
  }

  private void QueueReload(object? sender, FileSystemEventArgs e)
  {
    try { NotifyChange(e.FullPath); }
    catch { /* 变更瞬间文件可能不可见, 忽略等下次 */ }
  }

  /// <summary>
  /// 手动把一个文件标记为待重载(相对 RootDir 或绝对路径均可).
  /// 文件系统不支持监视时的备用通道——网络共享盘/容器挂载卷上
  /// FileSystemWatcher 不产生事件, 宿主可在自己的变更检测里调它.
  /// </summary>
  public void NotifyChange(string path)
  {
    var full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(RootDir, path));
    if (!full.StartsWith(RootDir, StringComparison.OrdinalIgnoreCase)) return;
    _pendingReloads.TryAdd(NormalizePath(Path.GetRelativePath(RootDir, full)), 0);
  }

  /// <summary>
  /// 在游戏 Update 中调用(主线程)：把排队的文件变更重建为新资产、换入缓存并触发
  /// AssetReloaded；失败的保留旧资产并触发 ReloadFailed.返回本次重载数量.
  /// </summary>
  public int PumpReloads()
  {
    if (_pendingReloads.IsEmpty) return 0;
    var reloaded = 0;
    foreach (var key in _pendingReloads.Keys)
    {
      _pendingReloads.TryRemove(key, out _);
      var physical = PhysicalPath(key);
      if (!File.Exists(physical))
        continue;

      var ext = ExtensionOf(key);
      foreach (var cacheKey in _cache.Keys.Where(k => k.Path == key).ToArray())
      {
        if (!_loaders.TryGetValue((cacheKey.AssetType, ext), out var loader))
          continue;
        try
        {
          using var stream = ReadBuffered(physical);
          var asset = loader.LoadUntyped(ContextFor(physical), stream, key);
          _cache[cacheKey] = asset;
          AssetReloaded?.Invoke(key, cacheKey.AssetType, asset);
          Console.WriteLine(string.Concat("检查到文件变更", key));
          reloaded++;
        }
        catch (Exception ex)
        {
          ReloadFailed?.Invoke(key, ex);
        }
      }
    }
    return reloaded;
  }

  /// <summary>
  /// 预加载：扫描目录(默认整个根目录), 把所有命中已注册加载器的文件按其资产类型
  /// 全部载入缓存(已缓存的跳过).单文件失败不中断整体.返回本次加载数量.
  /// </summary>
  public int LoadDirectory(string? subDir = null, SearchOption option = SearchOption.AllDirectories)
  {
    var dir = subDir is null ? RootDir : Path.Combine(RootDir, subDir);
    if (!Directory.Exists(dir)) return 0;
    var count = 0;
    foreach (var file in Directory.EnumerateFiles(dir, "*.*", option))
    {
      string virt;
      try { virt = NormalizePath(Path.GetRelativePath(RootDir, Path.GetFullPath(file))); }
      catch { continue; }
      var ext = ExtensionOf(virt);
      foreach (var kv in _loaders)
      {
        if (kv.Key.Item2 != ext) continue;
        var cacheKey = new CacheKey(kv.Key.Item1, virt);
        if (_cache.ContainsKey(cacheKey)) continue;
        try
        {
          using var stream = ReadBuffered(file);
          _cache[cacheKey] = kv.Value.LoadUntyped(ContextFor(file), stream, virt);
          count++;
        }
        catch { /* 单个失败不拖垮预加载 */ }
      }
    }
    return count;
  }
  // ---------- 内部工具 ----------

  private MemoryStream ReadBuffered(string physical)
  {
    using var fs = File.OpenRead(physical);
    var ms = new MemoryStream();
    fs.CopyTo(ms);
    ms.Position = 0;
    return ms;
  }

  private ILoaderBox FindLoader<T>(string key)
  {
    var ext = ExtensionOf(key);
    if (_loaders.TryGetValue((typeof(T), ext), out var loader))
      return loader;
    var known = string.Join(", ", _loaders.Keys
        .Where(k => k.Item1 == typeof(T))
        .Select(k => k.Item2));
    throw new NotSupportedException(
        $"Leemo.Assets: 没有能为 {typeof(T).Name} 处理扩展名 '{ext}' 的加载器(路径 '{key}')." +
        (known.Length > 0 ? $"该类型已注册的扩展名：{known}." : "该类型尚未注册任何加载器."));
  }

  private string PhysicalPath(string virtualPath) =>
      Path.Combine(RootDir, virtualPath.Replace('/', Path.DirectorySeparatorChar));

  internal static string NormalizePath(string path) =>
      path.Replace('\\', '/').TrimStart('/').ToLowerInvariant();

  internal static string ExtensionOf(string virtualPath)
  {
    var dot = virtualPath.LastIndexOf('.');
    return dot < 0 ? "" : virtualPath[dot..];
  }

  internal static string NormalizeExtension(string ext) =>
      ext.StartsWith('.') ? ext.ToLowerInvariant() : "." + ext.ToLowerInvariant();

  public void Dispose()
  {
    DisableHotReload();
    UnloadAll();
  }
}


