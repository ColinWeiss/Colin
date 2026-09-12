using Colin.Core.Modulars.Tiles;
using System.Collections.Concurrent;

namespace Colin.Core.Modulars
{
  /// <summary>
  /// 业务线处理核心.
  /// </summary>
  public class Business : SceneModule
  {
    private Dictionary<Type, BusinessLine> _businesses = new Dictionary<Type, BusinessLine>();

    private ConcurrentQueue<Action> _mainThreadJobs = new ConcurrentQueue<Action>();

    /// <summary>
    /// 把一个收尾作业排到主线程执行.
    /// <br>后台线程做完重活后, 用它把状态复位和事件通知丢回主线程, 不要在后台直接碰游戏数据.</br>
    /// </summary>
    public void MarkMainThreadJob(Action job)
      => _mainThreadJobs.Enqueue(job);

    /// <summary>
    /// 为指定业务线添加工作项.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="business"></param>
    public void Mark<T>(IBusinessCase business) where T : BusinessLine
    {
      _businesses[typeof(T)].Mark(business);
    }

    /// <summary>
    /// 根据指定类型获取业务线.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public T Get<T>() where T : BusinessLine
    {
      return (T)_businesses[typeof(T)];
    }

    public void Register<T>() where T : BusinessLine, new()
    {
      T t = new T();
      t.Scene = Scene;
      _businesses[typeof(T)] = t;
    }

    public override void DoUpdate(GameTime time)
    {
      // 先清掉后台线程攒下来的收尾作业, 单个作业出错不能影响后面排队的人
      // 收尾作业也按时间预算消费, 成批区块同时完成时不会把一帧拖爆
      long start = System.Diagnostics.Stopwatch.GetTimestamp();
      while (_mainThreadJobs.TryDequeue(out var job))
      {
        try
        {
          job();
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error", string.Concat("主线程收尾作业执行异常: ", ex));
        }
        if ((System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency >= BusinessLine.CommandBudgetMs)
          break;
      }
      for (int index = 0; index < _businesses.Count; index++)
      {
        _businesses.ElementAt(index).Value.DoPrepare();
      }
      for (int index = 0; index < _businesses.Count; index++)
      {
        _businesses.ElementAt(index).Value.DoUpdate();
      }
      base.DoUpdate(time);
    }

    public override void Dispose()
    {
      base.Dispose();
    }
  }
}