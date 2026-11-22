using Microsoft.Xna.Framework.Graphics;

namespace Leemo.Assets;

/// <summary>
/// 可选契约: 加载器声明自己只受理某段路径.
/// <br>预加载扫描据此在<b>读文件之前</b>就把不归它管的路径跳过; 否则就得"读完整个文件 → 抛异常拒载 → 被上层吞掉",
/// 每抛一次异常在附加调试器时都要过一次调试器通知, 上千个文件就是秒级开销.</br>
/// <br>按需 <c>Load&lt;T&gt;</c> 不走这个过滤: 显式点名要加载时, 加载器自己的校验仍然生效.</br>
/// </summary>
public interface IPathFiltered
{
  /// <summary>该虚拟路径是否归本加载器受理 (虚拟路径: 根目录相对、正斜杠、小写).</summary>
  bool Accepts(string virtualPath);
}

/// <summary>
/// 资产加载器契约：一种加载器服务一种资产类型, 按文件扩展名分流.
/// 实现它即可为管线接入任意新资产类型(音频表、动画描述、粒子配置……).
/// </summary>
public interface IAssetLoader<T>
{
  /// <summary>本加载器受理的扩展名集合(小写、含点, 如 ".png").</summary>
  IEnumerable<string> Extensions { get; }

  /// <summary>从原始文件流构造资产.首次加载与热重载都会调用它.</summary>
  /// <param name="context">宿主环境(GraphicsDevice 等).</param>
  /// <param name="stream">可定位、可读的文件内容流(管线保证为 MemoryStream).</param>
  /// <param name="path">资产虚拟路径(根目录相对、正斜杠、小写、含扩展名).</param>
  T Load(AssetLoadContext context, Stream stream, string path);
}

/// <summary>交给加载器的宿主环境.宿主游戏可通过 Services 提供额外服务(内容管理器、日志、音频设备等).</summary>
/// <param name="PhysicalPath">本次加载文件的绝对物理路径.需要定位源文件邻近资源(如着色器 #include)的加载器使用.</param>
public readonly record struct AssetLoadContext(GraphicsDevice GraphicsDevice, IServiceProvider? Services, string PhysicalPath)
{
  public T? GetService<T>() where T : class =>
      Services?.GetService(typeof(T)) as T;
}