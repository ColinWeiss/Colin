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

    //派生类型的 + / - 必须自己声明: 基类算子返回 EventNode<T>, 无法赋回 DivEventNode<T>.
    public static DivEventNode<T> operator +(DivEventNode<T> node, EventHandler<T> a)
    {
      if (node is null || a is null)
        return node;
      node.EventList.Add(a);
      node.Event += a;
      return node;
    }

    public static DivEventNode<T> operator -(DivEventNode<T> node, EventHandler<T> a)
    {
      if (node is null || a is null)
        return node;
      node.Event -= a;
      node.EventList.Remove(a);
      return node;
    }

    private static readonly bool _isMouseArgs = typeof(T).IsSubclassOf(typeof(MouseArgs));

    public override bool CheckCondition()
    {
      //可见性用"整条祖先链都可见"判定: 自身 IsVisible 是作者态 (不向下级联),
      //否则隐藏父级的子元素仍然能被点到.
      if (Div is null || Div.Disposed || Div.IsVisibleInTree is false || Div.IsCulled || Div.Interact.IsInteractive is false)
        return false;
      if (UserInterface.InteractionSuspended)
        return false;
      if (_isMouseArgs)
      {
        Point mousePos = Point.Zero;
        if (Div.Module is not null)
          mousePos = Div.Module.UICamera.ConvertToWorld(MouseResponder.Position).ToPoint();
        return Div.ContainsScreenPoint(mousePos);
      }
      return true;
    }

    /// <summary>
    /// 捕获/停止冒泡的登记: 由 <see cref="DivEvents.MouseCapture"/> 等开关决定本节点是否吞掉事件.
    /// <br>放在派发前的拦截点, 保证它先于用户挂的处理器生效 (与旧实现里 DoBlockOut 的注册顺序一致).</br>
    /// </summary>
    protected override void OnBeforeInvoke(T args)
    {
      if (args is null)
        return;
      DivEvents events = Div?.Events;
      if (events is null)
        return;
      if (_isMouseArgs)
      {
        if (events.MouseCapture)
          args.IsCapture = true;
        if (events.MouseBubbling)
          args.StopBubbling = true;
      }
      else
      {
        if (events.KeysCapture)
          args.IsCapture = true;
        if (events.KeysBubbling)
          args.StopBubbling = true;
      }
    }

    void IDivEventChannel.ChannelAppend(IDivEventChannel source) => Append((DivEventNode<T>)(object)source);
    void IDivEventChannel.ChannelInsert(int index, IDivEventChannel source) => Insert(index, (DivEventNode<T>)(object)source);
    void IDivEventChannel.ChannelRegister(IDivEventChannel source) => Register((DivEventNode<T>)(object)source);
    void IDivEventChannel.ChannelRemove(IDivEventChannel source) => Remove((DivEventNode<T>)(object)source);
    void IDivEventChannel.ChannelDispose() => Dispose();
  }
}
