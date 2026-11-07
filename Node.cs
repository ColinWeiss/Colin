namespace Colin.Core
{
  /// <summary>
  /// 表示一类节点.
  /// </summary>
  /// <typeparam name="T">节点承载的值类型.</typeparam>
  public class Node<T>
  {
    public T Value;

    public Node() { }

    public Node(T value) => Value = value;

    /// <summary>
    /// 在层级树结构中表示父节点; 在链表结构中表示上一个节点.
    /// <br>[!] 由 <see cref="DoRegister"/> / <see cref="DoInsert"/> / <see cref="DoAppend"/> 维护,
    /// 移除时由 <see cref="DoRemove"/> 置空 —— 事件冒泡 (<c>EventNode.TriggerBubbling</c>) 依赖它.</br>
    /// </summary>
    public Node<T> Last;

    /// <summary>
    /// 表示 <see cref="Children"/> 中的 0 号节点; 无子节点时为 <see langword="null"/>.
    /// </summary>
    public Node<T> Next
    {
      get
      {
        if (_children is null || _children.Count == 0)
          return null;
        return _children[0];
      }
      set
      {
        _children ??= new List<Node<T>>();
        if (_children.Count == 0)
          _children.Add(value);
        else if (value is null)
          _children.RemoveAt(0);
        else
          _children[0] = value;
      }
    }

    private List<Node<T>> _children;

    /// <summary>
    /// 子节点列表.
    /// </summary>
    public List<Node<T>> Children => _children ??= new List<Node<T>>();

    /// <summary>
    /// 于子节点列表末尾附加节点, 并维护 <see cref="Last"/>.
    /// </summary>
    protected void DoAppend(Node<T> node)
    {
      if (node is null)
        return;
      List<Node<T>> children = Children;
      if (children.Contains(node) is false)
        children.Add(node);
      node.Last = this;
    }

    /// <summary>
    /// 于子节点列表指定位置插入节点 (越界夹取), 并维护 <see cref="Last"/>.
    /// </summary>
    protected void DoInsert(int index, Node<T> node)
    {
      if (node is null)
        return;
      List<Node<T>> children = Children;
      children.Remove(node);
      children.Insert(Math.Clamp(index, 0, children.Count), node);
      node.Last = this;
    }

    /// <summary>
    /// 为子树登记子节点, 并维护 <see cref="Last"/>.
    /// <br>[!] 已在列表内时不重复添加 —— 重复登记会让同一次派发把同一节点的处理器触发两遍
    /// (重挂载/重复 Attach 是常见来源).</br>
    /// </summary>
    protected void DoRegister(Node<T> node)
    {
      if (node is null)
        return;
      List<Node<T>> children = Children;
      if (children.Contains(node) is false)
        children.Add(node);
      node.Last = this;
    }

    /// <summary>
    /// 从子节点中移除指定节点, 并清空其 <see cref="Last"/>.
    /// </summary>
    protected void DoRemove(Node<T> node)
    {
      if (node is null)
        return;
      if (_children is not null && _children.Remove(node))
        node.Last = null;
    }
  }
}
