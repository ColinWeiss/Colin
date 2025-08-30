using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Ecses
{
  public interface IEntityCloneableCom
  {
    public void Clone<T>(T com);
  }
}