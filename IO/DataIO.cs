using Colin.Core.Modulars;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.IO
{
  /// <summary>
  /// I/O.
  /// <br>用以提供多种不同场景下的 I/O 执行方案.</br>
  /// </summary>
  public class DataIO
  {
    public static void DoSave(string filePath, IOStep step, bool async = false)
    {
      //先把数据整成键值组再落盘: 存的人只管往里塞键值对, 落盘格式统一走 TagIO.
      TagCompound root = new TagCompound();
      step.SaveStep(root);
      if (async)
      {
        using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        TagIO.Write(fs, root);
      }
      else
      {
        TagIO.Write(filePath, root);
      }
    }
    public static void DoLoad(string filePath, IOStep step, bool async = false)
    {
      if (async)
      {
        using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true);
        step.LoadStep(TagIO.Read(fs));
      }
      else
      {
        step.LoadStep(TagIO.Read(filePath));
      }
    }
  }
}