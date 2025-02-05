using DeltaMachine.Core.GameContents.UserInterfaces.Gameplays;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Colin.Core.Events
{
  /// <summary>
  /// 事件节点.
  /// 包含触发事件/事件处理办法.
  /// </summary>
  public class EventNode<T> where T : IEventBase
  {
    /// <summary>
    /// 在层级树事件结构中表示父节点;
    /// <br>在链表事件结构中表示上一个节点.</br>
    /// </summary>
    public EventNode<T> Last;

    /// <summary>
    /// 表示 <see cref="Children"/> 中的0号节点.
    /// </summary>
    public EventNode<T> Next
    {
      get => Children[0];
      set
      {
        if (Children is null)
        {
          _children = new List<EventNode<T>>();
        }
        _children[0] = value;
      }
    }

    private List<EventNode<T>> _children;
    /// <summary>
    /// 子节点列表.
    /// </summary>
    public List<EventNode<T>> Children
    {
      get
      {
        _children ??= new List<EventNode<T>>();
        return _children;
      }
    }

    /// <summary>
    /// 获取该节点对应的事件类型.
    /// </summary>
    public Type EventType => typeof(T);

    /// <summary>
    /// 指示该节点对应的事件.
    /// </summary>
    public event EventHandler<T> Event;

    /// <summary>
    /// 触发条件检查.
    /// </summary>
    public virtual bool CheckCondition() => true;
    /// <summary>
    /// 触发事件.
    /// </summary>
    public virtual void Trigger(T args)
    {
      if (CheckCondition())
      {
        foreach (var child in _children)
        {
          child.Trigger(args); //播子节点.
          if (args.IsHandled)
            break;
        }
        if (args.IsHandled is false)
          Event?.Invoke(args.Sender, args); //播自己.
        if (!args.IsHandled && Last is not null)
        {
          Last.Trigger(args); //播到上一级.
        }
      }
    }

    /// <summary>
    /// 为节点附加子节点.
    /// </summary>
    public void Append(EventNode<T> node)
    {
      if (_children is null)
        Next = node;
      else
        _children.Add(node);
    }

    /// <summary>
    /// 从该节点中删除指定节点.
    /// </summary>
    public void Remove(EventNode<T> node)
    {
      if (_children is null)
        Console.WriteLine("Error", "EventNode Remove Failed; Children Is Null.");
      else
        _children.Remove(node);
    }
  }
}