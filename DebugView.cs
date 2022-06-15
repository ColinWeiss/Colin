using Colin.Common.Code.Physics.Dynamics;
using Colin.Common.Code.Physics.Extensions.DebugView;
using Colin.Common.Code.Physics.Shared;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin
{
    public class DebugView : DebugViewBase
    {
        public DebugView( World world ) : base( world )
        {
        }

        public override void RenderCircle( Vector2 center, float radius, Color color )
        {
            throw new NotImplementedException( );
        }

        public override void RenderPolygon( Vector2[ ] vertices, int count, Color color, bool closed = true )
        {
            throw new NotImplementedException( );
        }

        public override void RenderSegment( Vector2 start, Vector2 end, Color color )
        {
            throw new NotImplementedException( );
        }

        public override void RenderSolidCircle( Vector2 center, float radius, Vector2 axis, Color color )
        {
            throw new NotImplementedException( );
        }

        public override void RenderSolidPolygon( Vector2[ ] vertices, int count, Color color, bool outline = true )
        {
            throw new NotImplementedException( );
        }

        public override void RenderTransform( ref Transform transform )
        {
            throw new NotImplementedException( );
        }
    }
}
