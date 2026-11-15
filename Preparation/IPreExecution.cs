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

    public void Prepare();
  }

  /// <summary>
  /// 需要主线程执行的预工作项 (图形设备调用不能在预加载后台线程执行).
  /// </summary>
  public interface IMainThreadPreExecution : IPreExecution
  {
    void PrepareMainThread();
  }
}