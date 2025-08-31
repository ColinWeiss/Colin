using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Extensions
{
  public static class ArrayExt
  {
    public static void ForEach<T>(this T[] array, Action<T> action)
    {
      for (int i = 0; i < array.Length; i++)
        action(array[i]);
    }
  }
}