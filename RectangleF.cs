using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace Colin.Core
{
    public struct RectangleF
    {
        private static RectangleF emptyRectangleF;

        [DataMember]
        public float X;

        [DataMember]
        public float Y;

        [DataMember]
        public float Width;

        [DataMember]
        public float Height;

        public static RectangleF Empty => emptyRectangleF;

        public float Left => X;

        public float Right => X + Width;

        public float Top => Y;

        public float Bottom => Y + Height;

        public bool IsEmpty
        {
            get
            {
                if(Width == 0 && Height == 0 && X == 0)
                {
                    return Y == 0;
                }

                return false;
            }
        }

        public Vector2 Location
        {
            get
            {
                return new Vector2( X, Y );
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2 Size
        {
            get
            {
                return new Vector2( Width, Height );
            }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public Vector2 Center => new Vector2( X + Width / 2, Y + Height / 2 );

        public Vector2 Half => new Vector2( Width / 2, Height / 2 );

        internal string DebugDisplayString => X + "  " + Y + "  " + Width + "  " + Height;

        public RectangleF( float x, float y, float width, float height )
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectangleF( Vector2 location, Vector2 size )
        {
            X = location.X;
            Y = location.Y;
            Width = size.X;
            Height = size.Y;
        }

        public bool Contains( float x, float y )
        {
            if(X <= x && x < X + Width && Y <= y)
            {
                return y < Y + Height;
            }

            return false;
        }

        public bool Contains( Vector2 value )
        {
            if(X <= value.X && value.X < X + Width && Y <= value.Y)
            {
                return value.Y < Y + Height;
            }

            return false;
        }

        public void Contains( ref Vector2 value, out bool result )
        {
            result = X <= value.X && value.X < X + Width && Y <= value.Y && value.Y < Y + Height;
        }

        public bool Contains( RectangleF value )
        {
            if(X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y)
            {
                return value.Y + value.Height <= Y + Height;
            }

            return false;
        }

        public void Contains( ref RectangleF value, out bool result )
        {
            result = X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y && value.Y + value.Height <= Y + Height;
        }

        public void Inflate( float horizontalAmount, float verticalAmount )
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        public bool Intersects( RectangleF value )
        {
            if(value.Left < Right && Left < value.Right && value.Top < Bottom)
            {
                return Top < value.Bottom;
            }

            return false;
        }

        public void Intersects( ref RectangleF value, out bool result )
        {
            result = value.Left < Right && Left < value.Right && value.Top < Bottom && Top < value.Bottom;
        }

        public static RectangleF Intersect( RectangleF value1, RectangleF value2 )
        {
            Intersect( ref value1, ref value2, out var result );
            return result;
        }

        public static void Intersect( ref RectangleF value1, ref RectangleF value2, out RectangleF result )
        {
            if(value1.Intersects( value2 ))
            {
                float num = Math.Min( value1.X + value1.Width, value2.X + value2.Width );
                float num2 = Math.Max( value1.X, value2.X );
                float num3 = Math.Max( value1.Y, value2.Y );
                float num4 = Math.Min( value1.Y + value1.Height, value2.Y + value2.Height );
                result = new RectangleF( num2, num3, num - num2, num4 - num3 );
            }
            else
            {
                result = new RectangleF( 0, 0, 0, 0 );
            }
        }

        public void Offset( float offsetX, float offsetY )
        {
            X += offsetX;
            Y += offsetY;
        }

        public void Offset( Vector2 amount )
        {
            X += amount.X;
            Y += amount.Y;
        }

        public override string ToString( )
        {
            return "{X:" + X + " Y:" + Y + " Width:" + Width + " Height:" + Height + "}";
        }

        public static RectangleF Union( RectangleF value1, RectangleF value2 )
        {
            float num = Math.Min( value1.X, value2.X );
            float num2 = Math.Min( value1.Y, value2.Y );
            return new RectangleF( num, num2, Math.Max( value1.Right, value2.Right ) - num, Math.Max( value1.Bottom, value2.Bottom ) - num2 );
        }

        public static void Union( ref RectangleF value1, ref RectangleF value2, out RectangleF result )
        {
            result.X = Math.Min( value1.X, value2.X );
            result.Y = Math.Min( value1.Y, value2.Y );
            result.Width = Math.Max( value1.Right, value2.Right ) - result.X;
            result.Height = Math.Max( value1.Bottom, value2.Bottom ) - result.Y;
        }

        public void Deconstruct( out float x, out float y, out float width, out float height )
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }
    }
}
