using System.Threading;

namespace Colin.Core.IO
{
  public interface INBT : IOStep
  {
    private static int _countOfHandlerTypes = 0;
    public static class NBTIDHelper<T> where T : INBT
    {
      public static readonly int ID = Interlocked.Increment(ref _countOfHandlerTypes) - 1;
    }
  }
}