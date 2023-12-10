using System.Text;

namespace Colin.Core
{
    public struct Point3 : IEquatable<Point3>
    {
        private static readonly Point3 zero = new Point3( 0, 0, 0 );

        private static readonly Point3 one = new Point3( 1, 1, 1 );

        private static readonly Point3 unitX = new Point3( 1, 0, 0 );

        private static readonly Point3 unitY = new Point3( 0, 1, 0 );

        private static readonly Point3 unitZ = new Point3( 0, 0, 1 );

        private static readonly Point3 up = new Point3( 0, 1, 0 );

        private static readonly Point3 down = new Point3( 0, -1, 0 );

        private static readonly Point3 right = new Point3( 1, 0, 0 );

        private static readonly Point3 left = new Point3( -1, 0, 0 );

        private static readonly Point3 forward = new Point3( 0, 0, -1 );

        private static readonly Point3 backward = new Point3( 0, 0, 1 );

        public int X;

        public int Y;

        public int Z;

        public static Point3 Zero => zero;

        public static Point3 One => one;

        public static Point3 UnitX => unitX;

        public static Point3 UnitY => unitY;

        public static Point3 UnitZ => unitZ;

        public static Point3 Up => up;

        public static Point3 Down => down;

        public static Point3 Right => right;

        public static Point3 Left => left;

        public static Point3 Forward => forward;

        public static Point3 Backward => backward;

        internal string DebugDisplayString => X + "  " + Y + "  " + Z;

        public Point3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Point3(float x, float y, float z)
        {
            X = (int)x;
            Y = (int)y;
            Z = (int)z;
        }

        public Point3(int value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public Point3(float value)
        {
            X = (int)value;
            Y = (int)value;
            Z = (int)value;
        }


        public Point3(Point value, int z)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
        }

