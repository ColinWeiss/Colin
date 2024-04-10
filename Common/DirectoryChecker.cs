using Colin.Core.IO;
using System.Reflection;

namespace Colin.Core.Common
{
  internal sealed class DirectoryChecker : IProgramChecker
  {
    public void Check()
    {
      PropertyInfo[] properties = typeof(BasicsDirectory).GetProperties();
      foreach (PropertyInfo property in properties)
      {
        CheckDir((string)property.GetValue(null));
      }
    }
    public static void CheckDir(string path)
    {
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
    }
  }
}