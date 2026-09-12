using System.Collections.Concurrent;
using System.Diagnostics;

namespace Colin.Core.Modulars
{
  /// <summary>
  /// 业务线的抽象, 目的在于以时序区分帧流.
  /// </summary>
  public abstract class BusinessLine
  {
    public Scene Scene { get; set; }

    public Business Business { get; set; }

    /// <summary>
    /// 每帧命令消费的时间预算, 单位毫秒.
    /// <br>超预算的命令留在队列里下一帧接着执行, 避免成批的区块命令把一帧拖爆.</br>
    /// </summary>
    public static double CommandBudgetMs = 2.0;

    private ConcurrentQueue<IBusinessCase> _cache = new ConcurrentQueue<IBusinessCase>();
    private ConcurrentQueue<IBusinessCase> _current = new ConcurrentQueue<IBusinessCase>();
    public ConcurrentQueue<IBusinessCase> Cases => _current;

    /// <summary>
    /// 标记工作项至缓存队列.
    /// </summary>
    /// <param name="business"></param>
    public void Mark(IBusinessCase business)
    {
      _cache.Enqueue(business);
    }

    /// <summary>
    /// 执行准备阶段.
    /// </summary>
    public void DoPrepare()
    {
      var temp = _current;
      _current = _cache;
      _cache = temp;
      OnPrepare();
    }
    /// <summary>
    /// 在准备阶段执行.
    /// <br>用以完成数据准备工作, 不建议于此处进行除确认队列内容之外的实际操作.</br>
    /// </summary>
    protected abstract void OnPrepare();

    public void DoUpdate()
    {
      // 按时间预算消费命令, 预算用完就停, 剩下的留在队列里下一帧接着执行
      long start = Stopwatch.GetTimestamp();
      while (_current.TryDequeue(out var business))
      {
        business.Execute();
        if ((Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency >= CommandBudgetMs)
          break;
      }
    }
  }
}