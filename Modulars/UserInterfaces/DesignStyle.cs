using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;

namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 为划分元素指定设计样式.
    /// </summary>
    public struct DesignStyle
    {
        public Color Color;

        public Vector2 Scale;

        public float Rotation;

        public Vector2 Anchor;

        public DesignStyle( )
        {
            Color = Color.White;
            Scale = Vector2.One;
            Rotation = 0f;
            Anchor = Vector2.Zero;
        }
    }
}
