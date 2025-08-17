using Colin.Core.Events;

namespace Colin.Core.Modulars.UserInterfaces.Events
{
  public class DivEventNode<T> : EventNode<T> where T : IEventBase
  {
    public Div Div { get; set; }
    public override bool CheckCondition()
    {
      if (Div is null || Div.IsVisible is false || Div.Interact.IsInteractive is false)
        return false;
      if (typeof(T).IsSubclassOf(typeof(MouseArgs)))
      {
        Point mousePos = Point.Zero;
        if (Div.Module is not null && Div is not null)
        {
          mousePos = Div.Module.UICamera.ConvertToWorld(MouseResponder.Position).ToPoint();
        }
        return Div.ContainsScreenPoint(mousePos);
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