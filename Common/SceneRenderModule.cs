using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Common
{
  public class SceneRenderModule : SceneModule, IRenderableISceneModule
  {
    public RenderTarget2D RawRt { get; set; }
    public bool RawRtVisible { get; set; }
    public bool Presentation { get; set; }

    public virtual void DoRawRender(GraphicsDevice device, SpriteBatch batch) { }

    public virtual void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch) { }
  }
}