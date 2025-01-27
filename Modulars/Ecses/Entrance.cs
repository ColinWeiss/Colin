using System.Numerics;

namespace Colin.Core.Modulars.Ecses
{
  /// <summary>
  /// 表示一个词条.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public struct Entrance<T> where T : IEquatable<T>, INumber<T>
  {
    /// <summary>
    /// 当前值.
    /// </summary>
    public T Value;
    /// <summary>
    /// 默认值.
    /// </summary>
    public T Default;
    public Entrance(T defaultValue)
    {
      Value = defaultValue;
      Default = defaultValue;
    }
    public void Reset()
    {
      Value = Default;
    }
    public void SetDefault(T defaultT) => Default = defaultT;
    public static implicit operator T(Entrance<T> d) => d.Value;
    public static Entrance<T> operator +(Entrance<T> d, T b)
    {
      d.Value += b;
      return d;
    }
    public static Entrance<T> operator -(Entrance<T> d, T b)
    {
      d.Value -= b;
      return d;
    }
    public static Entrance<T> operator *(Entrance<T> d, T b)
    {
      d.Value *= b;
      return d;
    }
    public static Entrance<T> operator /(Entrance<T> d, T b)
    {
      d.Value /= b;
      return d;
    }
    public static implicit operator Entrance<T>(T b) => new Entrance<T>(b);
  }
}