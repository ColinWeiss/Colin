/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
*/

using Colin.Common.Code.Physics.Dynamics;
using Colin.Common.Code.Physics.Shared;
using Microsoft.Xna.Framework;

namespace Colin.Common.Code.Physics.Extensions.DebugView
{
    /// <summary>Implement and register this class with a World to provide debug Rendering of physics entities in your game.</summary>
    public abstract class DebugViewBase
    {
        protected DebugViewBase( World world )
        {
            World = world;
        }

        protected World World { get; }

        /// <summary>Gets or sets the debug view flags.</summary>
        /// <value>The flags.</value>
        public DebugViewFlags Flags { get; set; }

        /// <summary>Append flags to the current flags.</summary>
        /// <param name="flags">The flags.</param>
        public void AppendFlags( DebugViewFlags flags )
        {
            Flags |= flags;
        }

        /// <summary>Remove flags from the current flags.</summary>
        /// <param name="flags">The flags.</param>
        public void RemoveFlags( DebugViewFlags flags )
        {
            Flags &= ~flags;
        }

        /// <summary>Render a closed polygon provided in CCW order.</summary>
        public abstract void RenderPolygon( Vector2[ ] vertices, int count, Color color, bool closed = true );

        /// <summary>Render a solid closed polygon provided in CCW order.</summary>
        public abstract void RenderSolidPolygon( Vector2[ ] vertices, int count, Color color, bool outline = true );

        /// <summary>Render a circle.</summary>
        public abstract void RenderCircle( Vector2 center, float radius, Color color );

        /// <summary>Render a solid circle.</summary>
        public abstract void RenderSolidCircle( Vector2 center, float radius, Vector2 axis, Color color );

        /// <summary>Render a line segment.</summary>
        public abstract void RenderSegment( Vector2 start, Vector2 end, Color color );

        /// <summary>Render a transform. Choose your own length scale.</summary>
        /// <param name="transform">The transform.</param>
        public abstract void RenderTransform( ref Transform transform );
    }
}