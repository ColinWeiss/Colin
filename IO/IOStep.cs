using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.IO
{
  /// <summary>
  /// 指示一个可通过键值组参与存档的对象.
  /// <br>存档就是往 <see cref="TagCompound"/> 里塞键值对, 读档就是按名字取值;
  /// 键不在就保持默认, 键多出来就没人读, 天然版本兼容.</br>
  /// <br>注意: 逐格的海量数据请摊成数组塞进一个键, 别一格开一组键值对, 物块那边是性能红线.</br>
  /// </summary>
  public interface IOStep
  {
    void SaveStep(TagCompound data);
    void LoadStep(TagCompound data);
  }
}
