using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.ModLoaders
{
    public interface IModType
    {
        public IMod Mod { get; set; }
    }
}