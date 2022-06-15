namespace Colin.Common.Code.Physics.Extensions.DebugView
{
    [Flags]
    public enum DebugViewFlags
    {
        /// <summary>Do not Render anything</summary>
        None = 0,

        /// <summary>Render shapes.</summary>
        Shape = 1 << 0,

        /// <summary>Render joint connections.</summary>
        Joint = 1 << 1,

        /// <summary>Render axis aligned bounding boxes.</summary>
        AABB = 1 << 2,

        /// <summary>Render broad-phase pairs.</summary>
        Pair = 1 << 3,

        /// <summary>Render center of mass frame.</summary>
        CenterOfMass = 1 << 4,

        /// <summary>Render useful debug data such as timings and number of bodies, joints, contacts and more.</summary>
        DebugPanel = 1 << 5,

        /// <summary>Render contact points between colliding bodies.</summary>
        ContactPoints = 1 << 6,

        /// <summary>Render contact normals. Need ContactPoints to be enabled first.</summary>
        ContactNormals = 1 << 7,

        /// <summary>Renders the vertices of polygons.</summary>
        PolygonPoints = 1 << 8,

        /// <summary>Renders the performance graph.</summary>
        PerformanceGraph = 1 << 9,

        /// <summary>Renders controllers.</summary>
        Controllers = 1 << 10
    }
}