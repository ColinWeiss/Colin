using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Common.Code
{
    /// <summary>
    /// 表示一个帧动画对象.
    /// </summary>
    public interface IFrameAnimation
    {
        public Rectangle GetFrame( );
    }
}