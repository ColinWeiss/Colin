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
    private static Dictionary<string, ConsoleColor> LineDisplay = new Dictionary<string, ConsoleColor>();
    static Console()
    {
      LineDisplay.Add("Normal", ConsoleColor.DarkGray);
      LineDisplay.Add("Remind", ConsoleColor.Cyan);
      LineDisplay.Add("Game", ConsoleColor.White);
      LineDisplay.Add("Warning", ConsoleColor.Yellow);
      LineDisplay.Add("Error", ConsoleColor.Red);
    }

    /// <summary>  
    /// 向控制台输出信息.
    /// </summary>  
    /// <param name="infoType">信息类型.</param>  
    /// <param name="output">输出内容.</param>  
    public static void WriteLine(string infoType, object output)
    {
      WriteLine(infoType, output.ToString());
    }

    /// <summary>  
    /// 向控制台输出信息.
    /// </summary>  
    /// <param name="infoType">信息类型.</param>  
    /// <param name="output">输出内容.</param>  
    public static void WriteLine(string infoType, string output)
    {
      System.Console.ForegroundColor = LineDisplay[infoType];
      string outPutText = string.Concat("[", CoreInfo.EngineName, "] ", output);
      System.Console.WriteLine(outPutText);
      System.Console.ResetColor();
    }

    /// <summary>  
    /// 向控制台输出信息.
    /// </summary>  
    public static void WriteLine(string output)
    {
      WriteLine("Normal", output);
    }

    /// <summary>  
    /// 向控制台输出信息.
    /// </summary>  
    public static void WriteLine(object output)
    {
      WriteLine("Normal", output);
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

    /// <summary>
    /// 执行CMD.
    /// </summary>
    /// <param name="cmdLine">命令行</param>
    /// <returns>执行结果</returns>
    public static void Execute(List<string> cmdLine)
    {
      string msg = "";
      for (int count = 0; count < cmdLine.Count; count++)
      {
        msg = CmdExecute(cmdLine[count]);
        if(msg != "")
          WriteLine("Error" , msg);
        msg = "";
      }
    }
    public static string CmdExecute(string command)
    {
      bool flag = true;
      string result;
      Process p = new Process();
      p.StartInfo.FileName = "cmd.exe";
      p.StartInfo.UseShellExecute = false;
      p.StartInfo.RedirectStandardInput = true;
      p.StartInfo.RedirectStandardOutput = true;
      p.StartInfo.RedirectStandardError = true;
      p.StartInfo.CreateNoWindow = true;
      p.Start();
      p.StandardInput.AutoFlush = true;
      p.StandardInput.WriteLine(command);
      WriteLine("Remind", "执行 " + command);
      p.StandardInput.WriteLine("exit");
      p.StandardInput.Close();
      result = p.StandardError.ReadToEnd();
      p.WaitForExit();
      p.Close();
      return result;
    }
  }
}