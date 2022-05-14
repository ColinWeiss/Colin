using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.UI
{
    /// <summary>
    /// 表示一个提供了基本布局元素的对象.
    /// </summary>
    public interface ILayout
    {
        float LocationX { get; }
        float LocationY { get; }
        float Width { get; }
        float Height { get; }
    }
}