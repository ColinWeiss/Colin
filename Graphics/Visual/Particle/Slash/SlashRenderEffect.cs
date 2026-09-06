using System.Text;

namespace Colin.Core.Graphics.Visual.Particle.Slash
{
  /// <summary>
  /// 刀光渲染 Effect 加载器: 嵌入 MGFX 主路径 + EffectProcessor 进程内编译回退
  /// (与 <see cref="Colin.Core.Graphics.Visual.Particle.Rendering.ParticleRenderEffect"/> 同一套机制).
  /// </summary>
  public static class SlashRenderEffect
  {
    private static Effect _shared;

    /// <summary>共享的刀光渲染 Effect (首次访问时加载).</summary>
    public static Effect GetOrCreate(GraphicsDevice device)
    {
      if (_shared is not null && !_shared.IsDisposed)
        return _shared;

      try
      {
        byte[] mgfx = Convert.FromBase64String(SlashCompiledShaders.RenderEffectBase64);
        _shared = new Effect(device, mgfx);
        Console.WriteLine("Remind", "刀光渲染着色器已从嵌入 MGFX 加载.");
        return _shared;
      }
      catch (Exception exception)
      {
        Console.WriteLine("Error", $"刀光嵌入 MGFX 加载失败: {exception.Message}");
      }

      _shared = new Effect(device, CompileEmbeddedSource());
      return _shared;
    }

    /// <summary>强制重载 (进程内重新编译嵌入源码).</summary>
    public static Effect Reload(GraphicsDevice device)
    {
      Effect recompiled = new Effect(device, CompileEmbeddedSource());
      _shared?.Dispose();
      _shared = recompiled;
      return _shared;
    }

    private static byte[] CompileEmbeddedSource()
    {
      try
      {
        byte[] mgfx = Colin.Core.Graphics.Visual.Particle.Rendering.ParticleEffectCompiler.CompileFromSource(SlashShaderSource.RenderEffectSource);
        Console.WriteLine("Remind", "刀光渲染着色器已由进程内 Effect 编译器生成.");
        return mgfx;
      }
      catch (Exception exception)
      {
        throw new InvalidOperationException("刀光渲染着色器进程内编译失败: " + exception.Message, exception);
      }
    }
  }
}
