using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Businesses
{
  /// <summary>
  /// 双缓冲命令队列. 
  /// </summary>
  public sealed class DeferralSe<T>
  {
    private ConcurrentQueue<T> Marks = new();
    private ConcurrentQueue<T> Current = new();

    /// <summary>
    /// 标记工作项.
    /// </summary>
    public void Mark(T item) => Marks.Enqueue(item);

    /// <summary>
    /// 指定结算前标记的内容算本批次.
    /// </summary>
    public void Flip() => (Current, Marks) = (Marks, Current);

    /// <summary>
    /// 处理当前批次工作项.
    /// </summary>
    public void Drain(Action<T> handler)
    {
      while (Current.TryDequeue(out var item))
        handler(item);
    }

    /// <summary>
    /// 立即处理当前批次工作项.
    /// </summary>
    public void DrainNow(Action<T> handler)
    {
      Flip();
      Drain(handler);
    }
  }
}