        public static Point3 Add(Point3 value1, Point3 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        public static void Add(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
        }

        public static Point3 Barycentric(Point3 value1, Point3 value2, Point3 value3, int amount1, int amount2)
        {
            return new Point3( MathHelper.Barycentric( value1.X, value2.X, value3.X, amount1, amount2 ), MathHelper.Barycentric( value1.Y, value2.Y, value3.Y, amount1, amount2 ), MathHelper.Barycentric( value1.Z, value2.Z, value3.Z, amount1, amount2 ) );
        }

        public static void Barycentric(ref Point3 value1, ref Point3 value2, ref Point3 value3, int amount1, int amount2, out Point3 result)
        {
            result.X = (int)MathHelper.Barycentric( value1.X, value2.X, value3.X, amount1, amount2 );
            result.Y = (int)MathHelper.Barycentric( value1.Y, value2.Y, value3.Y, amount1, amount2 );
            result.Z = (int)MathHelper.Barycentric( value1.Z, value2.Z, value3.Z, amount1, amount2 );
        }

        public static Point3 CatmullRom(Point3 value1, Point3 value2, Point3 value3, Point3 value4, int amount)
        {
            return new Point3( MathHelper.CatmullRom( value1.X, value2.X, value3.X, value4.X, amount ), MathHelper.CatmullRom( value1.Y, value2.Y, value3.Y, value4.Y, amount ), MathHelper.CatmullRom( value1.Z, value2.Z, value3.Z, value4.Z, amount ) );
        }

        public static void CatmullRom(ref Point3 value1, ref Point3 value2, ref Point3 value3, ref Point3 value4, int amount, out Point3 result)
        {
            result.X = (int)MathHelper.CatmullRom( value1.X, value2.X, value3.X, value4.X, amount );
            result.Y = (int)MathHelper.CatmullRom( value1.Y, value2.Y, value3.Y, value4.Y, amount );
            result.Z = (int)MathHelper.CatmullRom( value1.Z, value2.Z, value3.Z, value4.Z, amount );
        }

        public void Ceiling()
        {
            X = (int)MathF.Ceiling( X );
            Y = (int)MathF.Ceiling( Y );
            Z = (int)MathF.Ceiling( Z );
        }

        public static Point3 Ceiling(Point3 value)
        {
            value.X = (int)MathF.Ceiling( value.X );
            value.Y = (int)MathF.Ceiling( value.Y );
            value.Z = (int)MathF.Ceiling( value.Z );
            return value;
        }

        public static void Ceiling(ref Point3 value, out Point3 result)
        {
            result.X = (int)MathF.Ceiling( value.X );
            result.Y = (int)MathF.Ceiling( value.Y );
            result.Z = (int)MathF.Ceiling( value.Z );
        }

        public static Point3 Clamp(Point3 value1, Point3 min, Point3 max)
        {
            return new Point3( MathHelper.Clamp( value1.X, min.X, max.X ), MathHelper.Clamp( value1.Y, min.Y, max.Y ), MathHelper.Clamp( value1.Z, min.Z, max.Z ) );
        }

        public static void Clamp(ref Point3 value1, ref Point3 min, ref Point3 max, out Point3 result)
        {
            result.X = MathHelper.Clamp( value1.X, min.X, max.X );
            result.Y = MathHelper.Clamp( value1.Y, min.Y, max.Y );
            result.Z = MathHelper.Clamp( value1.Z, min.Z, max.Z );
        }

        public static Point3 Cross(Point3 vector1, Point3 Point)
        {
            Cross( ref vector1, ref Point, out vector1 );
            return vector1;
        }

        public static void Cross(ref Point3 vector1, ref Point3 Point, out Point3 result)
        {
            int x = vector1.Y * Point.Z - Point.Y * vector1.Z;
            int y = -(vector1.X * Point.Z - Point.X * vector1.Z);
            int z = vector1.X * Point.Y - Point.X * vector1.Y;
            result.X = x;
            result.Y = y;
            result.Z = z;
        }

        public static int Distance(Point3 value1, Point3 value2)
        {
            DistanceSquared( ref value1, ref value2, out var result );
            return (int)MathF.Sqrt( result );
        }

        public static void Distance(ref Point3 value1, ref Point3 value2, out int result)
        {
            DistanceSquared( ref value1, ref value2, out result );
            result = (int)MathF.Sqrt( result );
        }

        public static int DistanceSquared(Point3 value1, Point3 value2)
        {
            return (value1.X - value2.X) * (value1.X - value2.X) + (value1.Y - value2.Y) * (value1.Y - value2.Y) + (value1.Z - value2.Z) * (value1.Z - value2.Z);
        }

        public static void DistanceSquared(ref Point3 value1, ref Point3 value2, out int result)
        {
            result = (value1.X - value2.X) * (value1.X - value2.X) + (value1.Y - value2.Y) * (value1.Y - value2.Y) + (value1.Z - value2.Z) * (value1.Z - value2.Z);
        }

        public static Point3 Divide(Point3 value1, Point3 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        public static Point3 Divide(Point3 value1, int divider)
        {
            int num = 1 / divider;
            value1.X *= num;
            value1.Y *= num;
            value1.Z *= num;
            return value1;
        }

        public static void Divide(ref Point3 value1, int divider, out Point3 result)
        {
            int num = 1 / divider;
            result.X = value1.X * num;
            result.Y = value1.Y * num;
            result.Z = value1.Z * num;
        }

        public static void Divide(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
        }

        public static int Dot(Point3 value1, Point3 value2)
        {
            return value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
        }

        public static void Dot(ref Point3 value1, ref Point3 value2, out int result)
        {
            result = value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point3))
            {
                return false;
            }

            Point3 vector = (Point3)obj;
            if (X == vector.X && Y == vector.Y)
            {
                return Z == vector.Z;
            }

            return false;
        }

        public bool Equals(Point3 other)
        {
            if (X == other.X && Y == other.Y)
            {
                return Z == other.Z;
            }

            return false;
        }

        public void Floor()
        {
            X = (int)MathF.Floor( X );
            Y = (int)MathF.Floor( Y );
            Z = (int)MathF.Floor( Z );
        }

