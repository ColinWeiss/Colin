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

    /// <summary>
    /// 波次结清: 自带首次批次结转, 于本帧内反复「结转 - 处理」直至排空,
    /// 用以允许处理中派生的标记于同帧继续消费(同帧级联).
    /// <br>波次上限为熔断: 耗尽后仍未处理的项自动滚入下一批, 不丢失、不阻塞, 防止效果互标导致死循环.</br>
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="maxWaves">波次上限, 用以熔断无界级联.</param>
    public void Settle(Action<T> handler, int maxWaves = 16)
    {
      for (int wave = 0; wave < maxWaves; wave++)
      {
        Flip();
        Drain(handler);
        if (Marks.IsEmpty)
          break;
      }
    }
  }
}