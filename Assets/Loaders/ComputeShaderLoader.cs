using Colin.Core.Graphics.Shaders;
using SharpDX.D3DCompiler;

namespace Leemo.Assets;

/// <summary>
/// 计算着色器加载器(Colin 自定义资产类型 <see cref="ComputeShader"/>).
/// <br>.hlsl —— 经 SharpDX.D3DCompiler 在当前进程内编译为 cs_5_0 字节码(原 Asset.LoadComputeShaders
/// 的"源码即时编译"工作流, 编译器随程序集分发, 无任何外部工具依赖)；</br>
/// <br>.cso —— 直接读取预编译字节码(分发模式下可离线编好).</br>
/// </summary>
public sealed class ComputeShaderLoader : IAssetLoader<ComputeShader>
{
    public IEnumerable<string> Extensions { get; } = new[] { ".hlsl", ".cso" };

    public ComputeShader Load(AssetLoadContext ctx, Stream stream, string path)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var bytes = ms.ToArray();

        if (path.EndsWith(".cso", StringComparison.OrdinalIgnoreCase))
            return new ComputeShader(ctx.GraphicsDevice, bytes);

        using ShaderBytecode bytecode = ShaderBytecode.Compile(bytes, "Main", "cs_5_0", ShaderFlags.Debug);
        if (bytecode is null || bytecode.Data is null || bytecode.Data.Length == 0)
            throw new InvalidDataException($"Leemo.Assets: 计算着色器编译无产物 '{path}'.");
        return new ComputeShader(ctx.GraphicsDevice, bytecode.Data);
    }
}
