using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin
{
    /// <summary>
    /// 表示一个2D元素.
    /// </summary>
    public interface IElement2D
    {
        /// <summary>
        /// 元素的横坐标.
        /// </summary>
        public float PositionX { get; set; }

        /// <summary>
        /// 元素的纵坐标.
        /// </summary>
        public float PositionY { get; set; }

        /// <summary>
        /// 元素的横向分速度.
        /// </summary>
        public float VelocityX { get; set; }

        /// <summary>
        /// 元素的纵向分速度.
        /// </summary>
        public float VelocityY { get; set; }

        /// <summary>
        /// 元素的宽度.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// 元素的高度.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// 元素的缩放.
        /// </summary>
        public float Scale { get; set; }

    }
}