        public static Point3 Floor(Point3 value)
        {
            value.X = (int)MathF.Floor( value.X );
            value.Y = (int)MathF.Floor( value.Y );
            value.Z = (int)MathF.Floor( value.Z );
            return value;
        }

        public static void Floor(ref Point3 value, out Point3 result)
        {
            result.X = (int)MathF.Floor( value.X );
            result.Y = (int)MathF.Floor( value.Y );
            result.Z = (int)MathF.Floor( value.Z );
        }

        public override int GetHashCode()
        {
            return (((X.GetHashCode() * 397) ^ Y.GetHashCode()) * 397) ^ Z.GetHashCode();
        }

        public static Point3 Hermite(Point3 value1, Point3 tangent1, Point3 value2, Point3 tangent2, int amount)
        {
            return new Point3( MathHelper.Hermite( value1.X, tangent1.X, value2.X, tangent2.X, amount ), MathHelper.Hermite( value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount ), MathHelper.Hermite( value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount ) );
        }

        public static void Hermite(ref Point3 value1, ref Point3 tangent1, ref Point3 value2, ref Point3 tangent2, int amount, out Point3 result)
        {
            result.X = (int)MathHelper.Hermite( value1.X, tangent1.X, value2.X, tangent2.X, amount );
            result.Y = (int)MathHelper.Hermite( value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount );
            result.Z = (int)MathHelper.Hermite( value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount );
        }

        public int Length()
        {
            return (int)MathF.Sqrt( X * X + Y * Y + Z * Z );
        }

        public int LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public static Point3 Lerp(Point3 value1, Point3 value2, int amount)
        {
            return new Point3( MathHelper.Lerp( value1.X, value2.X, amount ), MathHelper.Lerp( value1.Y, value2.Y, amount ), MathHelper.Lerp( value1.Z, value2.Z, amount ) );
        }

        public static void Lerp(ref Point3 value1, ref Point3 value2, int amount, out Point3 result)
        {
            result.X = (int)MathHelper.Lerp( value1.X, value2.X, amount );
            result.Y = (int)MathHelper.Lerp( value1.Y, value2.Y, amount );
            result.Z = (int)MathHelper.Lerp( value1.Z, value2.Z, amount );
        }

        public static Point3 LerpPrecise(Point3 value1, Point3 value2, int amount)
        {
            return new Point3( MathHelper.LerpPrecise( value1.X, value2.X, amount ), MathHelper.LerpPrecise( value1.Y, value2.Y, amount ), MathHelper.LerpPrecise( value1.Z, value2.Z, amount ) );
        }

        public static void LerpPrecise(ref Point3 value1, ref Point3 value2, int amount, out Point3 result)
        {
            result.X = (int)MathHelper.LerpPrecise( value1.X, value2.X, amount );
            result.Y = (int)MathHelper.LerpPrecise( value1.Y, value2.Y, amount );
            result.Z = (int)MathHelper.LerpPrecise( value1.Z, value2.Z, amount );
        }

        public static Point3 Max(Point3 value1, Point3 value2)
        {
            return new Point3( MathHelper.Max( value1.X, value2.X ), MathHelper.Max( value1.Y, value2.Y ), MathHelper.Max( value1.Z, value2.Z ) );
        }

        public static void Max(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = MathHelper.Max( value1.X, value2.X );
            result.Y = MathHelper.Max( value1.Y, value2.Y );
            result.Z = MathHelper.Max( value1.Z, value2.Z );
        }

        public static Point3 Min(Point3 value1, Point3 value2)
        {
            return new Point3( MathHelper.Min( value1.X, value2.X ), MathHelper.Min( value1.Y, value2.Y ), MathHelper.Min( value1.Z, value2.Z ) );
        }

        public static void Min(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = MathHelper.Min( value1.X, value2.X );
            result.Y = MathHelper.Min( value1.Y, value2.Y );
            result.Z = MathHelper.Min( value1.Z, value2.Z );
        }

