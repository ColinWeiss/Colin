namespace Colin.Core
{
  /// <summary>
  /// 表示允许进行重置操作的对象.
  /// </summary>
  public interface IResetable
  {
    /// <summary>
    /// 启用重置功能.
    /// <br>[!] 该功能在启用后将每帧重置为 <see langword="true"/>.</br>
    /// </summary>
    bool ResetEnable { get; set; }
    void Reset();
  }
}