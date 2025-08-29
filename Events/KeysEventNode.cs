namespace Colin.Core.Events
{
  /// <summary>
  /// 键盘事件的 EventNode 封装.
  /// </summary>
  public class KeysEventNode : Node<KeysEventNode>
  {
    public EventNode<KeysClickedArgs> KeysClicked;
    public EventNode<KeysDownArgs> KeysDown;
    public EventNode<KeysClickingArgs> KeysClicking;

    public KeysEventNode()
    {
      KeysClicked = new();
      KeysDown = new();
      KeysClicking = new();
    }

    public void Append(KeysEventNode node)
    {
      KeysClicking.Append(node.KeysClicking);
      KeysDown.Append(node.KeysDown);
      KeysClicked.Append(node.KeysClicked);
    }

    public void Insert(int index, KeysEventNode node)
    {
      KeysClicking.Insert(index, node.KeysClicking);
      KeysDown.Insert(index, node.KeysDown);
      KeysClicked.Insert(index, node.KeysClicked);
    }

    public void Register(KeysEventNode node)
    {
      KeysClicking.Register(node.KeysClicking);
      KeysDown.Register(node.KeysDown);
      KeysClicked.Register(node.KeysClicked);
    }

    public void Remove(KeysEventNode node)
    {
      KeysClicking.Remove(node.KeysClicking);
      KeysDown.Remove(node.KeysDown);
      KeysClicked.Remove(node.KeysClicked);
    }
  }
}