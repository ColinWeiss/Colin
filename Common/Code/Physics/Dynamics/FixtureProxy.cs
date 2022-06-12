using Colin.Common.Code.Physics.Shared;

namespace Colin.Common.Code.Physics.Dynamics
{
    /// <summary>This proxy is used internally to connect fixtures to the broad-phase.</summary>
    public struct FixtureProxy
    {
        public AABB AABB;
        public int ChildIndex;
        public Fixture Fixture;
        public int ProxyId;
    }
}