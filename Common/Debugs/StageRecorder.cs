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
      _times.Clear();
      _counts.Clear();
      _stack.Clear();
    }

    internal static void AppendReport(StringBuilder builder, int top)
    {
      foreach (var pair in _times.OrderByDescending(p => p.Value).Take(top))
        builder.AppendLine(string.Format("  {0,9:F3} ms x{1,-3} {2}", pair.Value, _counts.GetValueOrDefault(pair.Key), pair.Key));
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