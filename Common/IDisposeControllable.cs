using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Common
{
  public interface IDisposeControllable : IDisposable
  {
    /// <summary>
    /// 指示当前对象可被回收.
    /// </summary>
    public bool CanDispose { get; set; }
  }
}