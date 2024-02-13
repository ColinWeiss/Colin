using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Common
{
    public interface IHierarchical
    {
        public IEnumerable<IHierarchical> Elements { get; }
        public void DoInitialize()
        {
            InitializeSelf();
            IHierarchical element;
            for (int count = 0; count < Elements.Count(); count++)
            {
                element = Elements.ElementAt(count);
                element.DoInitialize();
            }
        }
        public void DoReset()
        {
            IHierarchical element;
            for (int count = 0; count < Elements.Count(); count++)
            {
                element = Elements.ElementAt(count);
                if ( element is IResetable resetable )
                {
                    if(resetable.ResetEnable)
                        resetable.Reset();
                }
            }
        }
        public void InitializeSelf();
    }
}