        public static Point3 Multiply(Point3 value1, Point3 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        public static Point3 Multiply(Point3 value1, int scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            value1.Z *= scaleFactor;
            return value1;
        }

        public static void Multiply(ref Point3 value1, int scaleFactor, out Point3 result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
        }

        public static void Multiply(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
        }

        public static Point3 Negate(Point3 value)
        {
            value = new Point3( 0 - value.X, 0 - value.Y, 0 - value.Z );
            return value;
        }

        public static void Negate(ref Point3 value, out Point3 result)
        {
            result.X = 0 - value.X;
            result.Y = 0 - value.Y;
            result.Z = 0 - value.Z;
        }

        public void Normalize()
        {
            int num = (int)MathF.Sqrt( X * X + Y * Y + Z * Z );
            num = 1 / num;
            X *= num;
            Y *= num;
            Z *= num;
        }

        public static Point3 Normalize(Point3 value)
        {
            int num = (int)MathF.Sqrt( value.X * value.X + value.Y * value.Y + value.Z * value.Z );
            num = 1 / num;
            return new Point3( value.X * num, value.Y * num, value.Z * num );
        }

        public static void Normalize(ref Point3 value, out Point3 result)
        {
            int num = (int)MathF.Sqrt( value.X * value.X + value.Y * value.Y + value.Z * value.Z );
            num = 1 / num;
            result.X = value.X * num;
            result.Y = value.Y * num;
            result.Z = value.Z * num;
        }

        public static Point3 Reflect(Point3 vector, Point3 normal)
        {
            int num = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
            Point3 result = default( Point3 );
            result.X = (int)(vector.X - 2f * normal.X * num);
            result.Y = (int)(vector.Y - 2f * normal.Y * num);
            result.Z = (int)(vector.Z - 2f * normal.Z * num);
            return result;
        }

        public static void Reflect(ref Point3 vector, ref Point3 normal, out Point3 result)
        {
            int num = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
            result.X = (int)(vector.X - 2f * normal.X * num);
            result.Y = (int)(vector.Y - 2f * normal.Y * num);
            result.Z = (int)(vector.Z - 2f * normal.Z * num);
        }

        public void Round()
        {
            X = (int)MathF.Round( X );
            Y = (int)MathF.Round( Y );
            Z = (int)MathF.Round( Z );
        }

        public static Point3 Round(Point3 value)
        {
            value.X = (int)MathF.Round( value.X );
            value.Y = (int)MathF.Round( value.Y );
            value.Z = (int)MathF.Round( value.Z );
            return value;
        }

        public static void Round(ref Point3 value, out Point3 result)
        {
            result.X = (int)MathF.Round( value.X );
            result.Y = (int)MathF.Round( value.Y );
            result.Z = (int)MathF.Round( value.Z );
        }

        public static Point3 SmoothStep(Point3 value1, Point3 value2, int amount)
        {
            return new Point3( MathHelper.SmoothStep( value1.X, value2.X, amount ), MathHelper.SmoothStep( value1.Y, value2.Y, amount ), MathHelper.SmoothStep( value1.Z, value2.Z, amount ) );
        }

        public static void SmoothStep(ref Point3 value1, ref Point3 value2, int amount, out Point3 result)
        {
            result.X = (int)MathHelper.SmoothStep( value1.X, value2.X, amount );
            result.Y = (int)MathHelper.SmoothStep( value1.Y, value2.Y, amount );
            result.Z = (int)MathHelper.SmoothStep( value1.Z, value2.Z, amount );
        }

        public static Point3 Subtract(Point3 value1, Point3 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        public static void Subtract(ref Point3 value1, ref Point3 value2, out Point3 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder( 32 );
            stringBuilder.Append( "{X:" );
            stringBuilder.Append( X );
            stringBuilder.Append( " Y:" );
            stringBuilder.Append( Y );
            stringBuilder.Append( " Z:" );
            stringBuilder.Append( Z );
            stringBuilder.Append( "}" );
            return stringBuilder.ToString();
        }

        public static Point3 Transform(Point3 position, Matrix matrix)
        {
            Transform( ref position, ref matrix, out position );
            return position;
        }

        public static void Transform(ref Point3 position, ref Matrix matrix, out Point3 result)
        {
            int x = (int)(position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41);
            int y = (int)(position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42);
            int z = (int)(position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43);
            result.X = x;
            result.Y = y;
            result.Z = z;
        }

        public static Point3 Transform(Point3 value, Quaternion rotation)
        {
            Transform( ref value, ref rotation, out var result );
            return result;
        }

        public static void Transform(ref Point3 value, ref Quaternion rotation, out Point3 result)
        {
            int num = (int)(2f * (rotation.Y * value.Z - rotation.Z * value.Y));
            int num2 = (int)(2f * (rotation.Z * value.X - rotation.X * value.Z));
            int num3 = (int)(2f * (rotation.X * value.Y - rotation.Y * value.X));
            result.X = (int)(value.X + num * rotation.W + (rotation.Y * num3 - rotation.Z * num2));
            result.Y = (int)(value.Y + num2 * rotation.W + (rotation.Z * num - rotation.X * num3));
            result.Z = (int)(value.Z + num3 * rotation.W + (rotation.X * num2 - rotation.Y * num));
        }

        public static void Transform(Point3[] sourceArray, int sourceIndex, ref Matrix matrix, Point3[] destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException( "sourceArray" );
            }

            if (destinationArray == null)
            {
                throw new ArgumentNullException( "destinationArray" );
            }

            if (sourceArray.Length < sourceIndex + length)
            {
                throw new ArgumentException( "Source array length is lesser than sourceIndex + length" );
            }

            if (destinationArray.Length < destinationIndex + length)
            {
                throw new ArgumentException( "Destination array length is lesser than destinationIndex + length" );
            }

            for (int i = 0; i < length; i++)
            {
                Point3 vector = sourceArray[sourceIndex + i];
                destinationArray[destinationIndex + i] = new Point3( vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41, vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42, vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + matrix.M43 );
            }
        }

        public static void Transform(Point3[] sourceArray, int sourceIndex, ref Quaternion rotation, Point3[] destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException( "sourceArray" );
            }

            if (destinationArray == null)
            {
                throw new ArgumentNullException( "destinationArray" );
            }

            if (sourceArray.Length < sourceIndex + length)
            {
                throw new ArgumentException( "Source array length is lesser than sourceIndex + length" );
            }

            if (destinationArray.Length < destinationIndex + length)
            {
                throw new ArgumentException( "Destination array length is lesser than destinationIndex + length" );
            }

            for (int i = 0; i < length; i++)
            {
                Point3 vector = sourceArray[sourceIndex + i];
                int num = (int)(2f * (rotation.Y * vector.Z - rotation.Z * vector.Y));
                int num2 = (int)(2f * (rotation.Z * vector.X - rotation.X * vector.Z));
                int num3 = (int)(2f * (rotation.X * vector.Y - rotation.Y * vector.X));
                destinationArray[destinationIndex + i] = new Point3( vector.X + num * rotation.W + (rotation.Y * num3 - rotation.Z * num2), vector.Y + num2 * rotation.W + (rotation.Z * num - rotation.X * num3), vector.Z + num3 * rotation.W + (rotation.X * num2 - rotation.Y * num) );
            }
        }

