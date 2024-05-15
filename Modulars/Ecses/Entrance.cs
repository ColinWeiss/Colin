using System.Numerics;

namespace Colin.Core.Modulars.Ecses
{
  /// <summary>
  /// 表示一个词条.
  /// <br>不建议在其使用引用类型, 这样做可能在使用其功能时发生不可预知的后果.</br>
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public struct Entrance<T> where T : IEquatable<T>
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
    public static implicit operator Entrance<T>(T b) => new Entrance<T>(b);
  }
}