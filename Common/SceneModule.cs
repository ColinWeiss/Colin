using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Common
{
  public class SceneModule : ISceneModule
  {
    public Scene Scene { get; set; }

    public bool Enable { get; set; }

    public virtual void DoInitialize()
    {
    }
    public virtual void Start()
    {
    }
    public virtual void DoUpdate(GameTime time)
    {
    }
    public virtual void Dispose()
    {
    }
  }
}