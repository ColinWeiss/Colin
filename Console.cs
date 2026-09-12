using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Colin.Core
{
  /// <summary>
  /// 控制台输出信息类型.
  /// <br>Debug 级别的日志受 <see cref="CoreInfo.Debug"/> 门控, Release 构建里整个调用会被编译器消除.</br>
  /// </summary>
  public enum ConsoleTextType : int
  {
    /// <summary>
    /// 调试信息, 只在 Debug 模式输出.
    /// </summary>
    Debug,
    /// <summary>
    /// 普通信息.
    /// </summary>
    Normal,
    /// <summary>
    /// 游戏信息.
    /// </summary>
    Game,
    /// <summary>
    /// 警告信息.
    /// </summary>
    Warning,
    /// <summary>
    /// 提示信息.
    /// </summary>
    Remind,
    /// <summary>
    /// 错误信息.
    /// </summary>
    Error
  }

  /// <summary>
  /// 统一日志入口.
  /// <br>日志一律走 <see cref="Log"/>, 输出格式为 [级别][模块][时间戳]: 正文, 同时写控制台和 game_log.txt.</br>
  /// <br>别再散着用 System.Console 或 Debug.WriteLine 了, 都从这里走.</br>
  /// </summary>
  public class Console
  {
    private static Dictionary<ConsoleTextType, ConsoleColor> LineDisplay = new Dictionary<ConsoleTextType, ConsoleColor>();

    // 落盘走独立线程, 日志写盘不能卡主线程, 这个教训是帧尖峰分析换来的
    private static readonly BlockingCollection<string> _writeQueue = new BlockingCollection<string>(256);
    private static readonly string _logPath = Path.Combine(Colin.Core.IO.BasicsDirectory.LogDir, "game_log.txt");

    static Console()
    {
      LineDisplay.Add(ConsoleTextType.Debug, ConsoleColor.DarkGreen);
      LineDisplay.Add(ConsoleTextType.Normal, ConsoleColor.DarkGray);
      LineDisplay.Add(ConsoleTextType.Remind, ConsoleColor.Cyan);
      LineDisplay.Add(ConsoleTextType.Game, ConsoleColor.White);
      LineDisplay.Add(ConsoleTextType.Warning, ConsoleColor.Yellow);
      LineDisplay.Add(ConsoleTextType.Error, ConsoleColor.Red);
      Thread writer = new Thread(WriteLoop)
      {
        IsBackground = true,
        Name = "LogWriter",
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
      foreach (string line in _writeQueue.GetConsumingEnumerable())
      {
        try
        {
          File.AppendAllText(_logPath, line + Environment.NewLine);
        }
        catch
        {
          // 日志写不进去也不能影响游戏本体, 静默吞掉
        }
      }
    }

    /// <summary>
    /// 统一日志入口, 输出格式 [级别][模块][时间戳]: 正文.
    /// </summary>
    /// <param name="level">日志级别, Debug 级别只在 Debug 模式输出.</param>
    /// <param name="module">功能模块名, 用当前系统或类名, 别空着.</param>
    /// <param name="message">日志正文.</param>
    public static void Log(ConsoleTextType level, string module, string message)
    {
      if (level == ConsoleTextType.Debug && CoreInfo.Debug is false)
        return;
      string line = string.Format("[{0}][{1}][{2:yyyy-MM-dd HH:mm:ss.fff}]: {3}",
        level, module, DateTime.Now, message ?? string.Empty);
      try
      {
        System.Console.ForegroundColor = LineDisplay[level];
        System.Console.WriteLine(line);
        System.Console.ResetColor();
      }
      catch
      {
        // 没有控制台宿主的时候彩色输出会炸, 吞掉接着写文件
      }
      _writeQueue.TryAdd(line);
    }

    /// <summary>
    /// Debug 级别日志的快捷入口, Release 构建里整个调用会被编译器消除.
    /// </summary>
    public static void Debug(string module, string message)
      => Log(ConsoleTextType.Debug, module, message);

    /// <summary>
    /// 把旧式的字符串级别名转成枚举, 兼容历史调用.
    /// </summary>
    private static ConsoleTextType ParseLevel(string infoType)
    {
      switch (infoType)
      {
        case "Error": return ConsoleTextType.Error;
        case "Warning": return ConsoleTextType.Warning;
        case "Remind": return ConsoleTextType.Remind;
        case "Game": return ConsoleTextType.Game;
        default: return ConsoleTextType.Normal;
      }
    }

    /// <summary>
    /// 向控制台输出信息.
    /// </summary>
    /// <param name="infoType">信息类型.</param>
    /// <param name="output">输出内容.</param>
    public static void WriteLine(string infoType, object output)
    {
      Log(ParseLevel(infoType), "通用", output?.ToString() ?? string.Empty);
    }

    /// <summary>
    /// 向控制台输出信息.
    /// </summary>
    public static void WriteLine(string infoType, string output)
    {
      Log(ParseLevel(infoType), "通用", output);
    }

    /// <summary>
    /// 向控制台输出信息.
    /// </summary>
    public static void WriteLine(string output)
    {
      Log(ConsoleTextType.Normal, "通用", output);
    }

    /// <summary>
    /// 向控制台输出信息.
    /// </summary>
    public static void WriteLine(object output)
    {
      Log(ConsoleTextType.Normal, "通用", output?.ToString() ?? string.Empty);
    }

    /// <summary>
    /// 执行CMD命令.
    /// </summary>
    /// <param name="cmdLine">命令行</param>
    /// <returns>执行结果</returns>
    public static string Execute(string cmdLine)
    {
      using (var process = new Process())
      {
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        process.StandardInput.AutoFlush = true;
        process.StandardInput.WriteLine(cmdLine + "&exit");
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        process.Close();
        return output;
      }
    }

    /// <summary>
    /// 执行CMD.
    /// </summary>
    /// <param name="cmdLine">命令行</param>
    /// <returns>执行结果</returns>
    public static void Execute(string[] cmdLine)
    {
      Process process = new Process();
      try
      {
        for (int count = 0; count < cmdLine.Length; count++)
        {
          process.StartInfo.FileName = "cmd.exe";
          process.StartInfo.UseShellExecute = false;
          process.StartInfo.RedirectStandardInput = true;
          process.StartInfo.RedirectStandardOutput = true;
          process.StartInfo.RedirectStandardError = true;
          process.StartInfo.CreateNoWindow = false;
          process.Start();
          process.StandardInput.AutoFlush = true;
          process.StandardInput.WriteLine(cmdLine[count]);
          WriteLine("Execute: " + cmdLine[count]);
          process.StandardInput.WriteLine("exit");
          process.WaitForExit();
          process.Close();
        }
      }
      catch
      {
        WriteLine(process.StandardError.ReadToEnd());
      }
    }
  }
}