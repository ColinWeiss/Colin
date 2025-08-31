using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Ecses
{
  public interface IEcsComCloneable
  {
    public void Clone<T>(T com);
  }
}