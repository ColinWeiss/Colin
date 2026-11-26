namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// 让 PBR 模型接入 Leemo 资产管线.
  /// <br>[!] 模型体积通常很大 (贴图解码 + GPU 上传), 默认 <see cref="AllowPreload"/> 为 false:
  /// 启动期的目录预加载会跳过 .glb/.gltf, 但显式 Assets.Manager.Load&lt;PbrModel&gt;(路径) 仍然有效.</br>
  /// </summary>
  public sealed class PbrModelLoader : IAssetLoader<PbrModel>, IPathFiltered
  {
    public IEnumerable<string> Extensions { get; } = new[] { ".glb", ".gltf" };

    /// <summary>是否允许启动期目录预加载. 默认关闭, 避免一次解码/上传所有模型.</summary>
    public bool AllowPreload { get; set; }

    public bool Accepts(string virtualPath) => AllowPreload;

    public PbrModel Load(AssetLoadContext context, Stream stream, string path)
    {
      return PbrModel.Load(context.GraphicsDevice, context.PhysicalPath);
    }
  }
}
