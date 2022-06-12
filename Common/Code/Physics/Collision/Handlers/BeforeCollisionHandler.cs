using Colin.Common.Code.Physics.Dynamics;

namespace Colin.Common.Code.Physics.Collision.Handlers
{
    public delegate bool BeforeCollisionHandler( Fixture fixtureA, Fixture fixtureB );
}