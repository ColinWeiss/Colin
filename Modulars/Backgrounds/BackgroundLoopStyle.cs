namespace Colin.Core.Modulars.Backgrounds
{
  /// <summary>
  /// 背景图层循环样式.
  /// <br>当 <see cref="BackgroundLayer.IsLoop"/> 为 <see langword="true"/> 时应用.</br>
  /// </summary>
  public enum BackgroundLoopStyle
  {
    /// <summary>
    /// 上下左右均为连通状态.
    /// </summary>
    FullConnect,
    /// <summary>
    /// 仅左右为连通状态.
    /// </summary>
    LeftRightConnect,
    /// <summary>
    /// 仅上下为连通状态.
    /// </summary>
    TopBottomConnect
  }
}
