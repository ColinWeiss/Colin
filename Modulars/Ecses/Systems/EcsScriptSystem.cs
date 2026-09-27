using Colin.Core.Mathematical;
using Colin.Core.Modulars.Ecses.Components;

namespace Colin.Core.Modulars.Ecses.Systems
{
  /// <summary>
  /// 为EcsComScript提供生命周期钩子.
  /// </summary>
  public class EcsScriptSystem : EcsSystem
  {
    public override void Reset()
    {
      Entity _current;
      TypeSnapshotTable<IEcsCom> coms;
      for (int EntityCount = 0; EntityCount < Ecs.Entities.Length; EntityCount++)
      {
        _current = Ecs.Entities[EntityCount];
        if (_current is null)
          continue;
        coms = _current.Components;
        for (int comCount = 0; comCount < coms.Count; comCount++)
        {
          if (coms[comCount] is IResetable resetableCom && resetableCom.ResetEnable)
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
      IEcsCom _EntityCom;
      TypeSnapshotTable<IEcsCom> coms;
      for (int EntityCount = 0; EntityCount < Ecs.Entities.Length; EntityCount++)
      {
        _current = Ecs.Entities[EntityCount];
        if (_current is null)
          continue;
        coms = _current.Components;
        for (int comCount = 0; comCount < coms.Count; comCount++)
        {
          if (coms[comCount] is EcsComScript script && script._updateStarted is false)
          {
            script.UpdateStart();
            script._updateStarted = true;
          }
        }
        for (int comCount = 0; comCount < coms.Count; comCount++)
        {
          if (coms[comCount] is EcsComScript script && script.UpdateEnable)
            script.DoUpdate();
        }
      }
      for (int EntityCount = 0; EntityCount < Ecs.Entities.Length; EntityCount++)
      {
        _current = Ecs.Entities[EntityCount];
        if (_current is null)
          continue;
        coms = _current.Components;
        for (int comCount = 0; comCount < coms.Count; comCount++)
        {
          _EntityCom = coms[comCount];
          if (_EntityCom is IEcsComRemovable removableCom && removableCom.NeedClear)
          {
            coms.Remove(_EntityCom.GetType());
            comCount--;
          }
        }
      }
      base.DoUpdate();
    }
  }
}
