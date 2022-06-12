using Colin.Common.Code.Physics.Collision.Shapes;
using Colin.Common.Code.Physics.Shared;

namespace Colin.Common.Code.Physics.Definitions.Shapes
{
    public sealed class PolygonShapeDef : ShapeDef
    {
        public PolygonShapeDef( ) : base( ShapeType.Polygon )
        {
            SetDefaults( );
        }

        public Vertices Vertices { get; set; }

        public override void SetDefaults( )
        {
            Vertices = null;
            base.SetDefaults( );
        }
    }
}