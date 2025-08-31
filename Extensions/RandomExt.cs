namespace Colin.Core.Extensions
{
  public static class RandomExt
  {
    private static int _seed = Environment.TickCount;
    public static Random Rand = new Random(_seed);

    public static bool NextBoolean() => Rand.NextBool();

    /// <summary>
    /// returns current seed value
    /// </summary>
    /// <returns>Seed.</returns>
    public static int GetSeed()
    {
      return _seed;
    }


    /// <summary>
    /// resets rng with new seed
    /// </summary>
    /// <param name="seed">Seed.</param>
    public static void SetSeed(int seed)
    {
      _seed = seed;
      Rand = new System.Random(_seed);
    }


    /// <summary>
    /// returns a random float between 0 (inclusive) and 1 (exclusive)
    /// </summary>
    /// <returns>The float.</returns>
    public static float NextFloat()
    {
      return (float)Rand.NextDouble();
    }


    /// <summary>
    /// returns a random float between 0 (inclusive) and max (exclusive)
    /// </summary>
    /// <returns>The float.</returns>
    /// <param name="max">Max.</param>
    public static float NextFloat(this Random rand, float max)
    {
      return (float)new Random().NextDouble() * max;
    }


    /// <summary>
    /// returns a random int between 0 (inclusive) and max (exclusive)
    /// </summary>
    /// <returns>The float.</returns>
    /// <param name="max">Max.</param>
    public static int NextInt(int max)
    {
      return Rand.Next(max);
    }


    /// <summary>
    /// returns a random float between 0 and 2 * PI
    /// </summary>
    /// <returns>The angle.</returns>
    public static float NextAngle()
    {
      return (float)Rand.NextDouble() * MathHelper.TwoPi;
    }


    /// <summary>
    /// Returns a random unit vector with direction between 0 and 2 * PI
    /// </summary>
    /// <returns></returns>
    public static Vector2 NextUnitVector()
    {
      float angle = NextAngle();
      return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }


    /// <summary>
    /// returns a random color
    /// </summary>
    /// <returns>The color.</returns>
    public static Color NextColor()
    {
      return new Color(NextFloat(), NextFloat(), NextFloat());
    }


    /// <summary>
    /// Returns a random integer between min (inclusive) and max (exclusive)
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int Range(int min, int max)
    {
      return Rand.Next(min, max);
    }


    /// <summary>
    /// Returns a random float between min (inclusive) and max (exclusive)
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float Range(this Random rand, float min, float max)
    {
      return min + rand.NextFloat(max - min);
    }


    /// <summary>
    /// Returns a random Vector2, and x- and y-values of which are between min (inclusive) and max (exclusive)
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static Vector2 Range(this Random rand, Vector2 min, Vector2 max)
    {
      return min + new Vector2(rand.NextFloat(max.X - min.X), rand.NextFloat(max.Y - min.Y));
    }


    /// <summary>
    /// returns a random float between -1 and 1
    /// </summary>
    /// <returns>The one to one.</returns>
    public static float MinusOneToOne(this Random rand)
    {
      return rand.NextFloat(2f) - 1f;
    }


    /// <summary>
    /// returns true if the next random is less than percent. Percent should be between 0 and 1
    /// </summary>
    /// <param name="percent">Percent.</param>
    public static bool Chance(float percent)
    {
      return NextFloat() < percent;
    }


    /// <summary>
    /// returns true if the next random is less than value. Value should be between 0 and 100.
    /// </summary>
    /// <param name="value">Value.</param>
    public static bool Chance(int value)
    {
      return NextInt(100) < value;
    }


    /// <summary>
    /// randomly returns one of the given values
    /// </summary>
    /// <param name="first">First.</param>
    /// <param name="second">Second.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static T Choose<T>(T first, T second)
    {
      if (NextInt(2) == 0)
        return first;

      return second;
    }


    /// <summary>
    /// randomly returns one of the given values
    /// </summary>
    /// <param name="first">First.</param>
    /// <param name="second">Second.</param>
    /// <param name="third">Third.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static T Choose<T>(T first, T second, T third)
    {
      switch (NextInt(3))
      {
        case 0:
          return first;
        case 1:
          return second;
        default:
          return third;
      }
    }


    /// <summary>
    /// randomly returns one of the given values
    /// </summary>
    /// <param name="first">First.</param>
    /// <param name="second">Second.</param>
    /// <param name="third">Third.</param>
    /// <param name="fourth">Fourth.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static T Choose<T>(T first, T second, T third, T fourth)
    {
      switch (NextInt(4))
      {
        case 0:
          return first;
        case 1:
          return second;
        case 2:
          return third;
        default:
          return fourth;
      }
    }

    public static string RandomString(int size = 38)
    {
      var builder = new StringBuilder();
      char ch;
      for (int i = 0; i < size; i++)
      {
        ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * NextFloat() + 65)));
        builder.Append(ch);
      }
      return builder.ToString();
    }

    /// <summary>
    /// swaps the two object types
    /// </summary>
    /// <param name="first">First.</param>
    /// <param name="second">Second.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static void Swap<T>(ref T first, ref T second)
    {
      T temp = first;
      first = second;
      second = temp;
    }

    public static T GetRandom<T>(this T[] array) => array[Rand.Next(0, array.Length)];

    public static bool NextBool(this Random rand)
    {
      return rand.Next(2) == 1;
    }
    public static Vector2 NextVectorUnit(this Random rand)
    {
      float rad = rand.Next(0, 360) * 3.141592f / 180;
      return rad.RtoA();
    }
    public static Vector2 NextVectorRec(this Random rand, Point size)
    {
      float x = (float)rand.NextDouble() * size.X;
      float y = (float)rand.NextDouble() * size.Y;
      return new Vector2(x, y);
    }
    public static Vector2 NextVectorRec(this Random rand, Rectangle rect)
    {
      return rect.Location.ToVector2() + rand.NextVectorRec(rect.Size);
    }
  }
}