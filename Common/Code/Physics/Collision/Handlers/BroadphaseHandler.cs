using Colin.Common.Code.Physics.Dynamics;

namespace Colin.Common.Code.Physics.Collision.Handlers
{
    public delegate void BroadphaseHandler( ref FixtureProxy proxyA, ref FixtureProxy proxyB );
}