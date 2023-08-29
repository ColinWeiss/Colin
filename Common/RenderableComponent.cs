using Colin.Core.Graphics;

namespace Colin.Core.Common
{
    public class RenderableComponent : Component
    {
        public bool Visiable;

        public virtual void DoRender( ) { }

    }
}