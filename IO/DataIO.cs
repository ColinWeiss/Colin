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
      if (async)
      {
        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
        using (BinaryWriter writer = new BinaryWriter(fs))
          step.SaveStep(writer);
      }
      else
      {
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
          step.SaveStep(writer);
      }
    }
    public static void DoLoad(string filePath, IOStep step, bool async = false)
    {
      if (async)
      {
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true))
        using (BinaryReader reader = new BinaryReader(fs))
          step.LoadStep(reader);
      }
      else
      {
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        using (BinaryReader reader = new BinaryReader(fs))
          step.LoadStep(reader);
      }
    }
  }
}