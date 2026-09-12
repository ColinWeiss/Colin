using System.Diagnostics;

namespace Colin.Core.Common.Debugs
{
  /// <summary>
  /// 逐帧的阶段计时器, 和按秒平均的 DebugProfiler 互补.
  /// <br>按秒平均会把尖峰摊薄到看不见, 这里按帧记录, 交给 <see cref="HitchRecorder"/> 在尖峰帧抓拍.</br>
  /// <br>阶段之间存在嵌套, 同一段时间会被上层阶段重复计入, 分析时看相对占比即可.</br>
  /// <br>只在主线程使用.</br>
  /// </summary>
  public static class StageRecorder
  {
    private static readonly Dictionary<string, double> _times = new Dictionary<string, double>();
    private static readonly Dictionary<string, int> _counts = new Dictionary<string, int>();
    // 窗口累计, 用来算稳态平均帧成本, 每帧数据先进这里, 摘要输出后统一清
    private static readonly Dictionary<string, double> _windowTimes = new Dictionary<string, double>();
    private static readonly Dictionary<string, int> _windowCounts = new Dictionary<string, int>();
    private static readonly Stack<(string name, long start)> _stack = new Stack<(string name, long start)>();

    /// <summary>
    /// 开始一个阶段计时, 配合 using 使用, 离开作用域自动累计.
    /// </summary>
    public static StageToken Tag(string name)
    {
      _stack.Push((name, Stopwatch.GetTimestamp()));
      return default;
    }

    public static double GetTime(string name) => _times.GetValueOrDefault(name);

    public static int GetCount(string name) => _counts.GetValueOrDefault(name);

    internal static void ResetFrame()
    {
      foreach (var pair in _times)
      {
        _windowTimes[pair.Key] = _windowTimes.GetValueOrDefault(pair.Key) + pair.Value;
      }
      foreach (var pair in _counts)
      {
        _windowCounts[pair.Key] = _windowCounts.GetValueOrDefault(pair.Key) + pair.Value;
      }
      _times.Clear();
      _counts.Clear();
      _stack.Clear();
    }

    internal static void AppendReport(StringBuilder builder, int top)
    {
      foreach (var pair in _times.OrderByDescending(p => p.Value).Take(top))
        builder.AppendLine(string.Format("  {0,9:F3} ms x{1,-3} {2}", pair.Value, _counts.GetValueOrDefault(pair.Key), pair.Key));
    }

    /// <summary>
    /// 输出窗口内每阶段的每帧平均值, 用来看稳态下 7ms 都花在哪, 输出后清空窗口.
    /// <br>注意阶段有嵌套, 同一段时间会被上层阶段重复计入, 看相对占比即可.</br>
    /// </summary>
    internal static void AppendWindowReport(StringBuilder builder, int top, double frames)
    {
      if (frames <= 0)
      {
        _windowTimes.Clear();
        _windowCounts.Clear();
        return;
      }
      foreach (var pair in _windowTimes.OrderByDescending(p => p.Value).Take(top))
        builder.AppendLine(string.Format("  {0,7:F3} ms/帧 ({1,8:F0} ms合计 x{2,-5}) {3}",
          pair.Value / frames, pair.Value, _windowCounts.GetValueOrDefault(pair.Key), pair.Key));
      _windowTimes.Clear();
      _windowCounts.Clear();
    }

    public readonly struct StageToken : IDisposable
    {
      public void Dispose()
      {
        if (_stack.Count == 0)
          return;
        (string name, long start) = _stack.Pop();
        double ms = (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
        _times[name] = _times.GetValueOrDefault(name) + ms;
        _counts[name] = _counts.GetValueOrDefault(name) + 1;
      }
    }
  }
}