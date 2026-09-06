using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Colin.Core.Graphics.Visual.Particle.Rendering
{
  /// <summary>
  /// 进程内 Effect 编译器 (用户要求的核心特性): 直接调用 MonoGame 内容管线的
  /// <see cref="EffectProcessor.Process"/> —— 其内部即 ShaderResult + EffectObject
  /// 的进程内编译核心 (不经 CLI 工具), 运行时把 HLSL 源码编译为 MGFX 字节码.
  /// <br>依赖: MonoGame.Framework.Content.Pipeline.dll (vendor 于 Colin/Libs) +
  /// SharpDX.D3DCompiler (DirectX_11 profile 的着色器编译后端).</br>
  /// </summary>
  public static class ParticleEffectCompiler
  {
    /// <summary>编译诊断 (最近一次编译的警告与错误, 供编辑器显示).</summary>
    public static string LastDiagnostics { get; private set; } = string.Empty;

    /// <summary>
    /// 在进程内把 HLSL 源码编译为 MGFX 字节码 (DirectX_11 / HiDef).
    /// <br>源码写入临时文件后交由 <see cref="EffectProcessor"/> 编译 (管线要求文件路径形式的身份标识),
    /// 编译本身完全发生在当前进程内.</br>
    /// </summary>
    /// <param name="effectSource">HLSL 源码 (与 ParticleRender.fx 同构).</param>
    /// <param name="debug">是否生成调试信息.</param>
    /// <param name="defines">可选的预处理宏定义 (分号分隔).</param>
    /// <returns>可直接交由 <c>new Effect(GraphicsDevice, bytes)</c> 加载的 MGFX 字节.</returns>
    /// <exception cref="InvalidContentException">HLSL 存在语法/语义错误时抛出.</exception>
    public static byte[] CompileFromSource(string effectSource, bool debug = false, string defines = null)
    {
      string sourceFile = Path.Combine(Path.GetTempPath(), "ParticleRender.runtime.fx");
      string outputFile = Path.Combine(Path.GetTempPath(), "ParticleRender.runtime.mgfx");
      try
      {
        File.WriteAllText(sourceFile, effectSource, new UTF8Encoding(false));

        EffectContent effectContent = new EffectContent
        {
          Identity = new ContentIdentity(sourceFile, "ParticleEffectCompiler"),
          EffectCode = effectSource
        };

        EffectProcessor processor = new EffectProcessor
        {
          DebugMode = debug ? EffectProcessorDebugMode.Debug : EffectProcessorDebugMode.Auto,
          Defines = defines
        };

        CompiledEffectContent compiled = processor.Process(effectContent, new RuntimeProcessorContext(outputFile));
        LastDiagnostics = "编译完成.";
        return compiled.GetEffectCode();
      }
      finally
      {
        try
        {
          if (File.Exists(sourceFile))
            File.Delete(sourceFile);
          if (File.Exists(outputFile))
            File.Delete(outputFile);
        }
        catch
        {
          // 临时文件清理失败可忽略.
        }
      }
    }

    /// <summary>
    /// 运行时处理器上下文: 为 <see cref="EffectProcessor"/> 提供最小依赖环境.
    /// </summary>
    private sealed class RuntimeProcessorContext : ContentProcessorContext
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
      public override ContentIdentity SourceIdentity => new ContentIdentity("ParticleRender.runtime.fx");

      public override void AddDependency(string filename) { }

      public override void AddOutputFile(string filename) { }

      public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, IContentImporter importer, IContentProcessor processor)
        => throw new NotSupportedException("粒子效果编译不涉及子资产构建.");

      public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, IContentImporter importer, IContentProcessor processor, string? assetName)
        => throw new NotSupportedException("粒子效果编译不涉及子资产构建.");

      public override TOutput Convert<TInput, TOutput>(TInput input, IContentProcessor processor)
        => throw new NotSupportedException("粒子效果编译不涉及子资产转换.");

      public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        => throw new NotSupportedException("粒子效果编译不涉及子资产构建.");

      public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
        => throw new NotSupportedException("粒子效果编译不涉及子资产构建.");

      public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        => throw new NotSupportedException("粒子效果编译不涉及子资产转换.");
    }
  }
}
