using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core
{
    /// <summary>
    /// 表示允许进行重置操作的对象.
    /// </summary>
    public interface IResetable
    {
        /// <summary>
        /// 重置启用.
        /// <br>[!] 该功能每帧重置为 <see langword="true"/>, 若要每帧关闭请在每帧执行的逻辑下为其赋值 <see langword="false"/>.</br>
        /// </summary>
        public bool ResetEnable { get; set; }
        void Reset();
    }
}