using Colin.Common.Code.Physics.Collision.Shapes;
using Colin.Common.Code.Physics.Dynamics.Joints;
using Colin.Common.Code.Physics.Shared;
using Microsoft.Xna.Framework;

namespace Colin.Common.Code.Physics.Interfaces
{
    public interface IDebugView
    {
        void RenderJoint( Joint joint );
        void RenderShape( Shape shape, ref Transform transform, Color color );
    }
}