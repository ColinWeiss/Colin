namespace Colin.Core.Common
{
  /// <summary>
  /// 为应在 <see cref="Scene.Draw"/> 中渲染的场景模块定义的接口.
  /// <br>标识一个可随场景渲染进行渲染的对象.</br>
  /// <para>[!] 不需要在类内对 <see cref="RawRt"/> 实例化, 
  /// <br>这一操作在 <see cref="SceneModuleList"/> 加入该模块时自动实现.</br></para>
  /// </summary>
  public interface IRenderableISceneModule : IDisposable
  {
    /// <summary>
    /// 未经加工的模块渲染目标.
    /// </summary>
    public RenderTarget2D RawRt { get; set; }

    /// <summary>
    /// 指示源对象是否启用渲染步骤.
    /// </summary>
    public bool RawRtVisible { get; set; }

    /// <summary>
    /// 指示对象是否最终呈现.
    /// <br> <see cref="RawRtVisible"/> 指示其是否于 Rt 上绘制, 但它决定最终是否被绘制在 <see cref="Scene.SceneRenderTarget"/> 上.</br>
    /// </summary>
    public bool Presentation { get; set; }

    /// <summary>
    /// 在 <see cref="RawRt"/> 上进行渲染操作.
    /// <br>[当前Rt] <see cref="RawRt"/>.</br>
    /// </summary>
    public void DoRawRender(GraphicsDevice device, SpriteBatch batch);

    /// <summary>
    /// 在 <see cref="RawRt"/> 要渲染到 <see cref="Scene.SceneRenderTarget"/> 上时执行操作.
    /// <br>[当前Rt] <see cref="Scene.SceneRenderTarget"/>.</br>
    /// </summary>
    public void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch);

    public void InitRenderTarget()
    {
      RawRt?.Dispose();
      RawRt = RenderTargetExt.CreateDefault();
    }
    public void OnClientSizeChanged(object o, EventArgs e)
    {
      InitRenderTarget();
    }
  }
}