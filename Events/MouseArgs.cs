namespace Colin.Core.Events
{
  /// <summary>
  /// 鼠标事件参数.
  /// </summary>
  public record MouseArgs : IEventBase
  {
    public object Sender { get; set; }
    public bool IsCapture { get; set; }
    public bool StopBubbling { get; set; }
    /// <summary>
    /// 鼠标位置.
    /// </summary>
    public Vector2 MousePos;
    /// <summary>
    /// 滚轮状态;
    /// <br>上划-1,</br>
    /// <br>不动 0.</br>
    /// <br>下划 1.</br>
    /// </summary>
    public int Wheel;
  }

  public record MouseHoverArgs : MouseArgs;

  /// <summary>
  /// 鼠标左键单击前事件参数.
  /// </summary>
  public record LeftClickingArgs : MouseArgs;

  /// <summary>
  /// 鼠标左键长按事件参数.
  /// </summary>
  public record LeftDownArgs : MouseArgs;

  /// <summary>
  /// 鼠标左键单击后事件参数.
  /// </summary>
  public record LeftClickedArgs : MouseArgs;

  /// <summary>
  /// 鼠标左键松开事件参数.
  /// </summary>
  public record LeftUpArgs : MouseArgs;

  /// <summary>
  /// 鼠标右键单击前事件参数.
  /// </summary>
  public record RightClickingArgs : MouseArgs;

  /// <summary>
  /// 鼠标右键长按事件参数.
  /// </summary>
  public record RightDownArgs : MouseArgs;

  /// <summary>
  /// 鼠标右键单击后事件参数.
  /// </summary>
  public record RightClickedArgs : MouseArgs;

  /// <summary>
  /// 鼠标右键松开事件参数.
  /// </summary>
  public record RightUpArgs : MouseArgs;

  /// <summary>
  /// 鼠标滚轮上划事件参数.
  /// </summary>
  public record ScrollUpArgs : MouseArgs;

  /// <summary>
  /// 鼠标滚轮下划事件参数.
  /// </summary>
  public record ScrollDownArgs : MouseArgs;

  /// <summary>
  /// 鼠标滚轮点击事件参数.
  /// </summary>
  public record ScrollClickedArgs : MouseArgs;

}
