using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Common
{
    /// <summary>
    /// 代表一个层级树元素.
    /// </summary>
    public interface IHierarchicalElement
    {
        private void HierarchicalInitialize()
        {
            DoInitialize();
            IHierarchicalElement element;
            if (this is IEnumerable<IHierarchicalElement> child)
            {
                for (int count = 0; count < child.Count(); count++)
                {
                    element = child.ElementAt(count);
                    element.HierarchicalInitialize();
                }
            }
        }

        /// <summary>
        /// 执行层级元素初始化内容.
        /// </summary>
        public void DoInitialize();

        /// <summary>
        /// 执行该层级元素及其下所有子元素的初始化方法.
        /// </summary>
        /// <param name="element"></param>
        public static void DoElementInitialize(IHierarchicalElement element) => element.HierarchicalInitialize();

    }
}