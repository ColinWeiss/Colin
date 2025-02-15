namespace Colin.Core
{
  /// <summary>
  /// 表示一类节点.
  /// </summary>
  public class Node<T>
  {
    /// <summary>
    /// 在层级树结构中表示父节点;
    /// <br>在链表结构中表示上一个节点.</br>
    /// </summary>
    public Node<T> Last;

    /// <summary>
    /// 表示 <see cref="Children"/> 中的0号节点.
    /// </summary>
    public Node<T> Next
    {
      get => Children[0];
      set
      {
        if (Children is null)
        {
          _children = new List<Node<T>>();
          _children.Add(value);
        }
        else if (_children.Count >= 1)
          _children[0] = value;
        else
          _children.Add(value);
      }
    }

    private List<Node<T>> _children;
    /// <summary>
    /// 子节点列表.
    /// </summary>
    public List<Node<T>> Children
    {
      get
      {
        _children ??= new List<Node<T>>();
        return _children;
      }
    }

    /// <summary>
    /// 于链表末尾附加子节点.
    /// </summary>
    protected void DoAppend(Node<T> node)
    {
      if (Next is null)
        Next = node;
      else
        Next.DoAppend(node);
    }

    /// <summary>
    /// 插入节点.
    /// </summary>
    protected void DoInsert(int index, Node<T> node)
    {
      if (_children is null)
        Next = node;
      else
        _children.Insert(index, node);
    }

    /// <summary>
    /// 为子树附加子节点.
    /// </summary>
    protected void DoRegister(Node<T> node)
    {
      if (_children is null)
        Next = node;
      else
        _children.Add(node);
    }

    /// <summary>
    /// 从该节点中删除指定节点.
    /// </summary>
    protected void DoRemove(Node<T> node)
    {
      if (_children is null)
        Console.WriteLine("Error", "EventNode Remove Failed; Children Is Null.");
      else
        _children.Remove(node);
    }
  }
}