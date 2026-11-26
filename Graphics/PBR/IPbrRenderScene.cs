namespace Colin.Core.Graphics.PBR
{
  /// <summary>
  /// 可渲染场景模块的 3D 能力接口.
  /// <br>实现它的模块拥有一个 PBR 渲染器/相机/光照, 可以在自己的 DoRawRender 里绘制 3D 内容;
  /// ECS 等引擎模块以后可以原生实现该接口接入 3D, 不需要另挂适配模块.</br>
  /// </summary>
  public interface IPbrRenderScene : IRenderableISceneModule
  {
    PbrRenderer Renderer { get; }

    PbrCamera Camera { get; }

    PbrLighting Lighting { get; }

    /// <summary>是否在每帧 Begin 时先画全屏天空盒背景.</summary>
    bool DrawSkybox { get; set; }
  }
}