        public static void Transform(Point3[] sourceArray, ref Matrix matrix, Point3[] destinationArray)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException( "sourceArray" );
            }

            if (destinationArray == null)
            {
                throw new ArgumentNullException( "destinationArray" );
            }

            if (destinationArray.Length < sourceArray.Length)
            {
                throw new ArgumentException( "Destination array length is lesser than source array length" );
            }

            for (int i = 0; i < sourceArray.Length; i++)
            {
                Point3 vector = sourceArray[i];
                destinationArray[i] = new Point3( vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41, vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42, vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + matrix.M43 );
            }
        }

        public static void Transform(Point3[] sourceArray, ref Quaternion rotation, Point3[] destinationArray)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException( "sourceArray" );
            }

            if (destinationArray == null)
            {
                throw new ArgumentNullException( "destinationArray" );
            }

            if (destinationArray.Length < sourceArray.Length)
            {
                throw new ArgumentException( "Destination array length is lesser than source array length" );
            }

            for (int i = 0; i < sourceArray.Length; i++)
            {
                Point3 vector = sourceArray[i];
                int num = (int)(2f * (rotation.Y * vector.Z - rotation.Z * vector.Y));
                int num2 = (int)(2f * (rotation.Z * vector.X - rotation.X * vector.Z));
                int num3 = (int)(2f * (rotation.X * vector.Y - rotation.Y * vector.X));
                destinationArray[i] = new Point3( vector.X + num * rotation.W + (rotation.Y * num3 - rotation.Z * num2), vector.Y + num2 * rotation.W + (rotation.Z * num - rotation.X * num3), vector.Z + num3 * rotation.W + (rotation.X * num2 - rotation.Y * num) );
            }
        }

        public static Point3 TransformNormal(Point3 normal, Matrix matrix)
        {
            TransformNormal( ref normal, ref matrix, out normal );
            return normal;
        }

        public static void TransformNormal(ref Point3 normal, ref Matrix matrix, out Point3 result)
        {
            int x = (int)(normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31);
            int y = (int)(normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32);
            int z = (int)(normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33);
            result.X = x;
            result.Y = y;
            result.Z = z;
        }

        public static void TransformNormal(Point3[] sourceArray, int sourceIndex, ref Matrix matrix, Point3[] destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException( "sourceArray" );
            }

            if (destinationArray == null)
            {
                throw new ArgumentNullException( "destinationArray" );
            }

            if (sourceArray.Length < sourceIndex + length)
            {
                throw new ArgumentException( "Source array length is lesser than sourceIndex + length" );
            }

            if (destinationArray.Length < destinationIndex + length)
            {
                throw new ArgumentException( "Destination array length is lesser than destinationIndex + length" );
            }

            for (int i = 0; i < length; i++)
            {
                Point3 vector = sourceArray[sourceIndex + i];
                destinationArray[destinationIndex + i] = new Point3( vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31, vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32, vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 );
            }
        }

        public static void TransformNormal(Point3[] sourceArray, ref Matrix matrix, Point3[] destinationArray)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException( "sourceArray" );
            }

            if (destinationArray == null)
            {
                throw new ArgumentNullException( "destinationArray" );
            }

            if (destinationArray.Length < sourceArray.Length)
            {
                throw new ArgumentException( "Destination array length is lesser than source array length" );
            }

            for (int i = 0; i < sourceArray.Length; i++)
            {
                Point3 vector = sourceArray[i];
                destinationArray[i] = new Point3( vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31, vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32, vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 );
            }
        }

        public void Deconstruct(out int x, out int y, out int z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public static bool operator ==(Point3 value1, Point3 value2)
        {
            if (value1.X == value2.X && value1.Y == value2.Y)
            {
                return value1.Z == value2.Z;
            }

            return false;
        }

        public static bool operator !=(Point3 value1, Point3 value2)
        {
            return !(value1 == value2);
        }

        public static Point3 operator +(Point3 value1, Point3 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        public static Point3 operator -(Point3 value)
        {
            value = new Point3( 0 - value.X, 0 - value.Y, 0 - value.Z );
            return value;
        }

        public static Point3 operator -(Point3 value1, Point3 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        public static Point3 operator *(Point3 value1, Point3 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        public static Point3 operator *(Point3 value, int scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        public static Point3 operator *(int scaleFactor, Point3 value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        public static Point3 operator /(Point3 value1, Point3 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        public static Point3 operator /(Point3 value1, int divider)
        {
            int num = 1 / divider;
            value1.X *= num;
            value1.Y *= num;
            value1.Z *= num;
            return value1;
        }
    }
}
