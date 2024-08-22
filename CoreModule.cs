using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core
{
  public class CoreModule
  {
    public virtual void DoInitialize() { }

    public bool Enable => true;
    public virtual void DoUpdate(GameTime time) { }

    public bool Visible => false;
    public virtual void DoRender(GraphicsDevice device, SpriteBatch batch) { }
  }
}