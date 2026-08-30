namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 用户交互界面批次状态机.
  /// <br>追踪当前批次的混合 / 采样 / 深度 / 光栅 / 剪裁 / 变换状态, 请求状态与激活状态完全一致时不产生任何提交,</br>
  /// <br>使同状态的连续绘制 (如同级元素, 同剪裁区域的列表项) 得以合并为单次批次.</br>
  /// <br>[!] 仅可在主线程使用; 于离开界面渲染阶段或切换渲染目标前必须调用 <see cref="Flush"/>.</br>
  /// </summary>
  public static class UIBatch
  {
    /// <summary>
    /// 剪裁测试光栅器状态.
    /// </summary>
    public static readonly RasterizerState ScissorTestRasterizer = new RasterizerState()
    {
      CullMode = CullMode.None,
      ScissorTestEnable = true,
    };

    /// <summary>
    /// 无剪裁测试光栅器状态; 等价于 SpriteBatch 的默认光栅器.
    /// </summary>
    public static readonly RasterizerState DefaultRasterizer = RasterizerState.CullCounterClockwise;

    private static bool _active;
    private static BlendState _blend;
    private static SamplerState _sampler;
    private static DepthStencilState _depth;
    private static RasterizerState _rasterizer;
    private static Rectangle _scissorRectangle;
    private static Matrix _transform;

    /// <summary>
    /// 指示当前是否存在激活的界面批次.
    /// </summary>
    public static bool Active => _active;

    /// <summary>
    /// 请求以指定状态进行绘制; 若与当前激活状态完全一致则不产生任何提交.
    /// </summary>
    /// <param name="blend">混合状态.</param>
    /// <param name="sampler">采样器状态.</param>
    /// <param name="depth">深度模板状态.</param>
    /// <param name="rasterizer">光栅器状态.</param>
    /// <param name="scissorRectangle">剪裁矩形; 仅于光栅器启用剪裁测试时参与比较与设定.</param>
    /// <param name="transform">变换矩阵.</param>
    public static void Request(BlendState blend, SamplerState sampler, DepthStencilState depth, RasterizerState rasterizer, Rectangle scissorRectangle, Matrix transform)
    {
      if (_active
          && _blend == blend
          && _sampler == sampler
          && _depth == depth
          && _rasterizer == rasterizer
          && (rasterizer.ScissorTestEnable is false || _scissorRectangle == scissorRectangle)
          && _transform == transform)
        return;
      Flush();
      if (rasterizer.ScissorTestEnable)
        CoreInfo.Graphics.GraphicsDevice.ScissorRectangle = scissorRectangle;
      CoreInfo.Batch.Begin(SpriteSortMode.Deferred, blend, sampler, depth, rasterizer, transformMatrix: transform);
      _active = true;
      _blend = blend;
      _sampler = sampler;
      _depth = depth;
      _rasterizer = rasterizer;
      _scissorRectangle = scissorRectangle;
      _transform = transform;
    }

    /// <summary>
    /// 提交当前激活的界面批次; 若无激活批次则不做任何操作.
    /// </summary>
    public static void Flush()
    {
      if (_active is false)
        return;
      CoreInfo.Batch.End();
      _active = false;
    }
  }
}
