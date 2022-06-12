using Colin.Common.Code.Physics.Collision.ContactSystem;
using Colin.Common.Code.Physics.Collision.Narrowphase;

namespace Colin.Common.Code.Physics.Dynamics.Handlers
{
    public delegate void PreSolveHandler( Contact contact, ref Manifold oldManifold );
}