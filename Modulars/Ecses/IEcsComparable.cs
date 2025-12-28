using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Ecses
{
  interface IEcsComparable
  {
    public bool Compare(IEcsComparable ecsCom);
  }
}