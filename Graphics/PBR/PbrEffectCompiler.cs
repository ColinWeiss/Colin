using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Reflection;

namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// PBR 着色器编译入口: 着色器源码以嵌入资源随程序集分发 (Colin/Graphics/PBR/PbrShader.fx),
  /// 运行时经 MonoGame 内容管线的 EffectProcessor 进程内编译为 MGFX 字节 (无 MGCB/无外部工具).
  /// 与粒子/刀光的着色器路径一致, 改 .fx 后重启或调用 <see cref="Reload"/> 即可生效.
  /// </summary>
  internal static class PbrEffectCompiler
  {
    public const string ResourceName = "Colin.Core.Graphics.PBR.PbrShader.fx";

    private static readonly object SyncRoot = new object();
    private static byte[] _bytecode;

    public static byte[] Compile()
    {
      lock (SyncRoot)
      {
        if (_bytecode is not null)
          return _bytecode;

        string source = LoadSource();
        string sourceFile = Path.Combine(Path.GetTempPath(), $"PbrShader.{Environment.ProcessId}.runtime.fx");
        try
        {
          File.WriteAllText(sourceFile, source, new UTF8Encoding(false));
          EffectContent content = new EffectContent
          {
            Identity = new ContentIdentity(sourceFile, nameof(PbrEffectCompiler)),
            EffectCode = source
          };
          _bytecode = EffectSourceLoader.Compile(
              content,
              Path.Combine(Path.GetTempPath(), "PbrShader.runtime.mgfx"));
          return _bytecode;
        }
        finally
        {
          try
          {
            if (File.Exists(sourceFile))
              File.Delete(sourceFile);
          }
          catch
          {
          }
        }
      }
    }

    public static byte[] Reload()
    {
      lock (SyncRoot)
      {
        _bytecode = null;
        return Compile();
      }
    }

    private static string LoadSource()
    {
      Assembly assembly = typeof(PbrEffectCompiler).Assembly;
      using Stream stream = assembly.GetManifestResourceStream(ResourceName);
      if (stream is null)
      {
        string known = string.Join(", ", assembly.GetManifestResourceNames());
        throw new InvalidOperationException(
            $"PBR 着色器源码资源 '{ResourceName}' 未找到, 当前程序集内资源: {known}");
      }
      using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
      return reader.ReadToEnd();
    }
  }
}
