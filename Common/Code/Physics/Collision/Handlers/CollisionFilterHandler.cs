using Colin.Common.Code.Physics.Dynamics;

namespace Colin.Common.Code.Physics.Collision.Handlers
{
    public delegate bool CollisionFilterHandler( Fixture fixtureA, Fixture fixtureB );
}