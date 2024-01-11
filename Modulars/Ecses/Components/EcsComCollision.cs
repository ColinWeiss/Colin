using Colin.Core.Modulars.Collisions;

namespace Colin.Core.Modulars.Ecses.Components
{
    public class EcsComCollision : ISectionComponent
    {
        public Polygon Polygon;
        public Vector2 Offset;
        public float Rotation
        {
            get => Polygon.Rotation;
            set => Polygon.Rotation = value;
        }
        public event Action<Section, Section> OnSectionCollision;
        public event Action<Section> OnTileCollision;
        public void DoSectionCollisionEvent(Section section0, Section section1) => OnSectionCollision?.Invoke(section0, section1);
        public void DoTileCollisionEvent(Section section0) => OnTileCollision?.Invoke(section0);
        public void DoInitialize() { }
    }
}