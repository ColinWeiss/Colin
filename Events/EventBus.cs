using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Colin.Core.Events
{
  /// <summary>
  /// 事件消息标记. 所有走 <see cref="EventBus"/> 的事件都应是 readonly struct 并实现该接口.
  /// <br>它只表示"一件已经发生的事", 不承载返回值; 需要返回值的请求请用接口/命令管线.</br>
  /// </summary>
  public interface IEventMessage
  {
  }

  /// <summary>
  /// 事件处理回调. 使用 <c>in</c> 传递结构体, 避免每次派发复制整个事件.
  /// </summary>
  public delegate void EventCallback<T>(in T message) where T : struct, IEventMessage;

  /// <summary>事件总线统计快照.</summary>
  public readonly struct EventBusStats
  {
    public readonly long Published;
    public readonly long Dispatched;
    public readonly long HandlerErrors;
    public readonly int Pending;
    public readonly int Channels;

    public EventBusStats(long published, long dispatched, long handlerErrors, int pending, int channels)
    {
      Published = published;
      Dispatched = dispatched;
      HandlerErrors = handlerErrors;
      Pending = pending;
      Channels = channels;
    }
  }

  /// <summary>最近一次事件派发的追踪记录 (只在事件监视器打开时记录).</summary>
  public readonly struct EventTraceEntry
  {
    public readonly long Sequence;
    public readonly Type Type;
    public readonly int Subscribers;
    public readonly double ElapsedMs;
    public readonly string Error;

    public EventTraceEntry(long sequence, Type type, int subscribers, double elapsedMs, string error)
    {
      Sequence = sequence;
      Type = type;
      Subscribers = subscribers;
      ElapsedMs = elapsedMs;
      Error = error;
    }
  }

  /// <summary>
  /// 场景级事件总线.
  /// <br>发布 = 入队, 在 <see cref="Flush"/> (Scene.Update 末尾) 统一派发; 需要同帧门控时用 <see cref="PublishNow"/>. </br>
  /// <br>订阅必须通过返回的 token 或 <see cref="EventScope"/> 释放, 否则场景切换会泄漏; <see cref="Clear"/> 是兜底.</br>
  /// </summary>
  public sealed class EventBus
  {
    private const int MaxEventsPerFlush = 4096;
    private const int TraceCapacity = 64;

    private abstract class Channel
    {
      public abstract int Count { get; }

      public abstract void Dispatch(object message, EventBus bus);

      public abstract bool Remove(int id);

      public abstract void Clear();
    }

    private struct Entry<T> where T : struct, IEventMessage
    {
      public int Id;
      public int Priority;
      public EventCallback<T> Callback;
    }

    private sealed class Channel<T> : Channel where T : struct, IEventMessage
    {
      private Entry<T>[] _entries = Array.Empty<Entry<T>>();

      public override int Count => _entries.Length;

      public int Add(int id, EventCallback<T> callback, int priority)
      {
        Entry<T>[] next = new Entry<T>[_entries.Length + 1];
        int insert = 0;
        while (insert < _entries.Length && _entries[insert].Priority <= priority)
          insert++;
        Array.Copy(_entries, 0, next, 0, insert);
        next[insert] = new Entry<T> { Id = id, Priority = priority, Callback = callback };
        Array.Copy(_entries, insert, next, insert + 1, _entries.Length - insert);
        _entries = next;
        return _entries.Length;
      }

      public override bool Remove(int id)
      {
        int index = -1;
        for (int cursor = 0; cursor < _entries.Length; cursor++)
        {
          if (_entries[cursor].Id == id)
          {
            index = cursor;
            break;
          }
        }
        if (index < 0)
          return false;
        Entry<T>[] next = new Entry<T>[_entries.Length - 1];
        Array.Copy(_entries, 0, next, 0, index);
        Array.Copy(_entries, index + 1, next, index, _entries.Length - index - 1);
        _entries = next;
        return true;
      }

      public override void Clear() => _entries = Array.Empty<Entry<T>>();

      public override void Dispatch(object message, EventBus bus)
      {
        if (message is not T typed)
          return;
        // 快照: 允许处理中订阅/退订; 当前这一波仍按旧快照走, 下一波才生效
        Entry<T>[] entries = _entries;
        for (int index = 0; index < entries.Length; index++)
        {
          try
          {
            entries[index].Callback(in typed);
          }
          catch (Exception exception)
          {
            bus.ReportHandlerError(typeof(T), exception);
          }
        }
      }
    }

    private sealed class Subscription : IDisposable
    {
      private EventBus _bus;
      private readonly Type _eventType;
      private readonly int _id;

      public Subscription(EventBus bus, Type eventType, int id)
      {
        _bus = bus;
        _eventType = eventType;
        _id = id;
      }

      public void Dispose()
      {
        if (_bus is null)
          return;
        _bus.Unsubscribe(_eventType, _id);
        _bus = null;
      }
    }

    private readonly Dictionary<Type, Channel> _channels = new Dictionary<Type, Channel>();
    private readonly ConcurrentQueue<object> _pending = new ConcurrentQueue<object>();
    private readonly EventTraceEntry[] _trace = new EventTraceEntry[TraceCapacity];
    private readonly object _traceLock = new object();
    private readonly int _ownerThreadId;
    private int _nextId;
    private bool _cleared;
    private bool _flushing;
    private int _dispatchDepth;
    private long _published;
    private long _dispatched;
    private long _handlerErrors;
    private long _sequence;
    private int _traceCount;
    private int _traceNext;
    private string _dispatchError;

    public EventBus()
    {
      _ownerThreadId = Environment.CurrentManagedThreadId;
    }

    /// <summary>打开后记录最近 64 次派发; 事件监视器用, 正常游玩保持关闭.</summary>
    public bool TraceEnabled { get; set; }

    public int PendingCount => _pending.Count;

    public EventBusStats Stats
      => new EventBusStats(Interlocked.Read(ref _published), _dispatched, _handlerErrors, _pending.Count, _channels.Count);

    public IDisposable Subscribe<T>(EventCallback<T> callback, int priority = 0)
      where T : struct, IEventMessage
    {
      if (callback is null)
        throw new ArgumentNullException(nameof(callback));
      if (_cleared)
        throw new ObjectDisposedException(nameof(EventBus), "场景已释放, 不能再订阅事件");
      if (_channels.TryGetValue(typeof(T), out Channel channel) is false)
      {
        channel = new Channel<T>();
        _channels.Add(typeof(T), channel);
      }
      int id = ++_nextId;
      ((Channel<T>)channel).Add(id, callback, priority);
      return new Subscription(this, typeof(T), id);
    }

    /// <summary>入队, 在 Scene.Update 末尾统一派发. 可从后台线程调用.</summary>
    public void Publish<T>(in T message) where T : struct, IEventMessage
    {
      if (_cleared)
        return;
      _pending.Enqueue(message);
      Interlocked.Increment(ref _published);
    }

    /// <summary>
    /// 立即派发, 只用于必须同帧的"前置/门控"事件.
    /// <br>如果在派发过程中调用, 会排入下一波, 避免递归; 主线程之外调用会退化为入队.</br>
    /// </summary>
    public void PublishNow<T>(in T message) where T : struct, IEventMessage
    {
      if (_cleared)
        return;
      if (Environment.CurrentManagedThreadId != _ownerThreadId || _flushing || _dispatchDepth > 0)
      {
        _pending.Enqueue(message);
        Interlocked.Increment(ref _published);
        return;
      }
      Interlocked.Increment(ref _published);
      DispatchBoxed(message);
    }

    /// <summary>派发当前队列; 由 Scene.Update 在模块更新之后调用.</summary>
    public void Flush()
    {
      if (_cleared || _flushing || _dispatchDepth > 0)
        return;
      if (Environment.CurrentManagedThreadId != _ownerThreadId)
        return;
      _flushing = true;
      try
      {
        int processed = 0;
        while (processed < MaxEventsPerFlush && _pending.TryDequeue(out object message))
        {
          DispatchBoxed(message);
          processed++;
        }
      }
      finally
      {
        _flushing = false;
      }
    }

    /// <summary>场景销毁时兜底清理; 之后订阅会抛异常.</summary>
    public void Clear()
    {
      _cleared = true;
      _pending.Clear();
      _channels.Clear();
      lock (_traceLock)
      {
        _traceCount = 0;
        _traceNext = 0;
      }
    }

    public void ClearTrace()
    {
      lock (_traceLock)
      {
        _traceCount = 0;
        _traceNext = 0;
      }
    }

    /// <summary>把最近的追踪记录按时间顺序拷进 <paramref name="output"/>.</summary>
    public void GetTrace(List<EventTraceEntry> output)
    {
      if (output is null)
        return;
      output.Clear();
      lock (_traceLock)
      {
        int start = _traceCount < TraceCapacity ? 0 : _traceNext;
        for (int index = 0; index < _traceCount; index++)
          output.Add(_trace[(start + index) % TraceCapacity]);
      }
    }

    internal void ReportHandlerError(Type eventType, Exception exception)
    {
      _handlerErrors++;
      _dispatchError = string.Concat(eventType?.Name ?? "?", ": ", exception.Message);
      Console.Log(ConsoleTextType.Error, "EventBus",
        string.Concat("事件 ", eventType?.Name ?? "?", " 的订阅者抛出异常: ", exception));
    }

    private void DispatchBoxed(object message)
    {
      if (message is null)
        return;
      Type type = message.GetType();
      _channels.TryGetValue(type, out Channel channel);
      int subscribers = channel?.Count ?? 0;
      long start = TraceEnabled ? Stopwatch.GetTimestamp() : 0L;
      string error = null;
      _dispatchError = null;
      _dispatchDepth++;
      try
      {
        channel?.Dispatch(message, this);
        error = _dispatchError;
      }
      catch (Exception exception)
      {
        error = exception.Message;
        ReportHandlerError(type, exception);
      }
      finally
      {
        _dispatchDepth--;
        _dispatched++;
      }
      if (TraceEnabled)
      {
        double elapsed = (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
        AddTrace(new EventTraceEntry(++_sequence, type, subscribers, elapsed, error));
      }
    }

    private void AddTrace(EventTraceEntry entry)
    {
      lock (_traceLock)
      {
        _trace[_traceNext] = entry;
        _traceNext = (_traceNext + 1) % TraceCapacity;
        if (_traceCount < TraceCapacity)
          _traceCount++;
      }
    }

    private void Unsubscribe(Type eventType, int id)
    {
      if (_channels.TryGetValue(eventType, out Channel channel) is false)
        return;
      channel.Remove(id);
    }
  }
}
