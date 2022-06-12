using Colin.Common.Code.Physics.Collision.ContactSystem;
using Colin.Common.Code.Physics.Dynamics;

namespace Colin.Common.Code.Physics.Collision.Handlers
{
    public delegate void OnCollisionHandler( Fixture fixtureA, Fixture fixtureB, Contact contact );
}