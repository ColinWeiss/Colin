using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Ecses.Components
{
    /// <summary>
    /// 这是切片最基底的规则...
    /// </summary>
    public class EcsComBasic
    {
        /// <summary>
        /// 时间流逝速度.
        /// </summary>
        public float TimeScale = 1f;

        /// <summary>
        /// 指示切片是否魔怔.
        /// <br>没有魔怔程度这种形容说法.</br>
        /// <br>它魔怔了那就是魔怔了, 哪怕它表现起来与其他同族切片无异.</br>
        /// <br>...这会带来极其严重的后果.</br>
        /// </summary>
        public bool Possessed;
    }
}