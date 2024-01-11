using Colin.Core.Modulars.Ecses.Components;

namespace Colin.Core.Modulars.Ecses.Systems
{
    public class EcsCollisionSystem : SectionSystem
    {
        public override void DoUpdate()
        {
            EcsComCollision collision = Current.GetComponent<EcsComCollision>();
            if (collision is null)
                return;
            foreach (var target in Ecs.Sections)
            {
                EcsComCollision targetCollision = target.GetComponent<EcsComCollision>();
                if (collision.Polygon is null || targetCollision.Polygon is null)
                    return;
                if (collision.Polygon.Overlaps(targetCollision.Polygon))
                    collision.DoSectionCollisionEvent(Current, target);
            }
            base.DoUpdate();
        }
    }
}