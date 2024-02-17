using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Graphics
{
    /// <summary>
    /// https://easings.net/zh-cn.
    /// </summary>
    public static class Easing
    {
        public static float EaseInSine( float percentage )
        {
            return 1 - MathF.Cos(percentage * 3.1415926f / 2f);
        }

        public static float EaseOutSine(float percentage)
        {
            return MathF.Sin(percentage * 3.1415926f / 2f);
        }

        public static float EaseInOutSine( float percentage )
        {
            return -(MathF.Cos(3.1415926f* percentage) - 1f) / 2f;
        }

        public static float EaseOutExpo(float percentage)
        {
            return percentage == 1f ? 1f : 1f - MathF.Pow(2, -10 * percentage);
        }
    }
}