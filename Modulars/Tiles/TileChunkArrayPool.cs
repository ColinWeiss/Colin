using System.Collections.Concurrent;
using System.Threading;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 区块大数组池.
  /// <br>Infos 和 Kernals 每块都在大对象堆上, 区块频繁装卸时反复分配会把堆搞碎, Gen2 一压缩就是十几毫秒的全场停顿.</br>
  /// <br>池子按数组长度分桶, 长度不同的区块各租各的, 互不串行.</br>
  /// <br>租借侧由 TileChunk.DoInitialize 独占调用, 归还侧只认异步存档落盘完成, 那之前后台存档线程还在读这两个数组.</br>
  /// <br>详细的使用规则和设计说明见同目录的 TileChunkArrayPool.md.</br>
  /// </summary>
  public static class TileChunkArrayPool
  {
    /// <summary>
    /// 每种长度最多缓存多少组数组.
    /// <br>兜住池子自身的内存上限, 超过上限的归还直接丢给 GC, 池子不背内存账.</br>
    /// </summary>
    public const int MaxPerLength = 64;

    private static readonly ConcurrentDictionary<int, ConcurrentBag<TileInfo[]>> _infos = new ConcurrentDictionary<int, ConcurrentBag<TileInfo[]>>();
    private static readonly ConcurrentDictionary<int, ConcurrentBag<TileKernel[]>> _kernals = new ConcurrentDictionary<int, ConcurrentBag<TileKernel[]>>();

    private static int _rentCount;
    private static int _returnCount;
    private static int _discardCount;

    /// <summary>累计租借次数, 观测用.</summary>
    public static int RentCount => _rentCount;
    /// <summary>累计归还入池次数, 观测用.</summary>
    public static int ReturnCount => _returnCount;
    /// <summary>累计因超出容量上限被丢弃的数组数, 观测用.</summary>
    public static int DiscardCount => _discardCount;

    /// <summary>
    /// 租一块 Infos 数组, 池空了就新分配.
    /// <br>这里不清零: DoInitialize 的 CreateInfo 会对每个格子做整结构体赋值, 陈旧数据活不过那一轮.</br>
    /// </summary>
    public static TileInfo[] RentInfos(int length)
    {
      Interlocked.Increment(ref _rentCount);
      if (length > 0 && _infos.TryGetValue(length, out ConcurrentBag<TileInfo[]> bag) && bag.TryTake(out TileInfo[] rented))
        return rented;
      return new TileInfo[length];
    }

    /// <summary>
    /// 租一块 Kernals 数组, 池空了就新分配.
    /// <br>这里必须清零: 读档和放置只会写有内容的格子, 陈旧的内核引用会变成看不见摸不着的幽灵物块.</br>
    /// </summary>
    public static TileKernel[] RentKernals(int length)
    {
      Interlocked.Increment(ref _rentCount);
      if (length > 0 && _kernals.TryGetValue(length, out ConcurrentBag<TileKernel[]> bag) && bag.TryTake(out TileKernel[] rented))
      {
        Array.Clear(rented, 0, rented.Length);
        return rented;
      }
      return new TileKernel[length];
    }

    /// <summary>
    /// 归还一组区块数组.
    /// <br>调用方必须保证从此刻起没有任何线程再读写这两个数组, 现在唯一的合法入口是异步存档落盘完成.</br>
    /// </summary>
    public static void Return(TileInfo[] infos, TileKernel[] kernals)
    {
      Interlocked.Increment(ref _returnCount);
      if (infos is not null)
      {
        ConcurrentBag<TileInfo[]> bag = _infos.GetOrAdd(infos.Length, static length => new ConcurrentBag<TileInfo[]>());
        if (bag.Count < MaxPerLength)
          bag.Add(infos);
        else
          Interlocked.Increment(ref _discardCount);
      }
      if (kernals is not null)
      {
        ConcurrentBag<TileKernel[]> bag = _kernals.GetOrAdd(kernals.Length, static length => new ConcurrentBag<TileKernel[]>());
        if (bag.Count < MaxPerLength)
          bag.Add(kernals);
        else
          Interlocked.Increment(ref _discardCount);
      }
    }
  }
}
