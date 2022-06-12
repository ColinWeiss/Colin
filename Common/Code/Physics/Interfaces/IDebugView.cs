using Colin.Common.Code.Physics.Collision.Shapes;
using Colin.Common.Code.Physics.Dynamics.Joints;
using Colin.Common.Code.Physics.Shared;
using Microsoft.Xna.Framework;

namespace Colin.Common.Code.Physics.Interfaces
{
    public interface IDebugView
    {
        void DrawJoint( Joint joint );
        void DrawShape( Shape shape, ref Transform transform, Color color );
    }
}