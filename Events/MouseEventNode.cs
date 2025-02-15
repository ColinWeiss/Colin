namespace Colin.Core.Events
{
  /// <summary>
  /// 鼠标事件的 EventNode 封装.
  /// </summary>
  public class MouseEventNode : Node<MouseEventNode>
  {
    public EventNode<LeftClickedArgs> LeftClicked;
    public EventNode<LeftClickingArgs> LeftClicking;
    public EventNode<LeftDownArgs> LeftDown;
    public EventNode<LeftUpArgs> LeftUp;
    public EventNode<RightClickedArgs> RightClicked;
    public EventNode<RightClickingArgs> RightClicking;
    public EventNode<RightDownArgs> RightDown;
    public EventNode<RightUpArgs> RightUp;
    public EventNode<ScrollDownArgs> ScrollDown;
    public EventNode<ScrollUpArgs> ScrollUp;

    public MouseEventNode() : base()
    {
      LeftClicked = new EventNode<LeftClickedArgs>();
      LeftClicking = new EventNode<LeftClickingArgs>();
      LeftDown = new EventNode<LeftDownArgs>();
      LeftUp = new EventNode<LeftUpArgs>();
      RightClicked = new EventNode<RightClickedArgs>();
      RightClicking = new EventNode<RightClickingArgs>();
      RightDown = new EventNode<RightDownArgs>();
      RightUp = new EventNode<RightUpArgs>();
      ScrollDown = new EventNode<ScrollDownArgs>();
      ScrollUp = new EventNode<ScrollUpArgs>();
    }

    public void Append(MouseEventNode node)
    {
      LeftClicked.Append(node.LeftClicked);
      LeftClicking.Append(node.LeftClicking);
      LeftDown.Append(node.LeftDown);
      LeftUp.Append(node.LeftUp);
      RightClicked.Append(node.RightClicked);
      RightClicking.Append(node.RightClicking);
      RightDown.Append(node.RightDown);
      RightUp.Append(node.RightUp);
      ScrollDown.Append(node.ScrollDown);
      ScrollUp.Append(node.ScrollUp);
    }

    public void Insert(int index, MouseEventNode node)
    {
      LeftClicked.Insert(index, node.LeftClicked);
      LeftClicking.Insert(index, node.LeftClicking);
      LeftDown.Insert(index, node.LeftDown);
      LeftUp.Insert(index, node.LeftUp);
      RightClicked.Insert(index, node.RightClicked);
      RightClicking.Insert(index, node.RightClicking);
      RightDown.Insert(index, node.RightDown);
      RightUp.Insert(index, node.RightUp);
      ScrollDown.Insert(index, node.ScrollDown);
      ScrollUp.Insert(index, node.ScrollUp);
    }

    public void Register(MouseEventNode node)
    {
      LeftClicked.Register(node.LeftClicked);
      LeftClicking.Register(node.LeftClicking);
      LeftDown.Register(node.LeftDown);
      LeftUp.Register(node.LeftUp);
      RightClicked.Register(node.RightClicked);
      RightClicking.Register(node.RightClicking);
      RightDown.Register(node.RightDown);
      RightUp.Register(node.RightUp);
      ScrollDown.Register(node.ScrollDown);
      ScrollUp.Register(node.ScrollUp);
    }

    public void Remove(MouseEventNode node)
    {
      LeftClicked.Remove(node.LeftClicked);
      LeftClicking.Remove(node.LeftClicking);
      LeftDown.Remove(node.LeftDown);
      LeftUp.Remove(node.LeftUp);
      RightClicked.Remove(node.RightClicked);
      RightClicking.Remove(node.RightClicking);
      RightDown.Remove(node.RightDown);
      RightUp.Remove(node.RightUp);
      ScrollDown.Remove(node.ScrollDown);
      ScrollUp.Remove(node.ScrollUp);
    }
  }
}