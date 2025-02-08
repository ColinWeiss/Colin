using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

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
  public class LeftClickingArgs : MouseArgs
  {
    /// <summary>
    /// 初始化一个新的 LeftClickingArgs 实例.
    /// </summary>
    public LeftClickingArgs()
    {
    }
  }

  /// <summary>
  /// 鼠标左键长按事件参数.
  /// </summary>
  public class LeftDownArgs : MouseArgs
  {
    /// <summary>
    /// 初始化一个新的 LeftDownArgs 实例.
    /// </summary>
    public LeftDownArgs()
    {
    }
  }

  /// <summary>
  /// 鼠标左键单击后事件参数.
  /// </summary>
  public class LeftClickedArgs : MouseArgs
  {
    /// <summary>
    /// 初始化一个新的 LeftClickedArgs 实例.
    /// </summary>
    public LeftClickedArgs()
    {
    }
  }

  /// <summary>
  /// 鼠标左键松开事件参数.
  /// </summary>
  public class LeftUpArgs : MouseArgs
  {
    /// <summary>
    /// 初始化一个新的 LeftUpArgs 实例.
    /// </summary>
    public LeftUpArgs()
    {
    }
  }

  /// <summary>
  /// 鼠标右键单击前事件参数.
  /// </summary>
  public class RightClickingArgs : MouseArgs
  {
    /// <summary>
    /// 初始化一个新的 RightClickingArgs 实例.
    /// </summary>
    public RightClickingArgs()
    {
    }
  }

  /// <summary>
  /// 鼠标右键长按事件参数.
  /// </summary>
  public class RightDownArgs : MouseArgs
  {
    /// <summary>
    /// 初始化一个新的 RightDownArgs 实例.
    /// </summary>
    public RightDownArgs()
    {
    }
  }

  /// <summary>
  /// 鼠标右键单击后事件参数.
  /// </summary>
  public class RightClickedArgs : MouseArgs
  {
    /// <summary>
    /// 初始化一个新的 RightClickedArgs 实例.
    /// </summary>
    public RightClickedArgs()
    {
    }
  }

  /// <summary>
  /// 鼠标右键松开事件参数.
  /// </summary>
  public class RightUpArgs : MouseArgs
  {
    /// <summary>
    /// 初始化一个新的 RightUpArgs 实例.
    /// </summary>
    public RightUpArgs()
    {
    }
  }

  /// <summary>
  /// 鼠标滚轮上划事件参数.
  /// </summary>
  public class ScrollUpArgs : MouseArgs
  {
    /// <summary>
    /// 初始化一个新的 WheelUpArgs 实例.
    /// </summary>
    public ScrollUpArgs()
    {
    }
  }

  /// <summary>
  /// 鼠标滚轮下划事件参数.
  /// </summary>
  public class ScrollDownArgs : MouseArgs
  {
    /// <summary>
    /// 初始化一个新的 WheelDownArgs 实例.
    /// </summary>
    public ScrollDownArgs()
    {
    }
  }
}
