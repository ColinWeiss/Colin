using Colin.Core.Modulars.UserInterfaces.Events;
using DeltaMachine.Core.GameContents.UserInterfaces.Gameplays;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Colin.Core.Events
{
  /// <summary>
  /// 事件节点.
  /// 包含触发事件/事件处理办法.
  /// </summary>
  public class EventNode<T> : Node<T> where T : IEventBase
  {
    /// <summary>
    /// 获取该节点对应的事件类型.
    /// </summary>
    public Type EventType => typeof(T);

    /// <summary>
    /// 指示该节点对应的事件.
    /// </summary>
    public event EventHandler<T> Event;

    public static EventNode<T> operator +(EventNode<T> node, EventHandler<T> a)
    {
      node.Event += a;
      return node;
    }

    public static EventNode<T> operator -(EventNode<T> node, EventHandler<T> a)
    {
      node.Event -= a;
      return node;
    }

    /// <summary>
    /// 触发条件检查.
    /// </summary>
    public virtual bool CheckCondition() => true;

    /// <summary>
    /// 触发事件捕获流程.
    /// </summary>
    public virtual void TriggerCapture(T args)
    {
      if (CheckCondition())
      {
        Node<T> node;
        for (int index = Children.Count - 1; index >= 0; index--)
        {
          node = Children[index];
          if (node is EventNode<T> eventNode)
          {
            if (args.IsCapture)
              break;
            eventNode.TriggerCapture(args); //播子节点.
          }
        }
        if (args.IsCapture is false)
          Event?.Invoke(args.Sender, args); //执行自己事件.
        if (Children.Count <= 0&& args.IsCapture is false)
          TriggerBubbling(args);
      }
    }

    /// <summary>
    /// 触发事件冒泡流程.
    /// </summary>
    /// <param name="args"></param>
    public virtual void TriggerBubbling(T args)
    {
      if (args.StopBubbling is false && Last is not null)
      {
        Event?.Invoke(args.Sender, args); //执行自己事件.
        if (Last is EventNode<T> eventNode)
          eventNode.TriggerBubbling(args); //播到上一级.
      }
    }

    public void Append(EventNode<T> node)
      => DoAppend(node);

    public void Insert(int index, EventNode<T> node)
      => DoInsert(index, node);

    public void Register(EventNode<T> node)
      => DoRegister(node);

    public void Remove(EventNode<T> node)
      => DoRemove(node);
  }
}