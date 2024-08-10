using Colin.Core.Modulars.Ecses.Components;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Ecses.Systems
{
  /// <summary>
  /// 用以处理切片位移的系统.
  /// </summary>
  public class EcsMoveSystem : SectionSystem
  {
    private EcsComTransform comTransform;

    public override void DoUpdate()
    {
      Section[] _sections = Ecs.Sections;
      Section _current;
      for (int count = 0; count < _sections.Length; count++)
      {
        _current = _sections[count];
        if (_current is null)
          continue;
        comTransform = _current.GetComponent<EcsComTransform>();
        if (comTransform is null)
          continue;
        comTransform.Translation += comTransform.Velocity * Time.DeltaTime;
      }
      base.DoUpdate();
    }
  }
}