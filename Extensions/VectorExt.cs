namespace Colin.Core.Extensions
{
  /// <summary>
  /// <seealso cref="Vector2"/> 的扩展类.
  /// </summary>
  public static class VectorExt
  {
    public static Vector2 GetAbs(this Vector2 vector2)
    {
      return new Vector2(Math.Abs(vector2.X), Math.Abs(vector2.Y));
    }
    /// <summary>
    /// 获取向量旋转至一定的角度的值; 该角度计算单位使用弧度制.
    /// </summary>
    /// <param name="vec">要旋转的向量.</param>
    /// <param name="radian">要旋转的角度.</param>
    /// <returns></returns>
    public static Vector2 GetRotateTo(this Vector2 vec, float radian)
    {
      float l = vec.Length();
      return new Vector2((float)Math.Cos(radian) * l, (float)Math.Sin(radian) * l);
    }

    /// <summary>
    /// 以指定坐标为旋转中心, 获取向量旋转至一定的角度的值; 该角度计算单位使用弧度制.
    /// </summary>
    /// <param name="vec">要旋转的向量.</param>
    /// <param name="radian">要旋转的角度.</param>
    /// <param name="center">作为旋转中心的坐标.</param>
    /// <returns></returns>
    public static Vector2 GetRotateTo(this Vector2 vec, float radian, Vector2 center)
    {
      vec -= center;
      float l = vec.Length();
      return new Vector2((float)Math.Cos(radian) * l, (float)Math.Sin(radian) * l) + center;
    }
    /// <summary>
    /// 对向量进行线性插值.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="target"></param>
    /// <param name="i"></param>
    /// <param name="maxi"></param>
    /// <returns></returns>
    public static Vector2 Closer(this ref Vector2 current, Vector2 target, float i, float maxi)
    {
      float x = current.X;
      float y = current.Y;
      float tx = target.X;
      float ty = target.Y;
      x *= maxi - i;
      x /= maxi;
      y *= maxi - i;
      y /= maxi;
      tx *= i;
      tx /= maxi;
      ty *= i;
      ty /= maxi;
      current = new Vector2(x + tx, y + ty);
      return new Vector2(x + tx, y + ty);
    }
    public static Vector2 RotateTo(this Vector2 vec, float radian)
    {
      float l = vec.Length();
      return new Vector2((float)Math.Cos(radian) * l, (float)Math.Sin(radian) * l);
    }
    public static Vector2 RotateTo(this Vector2 vec, float radian, Vector2 center)
    {
      vec -= center;
      float l = vec.Length();
      return new Vector2((float)Math.Cos(radian) * l, (float)Math.Sin(radian) * l) + center;
    }
    public static Vector2 RotateBy(this Vector2 vec, float radian)
    {
      float c = MathF.Cos(radian);
      float s = MathF.Sin(radian);
      return new Vector2(c * vec.X - s * vec.Y, s * vec.X + c * vec.Y);
    }
    public static Vector2 RotateBy(this Vector2 vec, float radian, Vector2 center = default)
    {
      vec -= center;
      float c = (float)Math.Cos(radian);
      float s = (float)Math.Sin(radian);
      return new Vector2(c * vec.X - s * vec.Y, s * vec.X + c * vec.Y) + center;
    }

    public static Vector2 RtoA(this float rad)
    {
      return new Vector2((float)Math.Cos((double)rad), (float)Math.Sin((double)rad));
    }
    public static Vector2 RtoA(this int rad)
    {
      return new Vector2((float)Math.Cos(rad), (float)Math.Sin(rad));
    }
    public static float AtoR(this Vector2 v) => MathF.Atan2(v.Y, v.X);
    public static float DtoR(this float v)
    {
      Vector2 angle = new Vector2((float)Math.Cos(v * 3.1415926f / 180f), (float)Math.Sin(v * 3.1415926f / 180f));
      return MathF.Atan2(angle.Y, angle.X);
    }
    public static float DtoR(this int v)
    {
      Vector2 angle = new Vector2((float)Math.Cos(v * 3.1415926f / 180f), (float)Math.Sin(v * 3.1415926f / 180f));
      return MathF.Atan2(angle.Y, angle.X);
    }

    public static float Cross(Vector2 a, Vector2 b)
    {
      return a.X * b.Y - a.Y * b.X;
    }

    public static float Slop(this Vector2 vec)
    {
      return vec.Y / vec.X;
    }
    public static Vector2 LerpTo(this Vector2 a, Vector2 b, float progress, float max)
    {
      return progress / max * b + (max - progress) / max * a;
    }
    public static Vector2 FlipHorizontally(this Vector2 vec)
    {
      vec.X *= -1;
      return vec;
    }
    public static Vector2 FlipVertically(this Vector2 vec)
    {
      vec.Y *= -1;
      return vec;
    }
    public static Vector2 Floor(this Vector2 vec)
    {
      return new Vector2((float)Math.Floor(vec.X), (float)Math.Floor(vec.Y));
    }
    public static Vector2 Ceiling(this Vector2 vec)
    {
      return new Vector2((float)Math.Ceiling(vec.X), (float)Math.Ceiling(vec.Y));
    }
    public static float AngleBetween(this Vector2 a, Vector2 b)
    {
      return Math.Abs(a.AtoR() - b.AtoR());
    }
    public static Vector2 MutiplyXY(this Vector2 a, Vector2 b)
    {
      return new Vector2(a.X * b.X, a.Y * b.Y);
    }
    public static Vector2 SwapXY(this Vector2 a)
    {
      return new Vector2(a.Y, a.X);
    }
    public static Point ToPoint(this Vector2 vec)
    {
      return new Point((int)vec.X, (int)vec.Y);
    }
    public static Vector2 SafeNormalized(this Vector2 vec, Vector2 defaultValue = default)
    {
      return vec.X == 0 && vec.Y == 0 ? defaultValue : vec / vec.Length();
    }
    public static Vector2 GetNormalized(this Vector2 vec)
    {
      float l = (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
      return vec / l;
    }
    public static Vector2 GetVector2(this Vector3 vec)
    {
      return new Vector2(vec.X, vec.Y);
    }
    public static Vector3 GetNormalized(this Vector3 vec)
    {
      float l = (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z);
      return vec / l;
    }
    public static Color ToColor(this Vector3 vec, float a = 1)
    {
      return new Color(vec.X, vec.Y, vec.Z, a);
    }
    public static float Dot(this Vector3 vec1, Vector3 vec2)
    {
      return Vector3.Dot(vec1, vec2);
    }
    public static Vector3 XYZ(this Vector4 vec)
    {
      return new Vector3(vec.X, vec.Y, vec.Z);
    }

    public static Vector2 ToVector2(this Vector3 vec)
    {
      return new Vector2(vec.X, vec.Y);
    }
    public static Vector3 ToVector3(this Vector2 vec)
    {
      return new Vector3(vec, 0);
    }

    /// <summary>
    /// 创建带有指定索引处值的Vector4
    /// </summary>
    public static Vector4 WithElement(this Vector4 vector, int index, float value)
    {
      if (index < 0 || index > 3)
        throw new ArgumentOutOfRangeException(nameof(index), "索引必须在0-3之间");

      switch (index)
      {
        case 0: return new Vector4(value, vector.Y, vector.Z, vector.W);
        case 1: return new Vector4(vector.X, value, vector.Z, vector.W);
        case 2: return new Vector4(vector.X, vector.Y, value, vector.W);
        case 3: return new Vector4(vector.X, vector.Y, vector.Z, value);
        default: return vector;
      }
    }

    /// <summary>
    /// 获取Vector4指定索引处的值
    /// </summary>
    public static float GetElement(this Vector4 vector, int index)
    {
      if (index < 0 || index > 3)
        throw new ArgumentOutOfRangeException(nameof(index), "索引必须在0-3之间");

      switch (index)
      {
        case 0: return vector.X;
        case 1: return vector.Y;
        case 2: return vector.Z;
        case 3: return vector.W;
        default: return 0;
      }
    }

    /// <summary>
    /// 创建一个从数组或列表填充的Vector4
    /// </summary>
    public static Vector4 CreateFromArray(float[] values, int startIndex = 0)
    {
      if (values == null)
        throw new ArgumentNullException(nameof(values));

      return new Vector4(
          startIndex < values.Length ? values[startIndex] : 0,
          startIndex + 1 < values.Length ? values[startIndex + 1] : 0,
          startIndex + 2 < values.Length ? values[startIndex + 2] : 0,
          startIndex + 3 < values.Length ? values[startIndex + 3] : 0
      );
    }

    /// <summary>
    /// 创建一个从列表填充的Vector4
    /// </summary>
    public static Vector4 CreateFromList<T>(System.Collections.Generic.List<T> list,
        Func<T, float> selector, int startIndex = 0)
    {
      if (list == null)
        throw new ArgumentNullException(nameof(list));
      if (selector == null)
        throw new ArgumentNullException(nameof(selector));

      return new Vector4(
          startIndex < list.Count ? selector(list[startIndex]) : 0,
          startIndex + 1 < list.Count ? selector(list[startIndex + 1]) : 0,
          startIndex + 2 < list.Count ? selector(list[startIndex + 2]) : 0,
          startIndex + 3 < list.Count ? selector(list[startIndex + 3]) : 0
      );
    }

    /// <summary>
    /// 标准化Vector4，使所有分量之和为1
    /// </summary>
    public static Vector4 NormalizeWeights(this Vector4 weights)
    {
      float sum = weights.X + weights.Y + weights.Z + weights.W;
      if (sum > 0)
      {
        return new Vector4(
            weights.X / sum,
            weights.Y / sum,
            weights.Z / sum,
            weights.W / sum
        );
      }
      return Vector4.Zero;
    }

    /// <summary>
    /// 检查权重向量是否有效（所有分量在0-1之间，总和接近1）
    /// </summary>
    public static bool IsValidWeightVector(this Vector4 weights, float tolerance = 0.01f)
    {
      // 检查每个分量是否在合理范围内
      if (weights.X < 0 || weights.Y < 0 || weights.Z < 0 || weights.W < 0)
        return false;
      if (weights.X > 1 || weights.Y > 1 || weights.Z > 1 || weights.W > 1)
        return false;

      // 检查总和是否接近1
      float sum = weights.X + weights.Y + weights.Z + weights.W;
      return Math.Abs(sum - 1.0f) <= tolerance;
    }

    /// <summary>
    /// 将Vector4转换为数组
    /// </summary>
    public static float[] ToArray(this Vector4 vector)
    {
      return new float[] { vector.X, vector.Y, vector.Z, vector.W };
    }

    /// <summary>
    /// 检查Vector4是否为有效骨骼索引（所有分量>=0）
    /// </summary>
    public static bool IsValidBoneIndexVector(this Vector4 indices)
    {
      return indices.X >= 0 && indices.Y >= 0 && indices.Z >= 0 && indices.W >= 0;
    }

    /// <summary>
    /// 获取Vector4中最大的分量索引
    /// </summary>
    public static int MaxComponentIndex(this Vector4 vector)
    {
      float max = vector.X;
      int index = 0;

      if (vector.Y > max) { max = vector.Y; index = 1; }
      if (vector.Z > max) { max = vector.Z; index = 2; }
      if (vector.W > max) { max = vector.W; index = 3; }

      return index;
    }

    /// <summary>
    /// 获取Vector4中最大的分量值
    /// </summary>
    public static float MaxComponent(this Vector4 vector)
    {
      return Math.Max(Math.Max(vector.X, vector.Y), Math.Max(vector.Z, vector.W));
    }

    /// <summary>
    /// Vector3的类似扩展方法
    /// </summary>
    public static Vector3 WithElement(this Vector3 vector, int index, float value)
    {
      if (index < 0 || index > 2)
        throw new ArgumentOutOfRangeException(nameof(index), "索引必须在0-2之间");

      switch (index)
      {
        case 0: return new Vector3(value, vector.Y, vector.Z);
        case 1: return new Vector3(vector.X, value, vector.Z);
        case 2: return new Vector3(vector.X, vector.Y, value);
        default: return vector;
      }
    }

    public static float GetElement(this Vector3 vector, int index)
    {
      if (index < 0 || index > 2)
        throw new ArgumentOutOfRangeException(nameof(index), "索引必须在0-2之间");

      switch (index)
      {
        case 0: return vector.X;
        case 1: return vector.Y;
        case 2: return vector.Z;
        default: return 0;
      }
    }

    /// <summary>
    /// 将Vector3转换为Vector4（W分量设为0或指定值）
    /// </summary>
    public static Vector4 ToVector4(this Vector3 vector, float w = 0)
    {
      return new Vector4(vector.X, vector.Y, vector.Z, w);
    }

    /// <summary>
    /// 将Vector4转换为Vector3（丢弃W分量）
    /// </summary>
    public static Vector3 ToVector3(this Vector4 vector)
    {
      return new Vector3(vector.X, vector.Y, vector.Z);
    }
  }
}