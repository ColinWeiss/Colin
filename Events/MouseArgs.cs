namespace Colin.Core.Events
{
  /// <summary>
  /// 鼠标事件参数.
  /// </summary>
  public class MouseArgs : IEventBase
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

  /// <summary>
  /// 鼠标左键单击前事件参数.
  /// </summary>
  public class LeftClickingArgs : MouseArgs;

  /// <summary>
  /// 鼠标左键长按事件参数.
  /// </summary>
  public class LeftDownArgs : MouseArgs;

  /// <summary>
  /// 鼠标左键单击后事件参数.
  /// </summary>
  public class LeftClickedArgs : MouseArgs;

  /// <summary>
  /// 鼠标左键松开事件参数.
  /// </summary>
  public class LeftUpArgs : MouseArgs;

  /// <summary>
  /// 鼠标右键单击前事件参数.
  /// </summary>
  public class RightClickingArgs : MouseArgs;

  /// <summary>
  /// 鼠标右键长按事件参数.
  /// </summary>
  public class RightDownArgs : MouseArgs;

  /// <summary>
  /// 鼠标右键单击后事件参数.
  /// </summary>
  public class RightClickedArgs : MouseArgs;

  /// <summary>
  /// 鼠标右键松开事件参数.
  /// </summary>
  public class RightUpArgs : MouseArgs;

  /// <summary>
  /// 鼠标滚轮上划事件参数.
  /// </summary>
  public class ScrollUpArgs : MouseArgs;

  /// <summary>
  /// 鼠标滚轮下划事件参数.
  /// </summary>
  public class ScrollDownArgs : MouseArgs;

  /// <summary>
  /// 鼠标滚轮点击事件参数.
  /// </summary>
  public class ScrollClickedArgs : MouseArgs;

}
