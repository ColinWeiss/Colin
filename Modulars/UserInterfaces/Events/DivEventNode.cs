using Colin.Core.Events;

namespace Colin.Core.Modulars.UserInterfaces.Events
{
  public class DivEventNode<T> : EventNode<T> where T : IEventBase
  {
    public Div Div { get; set; }
    public override bool CheckCondition()
    {
      if (Div.IsVisible is false || Div.Interact.IsInteractive is false)
        return false;
      if (typeof(T).IsSubclassOf(typeof(MouseArgs)))
      {
        return Div.ContainsScreenPoint(MouseResponder.Position.ToPoint());
      }
      else if (typeof(T).IsSubclassOf(typeof(KeysArgs)))
      {
        return true;
      }
      else
        return true;
    }

    public static DivEventNode<T> operator +(DivEventNode<T> node, EventHandler<T> a)
    {
      node.Event += a;
      return node;
    }

    public static DivEventNode<T> operator -(DivEventNode<T> node, EventHandler<T> a)
    {
      node.Event -= a;
      return node;
    }
  }
}