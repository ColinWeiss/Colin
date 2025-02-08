using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Events
{
  /// <summary>
  /// 键盘键位事件参数.
  /// </summary>
  public class KeysArgs : IEventBase
  {
    public object Sender { get; set; }
    public bool IsCapture { get; set; }
    public bool StopBubbling { get; set; }
    /// <summary>
    /// 键位.
    /// </summary>
    public Keys Keys;
  }
  /// <summary>
  /// 表示键盘按键被点击时的事件参数.
  /// </summary>
  public class KeysClickingArgs : KeysArgs
  {
    /// <summary>
    /// 初始化 KeysClickingArgs 类的新实例.
    /// </summary>
    public KeysClickingArgs()
    {
    }
  }

  /// <summary>
  /// 表示键盘按键被按下时的事件参数.
  /// </summary>
  public class KeysDownArgs : KeysArgs
  {
    /// <summary>
    /// 初始化 KeysDownArgs 类的新实例.
    /// </summary>
    public KeysDownArgs()
    {
    }
  }

  /// <summary>
  /// 表示键盘按键被点击后的事件参数.
  /// </summary>
  public class KeysClickedArgs : KeysArgs
  {
    /// <summary>
    /// 初始化 KeysClickedArgs 类的新实例.
    /// </summary>
    public KeysClickedArgs()
    {
    }
  }

}