using Colin.Core.Modulars.Tiles;
using DeltaMachine.Core.Common.GameSystems;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Modulars
{
  /// <summary>
  /// 业务线处理核心.
  /// </summary>
  public class Business : SceneModule
  {
    private Dictionary<Type, BusinessLine> _businesses = new Dictionary<Type, BusinessLine>();

    /// <summary>
    /// 为指定业务线添加工作项.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="business"></param>
    public void Mark<T>(IBusinessCase business) where T : BusinessLine
    {
      _businesses[typeof(T)].Mark(business);
    }

    /// <summary>
    /// 根据指定类型获取业务线.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public T Get<T>() where T : BusinessLine
    {
      return (T)_businesses[typeof(T)];
    }

    public void Register<T>() where T : BusinessLine, new()
    {
      T t = new T();
      t.Scene = Scene;
      _businesses[typeof(T)] = t;
    }

    public override void DoUpdate(GameTime time)
    {
      for (int index = 0; index < _businesses.Count; index++)
      {
        _businesses.ElementAt(index).Value.DoPrepare();
      }
      for (int index = 0; index < _businesses.Count; index++)
      {
        _businesses.ElementAt(index).Value.DoUpdate();
      }
      base.DoUpdate(time);
    }

    public override void Dispose()
    {
      TileBuildCommand.ResetCache();
      base.Dispose();
    }
  }
}