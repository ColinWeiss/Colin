namespace Colin.Core.Common
{
  /// <summary>
  /// 标识一个用于程序检查的对象.
  /// </summary>
  public interface IProgramChecker
  {
    /// <summary>
    /// 执行检查.
    /// </summary>
    void Check();
  }
}