using Colin.Core.Modulars.Ecses.Components;

namespace Colin.Core.Modulars.Ecses.Systems
{
  /// <summary>
  /// 用以处理实体位移的系统.
  /// </summary>
  public class EcsMoveSystem : Entitiesystem
  {
    private EcsComTransform comTransform;

    public override void DoUpdate()
    {
      Entity[] _Entities = Ecs.Entities;
      Entity _current;
      for (int count = 0; count < _Entities.Length; count++)
      {
        _current = _Entities[count];
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