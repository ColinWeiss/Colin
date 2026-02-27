using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.IO
{
  /// <summary>
  /// 指示一个可通过二进制流参与 IO 的对象.
  /// <br>适用于性能要求高的场景.</br>
  /// </summary>
  public interface IOStep
  {
    void SaveStep(BinaryWriter writer);
    void LoadStep(BinaryReader reader);
  }
}