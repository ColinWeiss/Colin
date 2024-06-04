using Colin.Core.Modulars.Ecses.Components;
using static System.Collections.Generic.Dictionary<System.Type, Colin.Core.Modulars.Ecses.ISectionCom>;

namespace Colin.Core.Modulars.Ecses.Systems
{
  /// <summary>
  /// 为EcsComScript提供生命周期钩子.
  /// </summary>
  public class EcsScriptSystem : SectionSystem
  {
    public override void Reset()
    {
      Section _current;
      for (int sectionCount = 0; sectionCount < Ecs.Sections.Length; sectionCount++)
      {
        _current = Ecs.Sections[sectionCount];
        if (_current is null)
          continue;
        foreach (ISectionCom component in _current.Components.Values)
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
      Section _current;
      ISectionCom _sectionCom;
      Dictionary<Type, ISectionCom> comDic;
      ValueCollection coms;
      for (int sectionCount = 0; sectionCount < Ecs.Sections.Length; sectionCount++)
      {
        _current = Ecs.Sections[sectionCount];
        if (_current is null)
          continue;
        comDic = _current.Components;
        coms = comDic.Values;
        foreach (ISectionCom component in coms)
        {
          if (component is EcsComScript script && script._updateStarted is false)
          {
            script.UpdateStart();
            script._updateStarted = true;
          }
        }
        foreach (ISectionCom component in coms)
          if (component is EcsComScript script && script.UpdateEnable)
            script.DoUpdate();
      }
      for (int sectionCount = 0; sectionCount < Ecs.Sections.Length; sectionCount++)
      {
        _current = Ecs.Sections[sectionCount];
        if (_current is null)
          continue;
        comDic = _current.Components;
        coms = comDic.Values;
        for (int comCount = 0; comCount < coms.Count; comCount++)
        {
          _sectionCom = coms.ElementAt(comCount);
          if (_sectionCom is ISectionRemovableCom removableCom && removableCom.NeedClear)
          {
            comDic.Remove(_sectionCom.GetType());
            comCount--;
          }
        }
      }
      base.DoUpdate();
    }
  }
}