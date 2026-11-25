namespace Colin.Core.Events
{
  /// <summary>
  /// 一组事件订阅的生命周期容器.
  /// <br>系统/页面销毁时 <see cref="Dispose"/> 一次性退订, 避免场景切换后的悬挂回调.</br>
  /// </summary>
  public sealed class EventScope : IDisposable
  {
    private readonly EventBus _bus;
    private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
    private bool _disposed;

    public EventScope(EventBus bus)
    {
      _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    }

    public EventBus Bus => _bus;

    public IDisposable Subscribe<T>(EventCallback<T> callback, int priority = 0)
      where T : struct, IEventMessage
    {
      if (_disposed)
        throw new ObjectDisposedException(nameof(EventScope));
      IDisposable subscription = _bus.Subscribe(callback, priority);
      _subscriptions.Add(subscription);
      return subscription;
    }

    public void Dispose()
    {
      if (_disposed)
        return;
      _disposed = true;
      for (int index = _subscriptions.Count - 1; index >= 0; index--)
      {
        try
        {
          _subscriptions[index].Dispose();
        }
        catch (Exception exception)
        {
          Console.Log(ConsoleTextType.Error, "EventScope", exception.ToString());
        }
      }
      _subscriptions.Clear();
    }
  }
}
