namespace Colin.Core.Extensions
{
  public static class StringExt
  {
    extension(object obj)
    {
      public int MsnHash => obj.GetType().GetMsnHashCode();
    }

    extension(Type type)
    {
      public int MsnHash => type.FullName.GetMsnHashCode();
    }

    public static int GetMsnHashCode(this Type type)
    {
      return type.FullName.GetMsnHashCode();
    }

    public static int GetMsnHashCode(this string str)
    {
      int result = 0;
      int len = str.Length;
      for (int i = 0; i < len; i++)
        result += (result + str[i] * 3737 % 1000000007) % 1000000007;
      return result;
    }
  }
}