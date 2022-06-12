using Colin.Common.Code.Physics.Collision.ContactSystem;
using Microsoft.Xna.Framework;

namespace Colin.Common.Code.Physics.Collision.Narrowphase
{
    /// <summary>Used for computing contact manifolds.</summary>
    internal struct ClipVertex
    {
        public ContactId Id;
        public Vector2 V;
    }
}