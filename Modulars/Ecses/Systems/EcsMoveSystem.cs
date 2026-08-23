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
      EcsComVelLimit _limit;
      Vector2 _velocity;
      for (int count = 0; count < _Entities.Length; count++)
      {
        _current = _Entities[count];
        if (_current is null)
          continue;
        comTransform = _current.GetCom<EcsComTransform>();
        if (comTransform is null)
          continue;
        _limit = _current.GetCom<EcsComVelLimit>();
        if (_limit is null)
          comTransform.Translation += comTransform.DeltaVelocity;
        else
        {
          _velocity = comTransform.DeltaVelocity;
          if (_limit.XLimit > 0)
          {
            if (Math.Abs(_velocity.X) > _limit.XLimit)
              _velocity.X = Math.Clamp(_velocity.X, -_limit.XLimit, _limit.XLimit);
          }
          if (_limit.YLimit > 0)
          {
            if (Math.Abs(_velocity.Y) > _limit.YLimit)
              _velocity.Y = Math.Clamp(_velocity.Y, -_limit.YLimit, _limit.YLimit);
          }
          comTransform.Translation += _velocity;
        }
      }
      base.DoUpdate();
    }
  }
}