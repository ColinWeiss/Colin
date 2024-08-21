using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.IO
{
  /// <summary>
  /// 指示一个可进行文件保存/读取的对象.
  /// </summary>
  interface IOStep
  {
    void LoadStep(BinaryReader reader);
    void SaveStep(BinaryWriter writer);
  }
}