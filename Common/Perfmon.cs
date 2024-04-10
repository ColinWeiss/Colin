using System.Diagnostics;

namespace Colin.Core.Common
{
  public static class Perfmon
  {
    public static Dictionary<string, string> Datas = new Dictionary<string, string>();
    public static Stopwatch Watch = new Stopwatch();
    private static bool _started = false;

    public static void Start()
    {
      if (_started)
        throw new InvalidOperationException();
      _started = true;
      Watch.Restart();
    }
    public static void End(string tag)
    {
      _started = false;
      Watch.Stop();
      Datas[tag] = Watch.ElapsedMilliseconds.ToString() + "ms";
    }
    public static void SetItem(string tag, object obj) => Datas[tag] = obj.ToString();
    public static string GetDatas()
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (var item in Datas)
      {
        stringBuilder.Append(item.Key)
            .Append(": ")
            .Append(item.Value)
            //.Append( "ms" )
            .Append('\n');
      }
      return stringBuilder.ToString();
    }
  }
}