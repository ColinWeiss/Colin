using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Leemo.Assets;

/// <summary>
/// 特效源码加载器：.fx —— 经 MonoGame 内容管线的 <c>EffectProcessor</c> 在当前进程内
/// 编译为 MGFX 字节码后构造 <see cref="Effect"/>(原 Asset.LoadEffects 的"启动时编译 .fx"
/// 工作流).编译核心随 MonoGame.Framework.Content.Pipeline.dll 分发, 玩家机器上
/// 不需要安装 mgfxc 等任何外部工具.
/// <br>#include 按源文件所在目录解析, 因此编译时定位的是磁盘上的原始 .fx(经
/// <see cref="AssetLoadContext.PhysicalPath"/>), 而非管线预读的字节流.</br>
/// </summary>
public sealed class EffectSourceLoader : IAssetLoader<Effect>
{
  public IEnumerable<string> Extensions { get; } = new[] { ".fx" };

  public Effect Load(AssetLoadContext ctx, Stream stream, string path)
  {
    var sourceFile = ctx.PhysicalPath;
    // EffectContent.Identity 指向真实源文件: 管线从磁盘读取源码, #include (Macros.fxh 等) 相对源文件目录解析.
    var content = new Microsoft.Xna.Framework.Content.Pipeline.Graphics.EffectContent
    {
      Identity = new Microsoft.Xna.Framework.Content.Pipeline.ContentIdentity(sourceFile, nameof(EffectSourceLoader))
    };
    byte[] mgfx = Compile(content, sourceFile + ".runtime.mgfx");
    return new Effect(ctx.GraphicsDevice, mgfx);
  }

  /// <summary>编译核心: 给定已构造的 <see cref="EffectContent"/> 与产物输出名, 完成进程内编译.</summary>
  public static byte[] Compile(EffectContent effectContent, string outputFilename, bool debug = false, string defines = null)
  {
    EffectProcessor processor = new EffectProcessor
    {
      DebugMode = debug ? EffectProcessorDebugMode.Debug : EffectProcessorDebugMode.Auto,
      Defines = defines
    };

    CompiledEffectContent compiled = processor.Process(effectContent, new RuntimeProcessorContext(outputFilename));
    return compiled.GetEffectCode();
  }
}

/// <summary>
/// 运行时处理器上下文: 为 <see cref="EffectProcessor"/> 提供最小依赖环境.
/// </summary>
public sealed class RuntimeProcessorContext : ContentProcessorContext
{
  private readonly ContentBuildLogger _logger = new ContentBuildLogger();
  private readonly OpaqueDataDictionary _parameters = new OpaqueDataDictionary();

  public RuntimeProcessorContext(string outputFilename)
  {
    OutputFilename = outputFilename;
    _logger.LoggerRootDirectory = Path.GetTempPath();
  }

  public override string BuildConfiguration => "Runtime";
  public override string IntermediateDirectory => Path.GetTempPath();
  public override string ProjectDirectory => Path.GetTempPath();
  public override string OutputDirectory => Path.GetTempPath();
  public override string OutputFilename { get; }
  public override ContentBuildLogger Logger => _logger;
  public override OpaqueDataDictionary Parameters => _parameters;
  public override TargetPlatform TargetPlatform => TargetPlatform.Windows;
  public override GraphicsProfile TargetProfile => GraphicsProfile.HiDef;
  public override ContentIdentity SourceIdentity => new ContentIdentity("Runtime");

  public override void AddDependency(string filename) { }

  public override void AddOutputFile(string filename) { }

  public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, IContentImporter importer, IContentProcessor processor)
    => throw new NotSupportedException("运行时 Effect 编译不涉及子资产构建.");

  public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, IContentImporter importer, IContentProcessor processor, string? assetName)
    => throw new NotSupportedException("运行时 Effect 编译不涉及子资产构建.");

  public override TOutput Convert<TInput, TOutput>(TInput input, IContentProcessor processor)
    => throw new NotSupportedException("运行时 Effect 编译不涉及子资产转换.");

  public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
    => throw new NotSupportedException("运行时 Effect 编译不涉及子资产构建.");

  public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
    => throw new NotSupportedException("运行时 Effect 编译不涉及子资产构建.");

  public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
    => throw new NotSupportedException("运行时 Effect 编译不涉及子资产转换.");
}
