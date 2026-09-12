using Microsoft.Xna.Framework.Graphics;

namespace Leemo.Assets;

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