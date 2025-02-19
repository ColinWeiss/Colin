using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Colin.Core.Modulars.Ecses
{
  public interface IEntityCom
  {
    /// <summary>
    /// 执行组件数据初始化.
    /// </summary>
    public void DoInitialize();
  }
}