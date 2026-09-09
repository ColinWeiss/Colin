namespace Leemo.Assets;

/// <summary>
/// 特效源码加载器：.fx —— 经 MonoGame 内容管线的 <c>EffectProcessor</c> 在当前进程内
/// 编译为 MGFX 字节码后构造 <see cref="Effect"/>（原 Asset.LoadEffects 的"启动时编译 .fx"
/// 工作流）。编译核心随 MonoGame.Framework.Content.Pipeline.dll 分发，玩家机器上
/// 不需要安装 mgfxc 等任何外部工具。
/// <br>#include 按源文件所在目录解析，因此编译时定位的是磁盘上的原始 .fx（经
/// <see cref="AssetLoadContext.PhysicalPath"/>），而非管线预读的字节流。</br>
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
        byte[] mgfx = Colin.Core.Graphics.Visual.Particle.Rendering.ParticleEffectCompiler.Compile(content, sourceFile + ".runtime.mgfx");
        return new Effect(ctx.GraphicsDevice, mgfx);
    }
}
