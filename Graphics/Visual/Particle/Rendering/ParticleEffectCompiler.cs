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
    /// 在进程内把磁盘上的 .fx 源文件编译为 MGFX 字节码 (DirectX_11 / HiDef).
    /// <br><see cref="EffectContent"/> 的 Identity 指向真实源文件, 因此 #include
    /// (如 Macros.fxh) 相对源文件所在目录解析.</br>
    /// </summary>
    /// <param name="sourceFile">.fx 源文件绝对路径.</param>
    /// <param name="debug">是否生成调试信息.</param>
    /// <param name="defines">可选的预处理宏定义 (分号分隔).</param>
    /// <returns>可直接交由 <c>new Effect(GraphicsDevice, bytes)</c> 加载的 MGFX 字节.</returns>
    /// <exception cref="InvalidContentException">HLSL 存在语法/语义错误时抛出.</exception>
    public static byte[] CompileFromFile(string sourceFile, bool debug = false, string defines = null)
    {
      EffectContent effectContent = new EffectContent
      {
        Identity = new ContentIdentity(sourceFile, nameof(ParticleEffectCompiler))
      };
      return Compile(effectContent, Path.Combine(Path.GetTempPath(), Path.GetFileName(sourceFile) + ".runtime.mgfx"));
    }

    /// <summary>
    /// 在进程内把 HLSL 源码字符串编译为 MGFX 字节码 (DirectX_11 / HiDef).
    /// <br>源码写入临时文件后交由 <see cref="EffectProcessor"/> 编译 (管线要求文件路径形式的身份标识),
    /// 编译本身完全发生在当前进程内. 源码为字符串时不含相对 #include 语义, 需要包含其他文件的
    /// 源码请改用 <see cref="CompileFromFile"/>.</br>
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

        return Compile(effectContent, outputFile);
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

    /// <summary>编译核心: 给定已构造的 <see cref="EffectContent"/> 与产物输出名, 完成进程内编译.</summary>
    public static byte[] Compile(EffectContent effectContent, string outputFilename, bool debug = false, string defines = null)
    {
      return EffectSourceLoader.Compile(effectContent, outputFilename, debug, defines);
    }
  }
}