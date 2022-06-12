using Colin.Common.Code.Physics.Collision.ContactSystem;
using Colin.Common.Code.Physics.Dynamics;
using Colin.Common.Code.Physics.Dynamics.Solver;

namespace Colin.Common.Code.Physics.Collision.Handlers
{
    public delegate void AfterCollisionHandler( Fixture fixtureA, Fixture fixtureB, Contact contact, ContactVelocityConstraint impulse );
}