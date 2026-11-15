namespace Colin.Core.Preparation
{
  /// <summary>
  /// 表示一个预工作项.
  /// </summary>
  public interface IPreExecution
  {
    /// <summary>
    /// 指示该预工作项的名称.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 指示该预工作项的当前进度.
    /// </summary>
    float Progress { get; set; }

    /// <summary>
    /// 是否必须由主线程执行.
    /// <br>凡创建/释放 RenderTarget、直接读写 <c>GraphicsDevice</c> 的工作都应返回 <see langword="true"/>:
    /// 启动阶段加载界面每帧都在主线程提交 SpriteBatch, 后台线程再动图形设备会撞顶点声明与呈现状态.</br>
    /// <br>默认 <see langword="false"/> (后台线程); 主线程任务由 <see cref="Preparator"/> 在 Update 段代跑, 后台任务原地等待.</br>
    /// </summary>
    bool OnMainThread => false;

    public void Prepare();
  }
}
