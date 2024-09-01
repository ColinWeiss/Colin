using Colin.Core.Modulars.Ecses.Components;
using static System.Collections.Generic.Dictionary<System.Type, Colin.Core.Modulars.Ecses.IEntityCom>;

namespace Colin.Core.Modulars.Ecses.Systems
{
  /// <summary>
  /// 为EcsComScript提供生命周期钩子.
  /// </summary>
  public class EcsScriptSystem : Entitiesystem
  {
    public override void Reset()
    {
      Entity _current;
      for (int EntityCount = 0; EntityCount < Ecs.Entities.Length; EntityCount++)
      {
        _current = Ecs.Entities[EntityCount];
        if (_current is null)
          continue;
        foreach (IEntityCom component in _current.Components.Values)
        {
          if (component is IResetable resetableCom && resetableCom.ResetEnable)
          {
            resetableCom.Reset();
            resetableCom.ResetEnable = true;
          }
        }
      }
      base.Reset();
    }
    public override void DoUpdate()
    {
      Entity _current;
      IEntityCom _EntityCom;
      Dictionary<Type, IEntityCom> comDic;
      ValueCollection coms;
      for (int EntityCount = 0; EntityCount < Ecs.Entities.Length; EntityCount++)
      {
        _current = Ecs.Entities[EntityCount];
        if (_current is null)
          continue;
        comDic = _current.Components;
        coms = comDic.Values;
        foreach (IEntityCom component in coms)
        {
          if (component is EcsComScript script && script._updateStarted is false)
          {
            script.UpdateStart();
            script._updateStarted = true;
          }
        }
        foreach (IEntityCom component in coms)
          if (component is EcsComScript script && script.UpdateEnable)
            script.DoUpdate();
      }
      for (int EntityCount = 0; EntityCount < Ecs.Entities.Length; EntityCount++)
      {
        _current = Ecs.Entities[EntityCount];
        if (_current is null)
          continue;
        comDic = _current.Components;
        coms = comDic.Values;
        for (int comCount = 0; comCount < coms.Count; comCount++)
        {
          _EntityCom = coms.ElementAt(comCount);
          if (_EntityCom is IEntityRemovableCom removableCom && removableCom.NeedClear)
          {
            comDic.Remove(_EntityCom.GetType());
            comCount--;
          }
        }
      }
      base.DoUpdate();
    }
  }
}