using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Colin.Core.Common.Debugs
{
  /// <summary>
  /// 帧尖峰黑匣子.
  /// <br>用真实墙钟逐帧测耗时, 不信 GameTime, 因为固定步长下它永远是个常数.</br>
  /// <br>抓拍的尖峰帧会带上当帧阶段耗时和 GC 停顿数据, 追加到 hitch_report.txt, 用来回答卡帧到底卡在哪.</br>
  /// </summary>
  public static class HitchRecorder
  {
    /// <summary>
    /// 尖峰判定的绝对下限毫秒数, 再怎么流畅的机器, 超过它的帧才算尖峰候选.
    /// </summary>
    public static double AbsoluteFloorMs = 24.0;

    /// <summary>
    /// 超过基线多少毫秒算尖峰, 基线是近期最好帧耗时, 也就是这台机器的流畅水平.
    /// </summary>
    public static double SpikeMarginMs = 12.0;

    /// <summary>
    /// 两次抓拍之间的最小间隔毫秒数, 持续卡顿的时候防止日志刷爆.
    /// </summary>
    public static double MinDumpIntervalMs = 250.0;

    public static bool Enable = true;

    /// <summary>
    /// 最近一次抓拍的内容, 方便外部工具读取.
    /// </summary>
    public static string LastReport;

    /// <summary>
    /// 近5秒的平均真实帧耗时, 给调试面板读.
    /// </summary>
    public static double LastAvgFrameMs;

    /// <summary>
    /// 近5秒的最大真实帧耗时, 给调试面板读.
    /// </summary>
    public static double LastMaxFrameMs;

    /// <summary>
    /// 近5秒的 GC 停顿合计, 给调试面板读.
    /// </summary>
    public static double LastWindowPauseMs;

    /// <summary>
    /// 近5秒的单次最大 GC 停顿, 给调试面板读.
    /// </summary>
    public static double LastMaxPauseMs;

    private static long _lastTimestamp;
    private static double _baselineMs = double.MaxValue;
    private static int _gen0, _gen1, _gen2;
    private static TimeSpan _gcPauseTotal;
    private static double _lastDump;
    private static double _lastSummary;
    private static double _accumFrameMs;
    private static double _accumFrames;
    private static double _maxFrameMs;
    private static double _accumPauseMs;
    private static int _wGen0, _wGen1, _wGen2;
    private static double _wMaxPause;
    private static readonly StringBuilder _builder = new StringBuilder(8192);
    private static readonly string _logPath = Path.Combine(Colin.Core.IO.BasicsDirectory.LogDir, "hitch_report.txt");

    // 落盘走独立线程, 同步写盘曾经在主线程上顶到一两百毫秒, 还会和 5 秒的区块卸载节奏撞车污染测量
    private static readonly BlockingCollection<string> _writeQueue = new BlockingCollection<string>(64);

    static HitchRecorder()
    {
      Thread writer = new Thread(WriteLoop)
      {
        IsBackground = true,
        Name = "HitchRecorder",
        Priority = ThreadPriority.BelowNormal
      };
      writer.Start();
    }

    private static void WriteLoop()
    {
      try
      {
        Directory.CreateDirectory(Colin.Core.IO.BasicsDirectory.LogDir);
      }
      catch
      {
        // 目录建不出来就只走控制台, 别拦着游戏
      }
      foreach (string text in _writeQueue.GetConsumingEnumerable())
      {
        try
        {
          File.AppendAllText(_logPath, text);
        }
        catch
        {
          // 日志写不进去也不能影响游戏本体, 静默吞掉
        }
      }
    }

    /// <summary>
    /// 每帧开头调用, 自己用秒表测上一帧的真实总耗时.
    /// <br>阶段数据是上一帧攒的, 抓拍和清零都在这里做.</br>
    /// </summary>
    public static void OnFrame()
    {
      long now = Stopwatch.GetTimestamp();
      double frameMs = _lastTimestamp == 0 ? 0 : (now - _lastTimestamp) * 1000.0 / Stopwatch.Frequency;
      _lastTimestamp = now;

      int gen0 = GC.CollectionCount(0);
      int gen1 = GC.CollectionCount(1);
      int gen2 = GC.CollectionCount(2);
      TimeSpan pause = GC.GetTotalPauseDuration();
      int d0 = gen0 - _gen0;
      int d1 = gen1 - _gen1;
      int d2 = gen2 - _gen2;
      double pauseDelta = (pause - _gcPauseTotal).TotalMilliseconds;
      _gen0 = gen0;
      _gen1 = gen1;
      _gen2 = gen2;
      _gcPauseTotal = pause;

      // 基线取近期最好帧, 慢慢上浮以适应负载变化, 尖峰判定同时要求超过绝对下限和基线加余量
      double nowMs = now * 1000.0 / Stopwatch.Frequency;
      if (frameMs > 0)
      {
        _baselineMs = Math.Min(_baselineMs, frameMs) * 1.0005;
        _accumFrameMs += frameMs;
        _accumFrames += 1;
        _maxFrameMs = Math.Max(_maxFrameMs, frameMs);
        _accumPauseMs += pauseDelta;
        _wGen0 += d0;
        _wGen1 += d1;
        _wGen2 += d2;
        // 单帧GC停顿直接拿停顿增量当近似, 免得每帧去查堆快照, 那个可不便宜
        if (pauseDelta > _wMaxPause)
          _wMaxPause = pauseDelta;
      }

      if (Enable && frameMs >= AbsoluteFloorMs && frameMs >= _baselineMs + SpikeMarginMs)
      {
        if (nowMs - _lastDump >= MinDumpIntervalMs)
        {
          _lastDump = nowMs;
          Dump(frameMs, d0, d1, d2, pauseDelta);
        }
      }
      nowMs = now * 1000.0 / Stopwatch.Frequency;
      if (nowMs - _lastSummary >= 5000.0 && _accumFrames > 0)
      {
        _lastSummary = nowMs;
        DumpSummary();
      }
      StageRecorder.ResetFrame();
    }

    private static void Dump(double frameMs, int d0, int d1, int d2, double pauseDelta)
    {
      _builder.Clear();
      _builder.AppendLine("================ 帧尖峰 ================");
      _builder.AppendLine(string.Format("真实帧耗时 {0:F2} ms, 本帧GC停顿 {1:F2} ms, GC次数增量 0代={2} 1代={3} 2代={4}", frameMs, pauseDelta, d0, d1, d2));
      GCMemoryInfo info = GC.GetGCMemoryInfo();
      _builder.AppendLine(string.Format("托管堆 {0:F1} MB, 堆碎片 {1:F1} MB, 已提交 {2:F1} MB", info.HeapSizeBytes / 1048576.0, info.FragmentedBytes / 1048576.0, info.TotalCommittedBytes / 1048576.0));
      _builder.AppendLine(string.Format("总帧数 {0}", Time.FrameCount));
      _builder.AppendLine("当帧阶段耗时(按耗时排序, 阶段有嵌套所以会有重复计入, 看相对占比):");
      StageRecorder.AppendReport(_builder, 18);
      _builder.AppendLine();
      Flush(_builder.ToString());
    }

    private static void DumpSummary()
    {
      _builder.Clear();
      _builder.AppendLine(string.Format("------ 近5秒: 平均真实帧 {0:F2} ms, 最大真实帧 {1:F2} ms, GC停顿合计 {2:F2} ms, 单次最大停顿 {3:F2} ms, GC次数 0代={4} 1代={5} 2代={6} ------",
        _accumFrameMs / Math.Max(1, _accumFrames), _maxFrameMs, _accumPauseMs, _wMaxPause, _wGen0, _wGen1, _wGen2));
      LastAvgFrameMs = _accumFrameMs / Math.Max(1, _accumFrames);
      LastMaxFrameMs = _maxFrameMs;
      LastWindowPauseMs = _accumPauseMs;
      LastMaxPauseMs = _wMaxPause;
      _builder.AppendLine("近5秒稳态阶段成本(每帧均值, 降序, 阶段嵌套会重复计入, 看相对占比):");
      StageRecorder.AppendWindowReport(_builder, 14, _accumFrames);
      _accumFrameMs = 0;
      _accumFrames = 0;
      _maxFrameMs = 0;
      _accumPauseMs = 0;
      _wGen0 = 0;
      _wGen1 = 0;
      _wGen2 = 0;
      _wMaxPause = 0;
      Flush(_builder.ToString());
    }

    private static void Flush(string text)
    {
      LastReport = text;
      Trace.Write(text);
      _writeQueue.TryAdd(text);
    }
  }
}