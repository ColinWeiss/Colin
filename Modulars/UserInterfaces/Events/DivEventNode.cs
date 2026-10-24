using Colin.Core.Events;

namespace Colin.Core.Modulars.UserInterfaces.Events
{
  /// <summary>
  /// 事件节点束的统一操作通道.
  /// <br><see cref="DivEvents"/> 持有一组参数类型互异的节点, 依靠该非泛型接口把逐节点手写的 Append/Insert/Register/Remove/Dispose 收敛为数组循环.</br>
  /// </summary>
  public interface IDivEventChannel
  {
    Div Div { get; set; }
    void ChannelAppend(IDivEventChannel source);
    void ChannelInsert(int index, IDivEventChannel source);
    void ChannelRegister(IDivEventChannel source);
    void ChannelRemove(IDivEventChannel source);
    void ChannelDispose();
  }

  public class DivEventNode<T> : EventNode<T>, IDivEventChannel where T : IEventBase
  {
    public Div Div { get; set; }

    private static readonly bool _isMouseArgs = typeof(T).IsSubclassOf(typeof(MouseArgs));

    public override bool CheckCondition()
    {
      if (Div is null || Div.Disposed || Div.IsVisible is false || Div.IsCulled || Div.Interact.IsInteractive is false)
        return false;
      if (UserInterface.InteractionSuspended)
        return false;
      if (_isMouseArgs)
      {
        Point mousePos = Point.Zero;
        if (Div.Module is not null && Div is not null)
        {
          mousePos = Div.Module.UICamera.ConvertToWorld(MouseResponder.Position).ToPoint();
        }
        return Div.ContainsScreenPoint(mousePos);
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

    void IDivEventChannel.ChannelAppend(IDivEventChannel source) => Append((DivEventNode<T>)(object)source);
    void IDivEventChannel.ChannelInsert(int index, IDivEventChannel source) => Insert(index, (DivEventNode<T>)(object)source);
    void IDivEventChannel.ChannelRegister(IDivEventChannel source) => Register((DivEventNode<T>)(object)source);
    void IDivEventChannel.ChannelRemove(IDivEventChannel source) => Remove((DivEventNode<T>)(object)source);
    void IDivEventChannel.ChannelDispose() => Dispose();
  }
}