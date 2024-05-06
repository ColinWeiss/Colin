using System.Diagnostics;

namespace Colin.Core
{
  /// <summary>
  /// 控制台输出信息类型.
  /// </summary>
  public enum ConsoleTextType : int
  {
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
  /// 你看它名字你就知道这是什么了.
  /// </summary>
  public class Console
  {
    /// <summary>  
    /// 向控制台输出信息.
    /// </summary>  
    /// <param name="informationType">信息类型.</param>  
    /// <param name="output">输出内容.</param>  
    public static void WriteLine(ConsoleTextType informationType, object output)
    {
      WriteLine(informationType, output.ToString());
    }

    /// <summary>  
    /// 向控制台输出信息.
    /// </summary>  
    /// <param name="informationType">信息类型.</param>  
    /// <param name="output">输出内容.</param>  
    public static void WriteLine(ConsoleTextType informationType, string output)
    {
      System.Console.ForegroundColor = GetConsoleColor(informationType);
      string NowTime = string.Concat("[", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "]");
      string outPutText = string.Concat("=>", "[", CoreInfo.EngineName, "] ", output);
      System.Console.WriteLine(outPutText);
    }

    /// <summary>  
    /// 向控制台输出信息.
    /// </summary>  
    public static void WriteLine(string output)
    {
      WriteLine(ConsoleTextType.Remind, output);
    }

    /// <summary>  
    /// 向控制台输出信息.
    /// </summary>  
    public static void WriteLine(object output)
    {
      WriteLine(ConsoleTextType.Remind, output);
    }

    /// <summary>  
    /// 根据输出文本选择控制台文字颜色.
    /// </summary>  
    /// <param name="informationType">信息类型.</param>  
    /// <returns></returns>  
    private static ConsoleColor GetConsoleColor(ConsoleTextType informationType)
    {
      switch (informationType)
      {
        case ConsoleTextType.Normal:
          return ConsoleColor.DarkGray;
        case ConsoleTextType.Remind:
          return ConsoleColor.Yellow;
        case ConsoleTextType.Game:
          return ConsoleColor.White;
        case ConsoleTextType.Warning:
          return ConsoleColor.Yellow;
        case ConsoleTextType.Error:
          return ConsoleColor.Red;
      }
      return ConsoleColor.White;
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
      using (var process = new Process())
      {
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = false;
        process.Start();
        process.StandardInput.AutoFlush = true;
        for (int count = 0; count < cmdLine.Length; count++)
        {
          process.StandardInput.WriteLine(cmdLine[count] );
        }
        process.StandardInput.WriteLine("exit");
        process.WaitForExit();
        process.Close();
      }
    }

    /// <summary>
    /// 执行CMD.
    /// </summary>
    /// <param name="cmdLine">命令行</param>
    /// <returns>执行结果</returns>
    public static void Execute(List<string> cmdLine)
    {
      using (var process = new Process())
      {
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = false;
        process.Start();
        process.StandardInput.AutoFlush = true;
        for (int count = 0; count < cmdLine.Count; count++)
        {
          process.StandardInput.WriteLine(cmdLine[count]);
        }
        process.StandardInput.WriteLine("exit");
        process.WaitForExit();
        process.Close();
      }
    }
  }
}