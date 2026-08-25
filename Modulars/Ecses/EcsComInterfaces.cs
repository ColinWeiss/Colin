using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Ecses
{
  public interface IEcsCom
  {
    /// <summary>
    /// 执行组件数据初始化.
    /// </summary>
    void DoInitialize();
  }

  public interface IEcsComBindable : IEcsCom
  {
    Entity Entity { get; set; }
  }

  public interface IEcsComCloneable
  {
    void Clone<T>(T com);
  }

  public interface IEcsComEvent;

  public interface IEcsComparable
  {
    public bool Compare(IEcsComparable ecsCom);
  }

  public interface IEcsComRemovable
  {
    bool NeedClear { get; set; }
  }

  public interface IEcsComFinalize
  {
    void DoFinalize();
  }
}