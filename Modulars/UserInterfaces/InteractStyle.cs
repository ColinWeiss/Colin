using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 为划分元素指定交互样式.
    /// </summary>
    public struct InteractStyle
    {
        /// <summary>
        /// 指示划分元素当前是否具有焦点.
        /// </summary>
        public bool Focus;

        /// <summary>
        /// 指示划分元素上一帧是否具有焦点.
        /// </summary>
        public bool FocusLast;

        /// <summary>
        /// 指示划分元素当前的交互状态.
        /// </summary>
        public bool Interaction;

        /// <summary>
        /// 指示划分元素上一帧的交互状态.
        /// </summary>
        public bool InteractionLast;

        /// <summary>
        /// 指示划分元素是否可拖拽.
        /// </summary>
        public bool IsDraggable;

        /// <summary>
        /// 若 <see cref="IsDraggable"/> 为 <see langword="true"/>, 则针对此矩形进行限制.
        /// </summary>
        public Rectangle DragLimit;

        /// <summary>
        /// 指示划分元素是否为可交互的.
        /// </summary>
        public bool IsInteractive;

        /// <summary>
        /// 指示划分元素是否为可选中的.
        /// </summary>
        public bool IsSelectable;
    }
}