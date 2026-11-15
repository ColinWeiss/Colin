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

  /// <summary>
  /// 纯表现组件标记: 只产出渲染数据, 不改世界状态.
  /// <br>实体 <see cref="Entity.LogicSuspended"/> 时, 引擎只跑这类组件的脚本, 让"模拟被外部接管"的实体
  /// 仍然能正常显示(联机远端角色/回放实体)。</br>
  /// </summary>
  public interface IEcsComPresentation;

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