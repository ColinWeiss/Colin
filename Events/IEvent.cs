namespace Colin.Core.Events
{
  /// <summary>
  /// <br>事件冒泡系统中的事件对象.</br>
  /// </summary>
  public interface IEvent
  {
    /// <summary>
    /// 指示该事件是否被捕获.
    /// </summary>
    public bool Captured { get; set; }
    /// <summary>
    /// 指示该事件的名称.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 指示该事件是否属于后序遍历事件.
    /// </summary>
    public bool Postorder { get; set; }
  }
}