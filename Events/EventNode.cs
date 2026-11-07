namespace Colin.Core.Events
{
  /// <summary>
  /// 事件节点.
  /// 包含触发事件/事件处理办法.
  /// </summary>
  public class EventNode<T> : Node<T>, IDisposable where T : IEventBase
  {
    public EventNode()
    {
    }

    /// <summary>
    /// 获取该节点对应的事件类型.
    /// </summary>
    public Type EventType => typeof(T);

    /// <summary>
    /// 指示该节点对应的事件.
    /// </summary>
    protected event EventHandler<T> Event;

    private List<EventHandler<T>> _eventList;

    /// <summary>
    /// 已登记的事件处理器 (延迟创建: 绝大多数事件节点没有任何处理器, 不该为此各付一个 List).
    /// </summary>
    public List<EventHandler<T>> EventList => _eventList ??= new List<EventHandler<T>>();

    public static EventNode<T> operator +(EventNode<T> node, EventHandler<T> a)
    {
      if (node is null || a is null)
        return node;
      node.EventList.Add(a);
      node.Event += a;
      return node;
    }

    public static EventNode<T> operator -(EventNode<T> node, EventHandler<T> a)
    {
      if (node is null || a is null)
        return node;
      node.Event -= a;
      node._eventList?.Remove(a);
      return node;
    }

    /// <summary>
    /// 只触发本节点自身的事件处理, 不走捕获/冒泡级联.
    /// <br>悬停进出这类由元素本地计算出的边缘事件用它, 别从场景泵那边全树乱窜; 事件发送者取 args.Sender.</br>
    /// </summary>
    public void TriggerSelf(T args)
      => Event?.Invoke(args.Sender, args);

    /// <summary>
    /// 触发条件检查.
    /// </summary>
    public virtual bool CheckCondition() => true;

    /// <summary>
    /// 执行本节点处理器之前的拦截点 (捕获/停止冒泡的登记处; 保证拦截先于用户处理器生效).
    /// </summary>
    protected virtual void OnBeforeInvoke(T args) { }

    /// <summary>
    /// 触发事件捕获流程.
    /// <br>先向子节点 (逆序, 即绘制顺序的最上层优先) 传播, 再执行本节点的处理器; 任一节点置位
    /// <see cref="IEventBase.IsCapture"/> 或 <see cref="IEventBase.StopBubbling"/> 都会终止后续传播 ——</br>
    /// <br>祖先的机会就在本次扫描里, 所以这里不再从叶子额外发起冒泡 (否则同一处理器会被触发两遍).</br>
    /// </summary>
    public virtual void TriggerCapture(T args)
    {
      if (CheckCondition() is false)
        return;
      List<Node<T>> children = Children;
      //[!] 派发期间集合会被改: 处理器里 Remove/Dispose/Attach 都很常见 (关弹窗、列表增删、Dispose 自摘除),
      //    而这里刻意不做快照 (悬停事件每帧整棵派发, 一次 ToArray 就是每节点每帧一次分配).
      //    常态走 O(1) 的下标递减; 一旦发现集合被改过, 就用"刚派发过的节点"身份重新定位,
      //    既不越界, 也不会因为节点下移而被重复派发.
      int index = children.Count - 1;
      while (index >= 0)
      {
        if (args.IsCapture || args.StopBubbling)
          break;
        if (index >= children.Count)
        {
          index = children.Count - 1;
          continue;
        }
        Node<T> node = children[index];
        int countBefore = children.Count;
        if (node is EventNode<T> eventNode)
          eventNode.TriggerCapture(args);
        if (children.Count == countBefore)
        {
          index--;
          continue;
        }
        int position = children.IndexOf(node);
        index = position < 0 ? children.Count - 1 : position - 1;
      }
      if (args.IsCapture || args.StopBubbling)
        return;
      OnBeforeInvoke(args);
      Event?.Invoke(args.Sender, args);
    }

    /// <summary>
    /// 触发事件冒泡流程: 从本节点沿 <see cref="Node{T}.Last"/> 向父级传播.
    /// <br>[!] 场景泵的常规派发走 <see cref="TriggerCapture"/> (一次扫描已覆盖祖先), 只在自定义派发里显式使用它.</br>
    /// </summary>
    public virtual void TriggerBubbling(T args)
    {
      if (args.StopBubbling)
        return;
      Event?.Invoke(args.Sender, args);
      if (Last is EventNode<T> eventNode)
        eventNode.TriggerBubbling(args);
    }

    public void Append(EventNode<T> node)
      => DoAppend(node);

    public void Insert(int index, EventNode<T> node)
      => DoInsert(index, node);

    public void Register(EventNode<T> node)
      => DoRegister(node);

    public void Remove(EventNode<T> node)
      => DoRemove(node);

    public void Dispose()
    {
      if (_eventList is not null)
      {
        for (int index = 0; index < _eventList.Count; index++)
          Event -= _eventList[index];
        _eventList.Clear();
      }
      //[!] 子节点 Dispose 会把自己从本列表里摘掉 —— 正序 + 活 Count 会漏掉后面那些 (被跳过却再也不会有人处理).
      //    从尾部取: 摘掉了就少一个, 没摘掉就强制移除, 两者都保证收敛.
      List<Node<T>> children = Children;
      while (children.Count > 0)
      {
        Node<T> child = children[children.Count - 1];
        if (child is EventNode<T> sub)
          sub.Dispose();
        if (children.Count > 0 && ReferenceEquals(children[children.Count - 1], child))
          children.RemoveAt(children.Count - 1);
      }
      //自摘除: 否则父级的事件树里会残留一个指向已释放 Div 的节点 (每次 SetRoot/重建都会多留一份).
      if (Last is not null)
      {
        List<Node<T>> siblings = Last.Children;
        siblings.Remove(this);
        Last = null;
      }
    }
  }
}
