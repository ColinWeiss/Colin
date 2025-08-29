using System.Diagnostics;

namespace Colin.Core.Common.Debugs
{
  /// <summary>
  /// 层级式分析各个模块的运行时长占用
  /// </summary>
  public class DebugProfiler
  {
    private static DebugProfiler _instance = null;
    private static DebugProfiler Instance => _instance ??= new();
    private DebugProfiler() { }

    private readonly List<StopwatchHierarchy> stopwatchStack = new([new StopwatchHierarchy("_root")]);

    internal static StopwatchHierarchy RootNode => Instance.stopwatchStack[0];

    /// <summary>
    /// 获取最近一轮的统计结果
    /// </summary>
    public static List<ReportLine> RecentReport => Instance.recentReport;

    /// <summary>
    /// 获取最近一次重置时的的统计结果
    /// </summary>
    public static List<ReportLine> RecentResetReport => Instance.recentResetReport;

    private List<ReportLine> recentReport = new();
    private List<ReportLine> recentResetReport = new();
    private bool resetFlag = false;

    public static void NextTick()
    {
      RootNode.report.totalTime = 0;
      foreach (StopwatchHierarchy child in RootNode.hierarchyChildren)
        RootNode.report.totalTime += child.report.totalTime;
      RootNode.report.count += 1;
      Instance.recentReport = GenerateReportLines();
      if (Instance.resetFlag)
      {
        Instance.recentResetReport = GenerateReportLines();
        Instance.resetFlag = false;
        ResetImmediate();
      }
    }

    /// <summary>
    /// 重置时长统计结果（如每帧/每若干秒重置）
    /// </summary>
    public static void Reset()
    {
      Instance.resetFlag = true;
    }

    private static void ResetImmediate()
    {
      RootNode.report.count = 0;
      RootNode.report.totalTime = 0;
      RootNode.hierarchyChildren.Clear();
    }

    /// <summary>
    /// 进行运行时长分析，使用方式如下：
    /// <code>
    /// using (DebugProfiler.Tag("tagname"))
    /// {
    ///   ... // your code
    /// }
    /// </code>
    /// </summary>
    /// * 请确保只在主线程使用
    public static DebugProfilerContext Tag(string name)
    {
      StopwatchHierarchy stopwatch = Instance.stopwatchStack[^1].ExtendChildren(name);
      stopwatch.stopwatch.Restart();
      stopwatch.hierarchyParent = Instance.stopwatchStack[^1];
      Instance.stopwatchStack.Add(stopwatch);
      return new DebugProfilerContext(stopwatch);
    }

    private static List<ReportLine> GenerateReportLines()
    {
      List<ReportLine> reportLines = new();
      TraverseHierarchy(Instance.stopwatchStack[0], reportLines);
      return reportLines;
    }

    private static void TraverseHierarchy(StopwatchHierarchy stopwatch, List<ReportLine> reportLines)
    {
      reportLines.Add(stopwatch.report.Clone());
      foreach (StopwatchHierarchy child in stopwatch.hierarchyChildren)
      {
        TraverseHierarchy(child, reportLines);
      }
    }

    internal static void PopContext(StopwatchHierarchy stopwatch)
    {
      if (!stopwatch.Equals(Instance.stopwatchStack[^1]))
      {
        throw new InvalidOperationException("性能分析栈异常，请确保性能分析只在主线程上运行");
      }
      stopwatch.stopwatch.Stop();
      stopwatch.report.count += 1;
      stopwatch.report.totalTime += stopwatch.stopwatch.ElapsedMilliseconds;
      stopwatch.stopwatch.Restart();
      Instance.stopwatchStack.RemoveAt(Instance.stopwatchStack.Count - 1);
    }

    public class ReportLine(string description, float totalTime, int count, string hierarchyPath = "")
    {
      public string description = description;
      public string hierarchyPath = hierarchyPath;
      public float totalTime = totalTime;
      public int count = count;

      public ReportLine Clone() => new ReportLine(description, totalTime, count, hierarchyPath);
    }

    public class StopwatchHierarchy(string name)
    {
      public Stopwatch stopwatch = new();
      public string name = name;

      public ReportLine report = new("", 0, 0);
      public StopwatchHierarchy hierarchyParent = null;
      public readonly List<StopwatchHierarchy> hierarchyChildren = new();

      internal StopwatchHierarchy ExtendChildren(string name)
      {
        for (int i = 0; i < hierarchyChildren.Count; i++)
          if (hierarchyChildren[i].name == name)
            return hierarchyChildren[i];
        hierarchyChildren.Add(new StopwatchHierarchy(name));
        hierarchyChildren[^1].report.description = new string(' ', (Instance.stopwatchStack.Count - 1) * 3) + name;
        hierarchyChildren[^1].report.hierarchyPath = report.hierarchyPath + "." + name;
        return hierarchyChildren[^1];
      }
    }

    public class DebugProfilerContext(StopwatchHierarchy stopwatch) : IDisposable
    {
      public StopwatchHierarchy stopwatch = stopwatch;

      public void Dispose() => PopContext(stopwatch);
    }
  }
}