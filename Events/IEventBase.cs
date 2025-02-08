using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Events
{
  /// <summary>
  /// 表示一个基础事件; 其中包含事件参数.
  /// </summary>
  public interface IEventBase 
  {
    public Type EventType => GetType();

    /// <summary>
    /// 指示该事件的发出者.
    /// </summary>
    public object Sender { get; set; }

    /// <summary>
    /// 指示该事件是否已被捕获.
    /// </summary>
    public bool IsCapture { get; set; }

    /// <summary>
    /// 指示该事件是否需要停止冒泡.
    /// </summary>
    public bool StopBubbling { get; set; }
  }
}