using Colin.Core.Resources;
using DeltaMachine.Core.Common.Tiles.Scripts;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Option = Colin.Core.Modulars.Tiles.TileOption;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 挂载在区块上的数据模块
  /// 以SOA方式为每个物块分配空间
  /// </summary>
  public interface ITileChunkModule
  {
    public static abstract int ModuleIndex { get; set; }

    public static abstract ITileChunkDataframe CreateDataframe();
  }

  public interface ITileChunkDataframe
  {
    public void Allocate(int size);

    public void Dispose();
  }
}