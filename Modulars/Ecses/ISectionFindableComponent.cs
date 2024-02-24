using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Ecses
{
    public interface ISectionFindableComponent : ISectionComponent
    {
        public Section Section { get; set; }
    }
}
