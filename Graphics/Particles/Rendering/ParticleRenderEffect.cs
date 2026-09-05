using System.Text;

namespace Particle.Rendering
{
  /// <summary>
  /// 粒子渲染 Effect 加载器: 按优先级依次尝试两条路径, 保证稳定的同时提供运行时编译特性.
  /// <list type="number">
  /// <item>加载嵌入的预编译 MGFX 二进制 (<see cref="ParticleCompiledShaders"/>, 零依赖主路径);</item>
  /// <item><b>进程内编译特性</b>: 经 <see cref="ParticleEffectCompiler"/> (EffectProcessor 同源核心,
  /// ShaderResult + EffectObject) 直接编译嵌入的 HLSL 源码 —— 无 CLI、无临时文件、无 MGCB;
  /// 修改 <see cref="ParticleShaderSource.RenderEffectSource"/> 后调用
  /// <see cref="Reload"/> 即可热更新渲染管线.</item>
  /// </list>
  /// <br>两条路径均不依赖外部文件, 也不触碰 Colin.Core 自有的资源加载流程.</br>
  /// </summary>
  public static class ParticleRenderEffect
  {
    private static Effect _shared;

    /// <summary>当前 Effect 的来源 (诊断: "嵌入 MGFX" / "进程内编译").</summary>
    public static string LastSource { get; private set; } = "未加载";

    /// <summary>共享的粒子渲染 Effect (首次访问时加载, 后续复用).</summary>
    public static Effect GetOrCreate(GraphicsDevice device)
    {
      if (_shared is not null && !_shared.IsDisposed)
        return _shared;

      // —— 主路径: 嵌入的预编译 MGFX ——
      try
      {
        byte[] mgfx = Convert.FromBase64String(ParticleCompiledShaders.RenderEffectBase64);
        _shared = new Effect(device, mgfx);
        LastSource = "嵌入 MGFX";
        Console.WriteLine("Remind", "粒子渲染着色器已从嵌入 MGFX 加载.");
        return _shared;
      }
      catch (Exception exception)
      {
        Console.WriteLine("Error", $"嵌入 MGFX 加载失败: {exception.Message}");
      }

      // —— 特性路径: EffectProcessor 同源进程内编译 ——
      _shared = new Effect(device, CompileEmbeddedSource());
      LastSource = "进程内编译";
      return _shared;
    }

    /// <summary>
    /// 强制重载: 用进程内编译器重新编译嵌入的 HLSL 源码并替换共享 Effect
    /// (编辑器热更新渲染管线用).
    /// </summary>
    public static Effect Reload(GraphicsDevice device)
    {
      byte[] mgfx = CompileEmbeddedSource();
      Effect recompiled = new Effect(device, mgfx);
      _shared?.Dispose();
      _shared = recompiled;
      LastSource = "进程内编译";
      return _shared;
    }

    /// <summary>编译嵌入的 HLSL 源码 (失败时抛出含行号的异常).</summary>
    private static byte[] CompileEmbeddedSource()
    {
      try
      {
        byte[] mgfx = ParticleEffectCompiler.CompileFromSource(ParticleShaderSource.RenderEffectSource);
        Console.WriteLine("Remind", "粒子渲染着色器已由进程内 Effect 编译器生成.");
        return mgfx;
      }
      catch (Exception exception)
      {
        string diagnostics = ParticleEffectCompiler.LastDiagnostics;
        string message = "粒子渲染着色器进程内编译失败: " + exception.Message;
        if (!string.IsNullOrEmpty(diagnostics))
          message += Environment.NewLine + diagnostics;
        Console.WriteLine("Error", message);
        throw new InvalidOperationException(message, exception);
      }
    }
  }
}
