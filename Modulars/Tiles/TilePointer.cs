using Colin.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块指针结构体.
  /// </summary>
  public struct TilePointer : IEquatable<TilePointer>, IEquatable<Point3>, IComparable<TilePointer>
  {
    /// <summary>
    /// 用于排序的顺序值.
    /// </summary>
    public int Sort;

    /// <summary>
    /// 指示该指针于对应格集合内的索引.
    /// </summary>
    public int Index;

    public int ToX;

    public int ToY;

    public int ToZ;

    public Point3 PointTo => new Point3(ToX, ToY, ToZ);

    public bool IsPointer => ToX != 0 || ToY != 0 || ToZ != 0;

    public void SetPointTo(Point3 target)
    {
      ToX = target.X;
      ToY = target.Y;
      ToZ = target.Z;
    }

    public int CompareTo(TilePointer other)
    {
      if(Sort != other.Sort)
        return Sort.CompareTo(other.Sort);
      else
        return Index.CompareTo(other.Index);
    }

    public bool Equals(TilePointer other)
    {
      return other.PointTo == PointTo;
    }

    public bool Equals(Point3 other)
    {
      return other == PointTo;
    }
  }
}