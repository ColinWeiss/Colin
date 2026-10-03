using Colin.Core.Common.Debugs;
using Colin.Core.IO;
using Colin.Core.Resources;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 区块遍历委托.
  /// </summary>
  public delegate void TileChunkForEachDelegate(ref TileInfo info);

  /// <summary>
  /// 物块区块类.
  /// </summary>
  public class TileChunk : IOStep
  {
    /// <summary>
    /// 获取物块区块所属的世界物块模块.
    /// </summary>
    public readonly Tile Tile;

    /// <summary>
    /// 获取物块模块的物块建造器.
    /// </summary>
    public readonly TileBuilder Builder;

    /// <summary>
    /// 获取物块模块的物块刷新模块.
    /// </summary>
    public readonly TileRefresher Refresher;

    /// <summary>
    /// 获取区块的深度.
    /// </summary>
    public readonly int Depth;

    /// <summary>
    /// 获取区块的宽度.
    /// </summary>
    public int Width => Tile.Context.ChunkWidth;

    /// <summary>
    /// 获取区块的宽度.
    /// </summary>
    public int Height => Tile.Context.ChunkHeight;

    private int _coordX;
    /// <summary>
    /// 指示区块的横坐标.
    /// </summary>
    public int CoordX => _coordX;

    private int _coordY;
    /// <summary>
    /// 指示区块的纵坐标.
    /// </summary>
    public int CoordY => _coordY;

    private Point _coord;
    /// <summary>
    /// 获取区块坐标.
    /// </summary>
    public Point Coord => _coord;

    private Rectangle? _bounds;
    /// <summary>
    /// 获取区块包围盒.
    /// </summary>
    public Rectangle Bounds =>
      _bounds ??=
      new Rectangle(
        CoordX * Tile.Context.ChunkWidth,
        CoordY * Tile.Context.ChunkHeight,
        Tile.Context.ChunkWidth,
        Tile.Context.ChunkHeight);

    private Rectangle? _realBounds;
    public Rectangle RealBounds =>
      _realBounds ??=
      new Rectangle(
        CoordX * Tile.Context.ChunkWidth * Tile.Context.TileSize.X,
        CoordY * Tile.Context.ChunkHeight * Tile.Context.TileSize.Y,
        Tile.Context.ChunkWidth * Tile.Context.TileSize.X,
        Tile.Context.ChunkHeight * Tile.Context.TileSize.Y);

    /// <summary>
    /// 区块内的物块信息.
    /// </summary>
    public TileInfo[] Infos;

    /// <summary>
    /// 区块内的物块内核.
    /// </summary>
    public TileKernel[] Kernals;

    /// <summary>
    /// 区块内的物块可编程行为.
    /// </summary>
    public List<TileHandler> Handler = new();

    /// <summary>
    /// 为区块添加指定类型的物块处理方式.
    /// </summary>
    public T AddHandler<T>() where T : TileHandler, new()
    {
      T handler = new T();
      AddHandler(handler);
      return handler;
    }

    public void AddHandler<T>(T handler) where T : TileHandler
    {
      handler.Tile = Tile;
      handler.Chunk = this;
      handler.Enable = new bool[handler.Length];
      int id = TileHandler.HandlerIDHelper<T>.HandlerID;
      if (id >= Handler.Count)
        Handler.AddRange(Enumerable.Repeat<TileHandler>(null, id - Handler.Count + 1));
      Handler[id] = handler;
    }

    /// <summary>
    /// 获取指定类型的物块处理方式.
    /// </summary>
    public T GetHandler<T>() where T : TileHandler
    {
      return Handler[TileHandler.HandlerIDHelper<T>.HandlerID] as T;
    }

    /// <summary>
    /// 索引器: 根据索引获取物块信息.
    /// </summary>
    public ref TileInfo this[int index]
      => ref Infos[index];

    /// <summary>
    /// 索引器: 根据索引获取物块信息.
    /// </summary>
    public ref TileInfo this[int x, int y, int z]
      => ref Infos[z * Width * Height + x + y * Width];

    /// <summary>
    /// 索引器: 根据索引获取物块信息.
    /// </summary>
    public ref TileInfo this[Point3 coord]
      => ref this[coord.X, coord.Y, coord.Z];

    /// <summary>
    /// 以坐标转换至索引.
    /// </summary>
    public int GetIndex(int x, int y, int z)
      => z * Width * Height + x + y * Width;

    /// <summary>
    /// 以坐标转换至索引.
    /// </summary>
    public int GetIndex(Point3 cCoord)
      => GetIndex(cCoord.X, cCoord.Y, cCoord.Z);

    public ref TileInfo GetRelative(int index, TileRelative relative)
    {
      ref TileInfo info = ref this[index];
      Point3 temp = Point3.Zero;
      switch (relative)
      {
        case TileRelative.Left:
          temp = Point3.Left;
          break;
        case TileRelative.Right:
          temp = Point3.Right;
          break;
        case TileRelative.Up:
          temp = Point3.Up;
          break;
        case TileRelative.Down:
          temp = Point3.Down;
          break;
        case TileRelative.Front:
          temp = Point3.Front;
          break;
        case TileRelative.Behind:
          temp = Point3.Behind;
          break;
      }
      temp = info.GetWCoord3() + temp;
      return ref Tile[temp];
    }

    public TileChunk(Tile tile, Point coord)
    {
      Tile = tile;
      Builder = tile.Scene.Business.Get<TileBuilder>();
      Refresher = tile.Scene.Business.Get<TileRefresher>();
      Depth = tile.Context.Depth;
      _coordX = coord.X;
      _coordY = coord.Y;
      _coord = coord;
    }

    public void ForEach(TileChunkForEachDelegate info, int x, int y, int width, int height, int depth)
    {
      ref TileInfo tileInfo = ref Infos[0];
      try
      {
        for (int cx = x; cx < x + width; cx++)
          for (int cy = y; cy < y + height; cy++)
          {
            tileInfo = ref Infos[GetIndex(cx, cy, depth)];
            info.Invoke(ref tileInfo);
          }
      }
      catch
      {
        Console.Log(ConsoleTextType.Error, "TileChunk", "区块遍历异常, 请检查输入参数合理性.");
      }
    }

    /// <summary>
    /// 执行初始化操作.
    /// <br>该操作会令区块初始化其整个物块信息的数组.</br>
    /// </summary>
    public void DoInitialize()
    {
      int length = Width * Height * Depth;
      // 大数组从池里租, 区块反复装卸不再反复喂大对象堆
      Infos = TileChunkArrayPool.RentInfos(length);
      Kernals = TileChunkArrayPool.RentKernals(length);
      Handler = new List<TileHandler>();
      for (int count = 0; count < length; count++)
        CreateInfo(count);
      Tile.Context.DoTileHandleInit(this);
      foreach (var item in Handler)
        item.DoInitialize();
    }

    public void CreateInfo(int index)
    {
      Infos[index] = new TileInfo();
      Infos[index].Empty = true;
      Infos[index].Index = index;
      Infos[index].ICoordX = (short)(index % (Tile.Context.ChunkWidth * Tile.Context.ChunkHeight) % Tile.Context.ChunkWidth);
      Infos[index].ICoordY = (short)(index % (Tile.Context.ChunkWidth * Tile.Context.ChunkHeight) / Tile.Context.ChunkWidth);
      Infos[index].ICoordZ = (short)(index / (Tile.Context.ChunkWidth * Tile.Context.ChunkHeight));
      Infos[index].WCoordX = CoordX * Tile.Context.ChunkWidth + Infos[index].ICoordX;
      Infos[index].WCoordY = CoordY * Tile.Context.ChunkHeight + Infos[index].ICoordY;
    }



    public Point MouseChunk
    {
      get
      {
        var mw = Tile.Scene.Camera.ConvertToWorld(MouseResponder.Position);
        var mouseT = (mw / Tile.Context.TileSizeF).ToPoint();
        return mouseT - Bounds.Location;
      }
    }

    /// <summary>
    /// 从区块内指定坐标转换至世界坐标.
    /// </summary>
    public Point3 ConvertWorld(Point3 iCoord)
    {
      Point3 result = new Point3();
      result.X = CoordX * Tile.Context.ChunkWidth + iCoord.X;
      result.Y = CoordY * Tile.Context.ChunkHeight + iCoord.Y;
      result.Z = iCoord.Z;
      return result;
    }

    /// <summary>
    /// 从区块内指定索引转换至世界坐标.
    /// </summary>
    public Point3 ConvertWorld(int index)
    {
      return Infos[index].GetWCoord3();
    }

    /// <summary>
    /// 判断指定世界坐标是否处于该区块内.
    /// </summary>
    public bool InChunk(Point wCoord)
    {
      int compX = wCoord.X >= 0 ? wCoord.X / Tile.Context.ChunkWidth : (wCoord.X + 1) / Tile.Context.ChunkWidth - 1;
      int compY = wCoord.Y >= 0 ? wCoord.Y / Tile.Context.ChunkHeight : (wCoord.Y + 1) / Tile.Context.ChunkHeight - 1;
      return Coord.Equals(new Point(compX, compY));
    }

    /// <summary>
    /// 判断指定世界坐标是否处于该区块内.
    /// </summary>
    public bool InChunk(Point3 wCoord)
      => InChunk(wCoord.ToPoint());

    /// <summary>
    /// 根据指定坐标和指定类型放置物块.
    /// <br>[!] 使用内部坐标.</br>
    /// </summary>
    public bool Place(TileKernel kernel, int x, int y, int z, bool doEvent = true, int? doRefresh = 1)
    {
      Point3 wCoord = ConvertWorld(new Point3(x, y, z));
      bool result = true;
      foreach (var handler in Handler)
      {
        if (result)
          result = handler.CanPlaceMark(GetIndex(x, y, z), wCoord);
        else
          return result;
      }
      if (kernel.CanPlaceMark(Tile, this, GetIndex(x, y, z), wCoord))
      {
        Builder.MarkPlace(wCoord, kernel, doEvent, doRefresh);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// 根据区块内坐标和指定类型放置物块.
    /// [!] 使用内部坐标.
    /// </summary>
    public bool Place<T>(int x, int y, int z, bool doEvent = true, int? doRefresh = 1) where T : TileKernel, new()
    {
      TileKernel behavior = CodeResources<TileKernel>.GetFromType(typeof(T));
      return Place(behavior, x, y, z, doEvent, doRefresh);
    }

    /// <summary>
    /// 破坏指定坐标的物块.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void Destruct(int x, int y, int z, bool doEvent = true, int? doRefresh = 1)
    {
      ref TileInfo info = ref this[x, y, z];
      if (info.IsNull)
        return;
      TileKernel comport = Kernals[GetIndex(x, y, z)];
      if (Tile.HasPointer(info.GetWCoord3()))
      {
        info = Tile.GetPointTo(info.GetWCoord3());
        if (info.Empty is false && !info.IsNull)
          Builder.MarkDestruct(info.GetWCoord3(), doEvent, doRefresh);
      }
      else if (!Builder.Cases.Select(a => (a as TileBuildCommand).WorldCoord).Contains(info.GetWCoord3()))
      {
        if (!info.Empty && !info.IsNull)
          Builder.MarkDestruct(info.GetWCoord3(), doEvent, doRefresh);
      }
    }

    internal bool _saving = false;
    internal bool _loading = false;
    internal bool _operation = false;
    public bool InOperation => _operation || _loading || _saving;
    /// <summary>
    /// 区块数据尚未就位 (生成中或读档中).
    /// <br>此期间格子内容是后台线程正在写入的半成品, 碰撞等主线程消费方应把它整体当实心占位.</br>
    /// <br>不包含 <see cref="_saving"/>: 存档中的区块数据是完整的, 照常参与碰撞.</br>
    /// </summary>
    public bool AwaitingData => _operation || _loading;
    public void SetOperation(bool flag)
    {
      _operation = flag;
    }

    /// <summary>
    /// 区块数据就位后的主线程收尾.
    /// <br>后台读档和生成期间各 Handler 欠下的活儿在这里补, 比如结构物块的指针重建.</br>
    /// </summary>
    public void DoChunkReady()
    {
      for (int i = 0; i < Handler.Count; i++)
        Handler[i].OnChunkReady();
    }

    public void AsyncLoadChunk(string path)
    {
      PrepareLoading();
      Task.Run(() =>
      {
        Exception loadError = null;
        try
        {
          DataIO.DoLoad(path, this, true);
        }
        catch (Exception exception)
        {
          loadError = exception;
        }
        // 反序列化在后台做完就行, 收尾的刷新和状态复位回主线程排队执行, 不和游戏逻辑抢数据
        // 刷新走标记队列而不是立刻刷完, 让刷新器按时间预算把工作量摊到后面几帧
        Tile.Scene.Business.MarkMainThreadJob(() =>
        {
          using (StageRecorder.Tag("Chunk.LoadCompletion"))
            CompleteLoad(path, loadError);
        });
      });
    }

    /// <summary>
    /// 把区块置为加载中状态: 格子标 Loading, 碰撞系统会把它当实心处理, 玩家不会踩空.
    /// <br>后台物化流程在把区块发布进 Chunks 之前调用, 保证世界看到的第一帧就是实心的.</br>
    /// </summary>
    public void PrepareLoading()
    {
      _loading = true;
      SetOperation(true);
    }

    /// <summary>
    /// 在当前(后台)线程上执行读档, 并把收尾作业排回主线程.
    /// <br>与 <see cref="AsyncLoadChunk"/> 的差别: 不再自起 Task, 供已经在后台线程上的物化流程复用.</br>
    /// <br>调用前需先 <see cref="PrepareLoading"/>, 调用方负责把区块发布进 Tile.Chunks.</br>
    /// </summary>
    public void LoadOffThread(string path)
    {
      Exception loadError = null;
      try
      {
        DataIO.DoLoad(path, this, true);
      }
      catch (Exception exception)
      {
        loadError = exception;
      }
      Tile.Scene.Business.MarkMainThreadJob(() =>
      {
        using (StageRecorder.Tag("Chunk.LoadCompletion"))
          CompleteLoad(path, loadError);
      });
    }

    /// <summary>
    /// 读档完成的主线程收尾: 坏档兜底重建、状态复位、补 Handler 欠账、整块标刷.
    /// </summary>
    private void CompleteLoad(string path, Exception loadError)
    {
      if (loadError is not null)
      {
        // 存档文件截断或损坏, 多半是上次闪退掐死了正在写的存档
        // 把区块重建成空的兜底, 别让半个文件把加载流程卡死, 日志里会留下具体是哪个文件
        Console.Log(ConsoleTextType.Error, "TileChunk", string.Concat("区块文件读取失败, 已按空区块重建: ", path, ", 原因: ", loadError.Message));
        for (int count = 0; count < Infos.Length; count++)
          CreateInfo(count);
        Array.Clear(Kernals, 0, Kernals.Length);
      }
      SetOperation(false);
      _loading = false;
      DoChunkReady();
      MarkRefreshAll();
    }

    public void LoadChunk(string path)
    {
      _loading = true;
      DataIO.DoLoad(path, this); //同步执行, 不使用 await.
      MarkRefreshAll();
      _loading = false;
      DoChunkReady();
    }

    // 全量刷新只刷有内容的格子, 空格子没挂行为, 刷了也是空转, 还占队列
    // 直接拿本区块的刷新队列一次入队, 逐格走 MarkRefresh 的字典查找太浪费
    public void MarkRefreshAll()
    {
      ConcurrentQueue<Point3> queue = Refresher.RefreshQueue.GetOrAdd(Coord, static _ => new ConcurrentQueue<Point3>());
      ref TileInfo info = ref this[0, 0, 0];
      for (int count = 0; count < Infos.Length; count++)
      {
        info = ref this[count];
        if (info.Empty)
          continue;
        queue.Enqueue(new Point3(info.ICoordX, info.ICoordY, info.ICoordZ));
      }
      MarkNeighborEdgeRefresh();
    }

    /// <summary>
    /// 把四邻八向已有区块的贴边格子标进刷新队列.
    /// <br>边框连接靠 IsSame 跨区块查邻居, 邻居晚到时旧边的边框是按空画的, 邻居就位后必须让人家重画一遍.</br>
    /// <br>四个正方向刷整条贴边, 四个对角刷角上一格, 不刷的话区块接缝处的边框会一直错着.</br>
    /// </summary>
    public void MarkNeighborEdgeRefresh()
    {
      int lastX = Tile.Context.ChunkWidth - 1;
      int lastY = Tile.Context.ChunkHeight - 1;
      for (int dx = -1; dx <= 1; dx++)
      {
        for (int dy = -1; dy <= 1; dy++)
        {
          if (dx == 0 && dy == 0)
            continue;
          TileChunk neighbor = Tile.GetChunk(Coord.X + dx, Coord.Y + dy);
          if (neighbor is null || neighbor.InOperation)
            continue;
          // 邻居还在加载的话不用管, 它自己就位时也会走这里把我们的贴边补刷
          ConcurrentQueue<Point3> queue = Refresher.RefreshQueue.GetOrAdd(neighbor.Coord, static _ => new ConcurrentQueue<Point3>());
          // 正方向只需要一条贴边, 对角只需要一格; 直接按坐标取, 不再全扫 18432 格再按 onEdge 过滤
          if (dx == 0)
          {
            int iy = dy == 1 ? 0 : lastY;
            for (int ix = 0; ix <= lastX; ix++)
              for (int z = 0; z < Depth; z++)
              {
                ref TileInfo info = ref neighbor[ix, iy, z];
                if (info.Empty)
                  continue;
                queue.Enqueue(new Point3(ix, iy, info.ICoordZ));
              }
          }
          else if (dy == 0)
          {
            int ix = dx == 1 ? 0 : lastX;
            for (int iy = 0; iy <= lastY; iy++)
              for (int z = 0; z < Depth; z++)
              {
                ref TileInfo info = ref neighbor[ix, iy, z];
                if (info.Empty)
                  continue;
                queue.Enqueue(new Point3(ix, iy, info.ICoordZ));
              }
          }
          else
          {
            int ix = dx == 1 ? 0 : lastX;
            int iy = dy == 1 ? 0 : lastY;
            for (int z = 0; z < Depth; z++)
            {
              ref TileInfo info = ref neighbor[ix, iy, z];
              if (info.Empty)
                continue;
              queue.Enqueue(new Point3(ix, iy, info.ICoordZ));
            }
          }
        }
      }
    }

    public void DoRefreshAll()
    {
      ref TileInfo info = ref this[0, 0, 0];
      for (int count = 0; count < Infos.Length; count++)
      {
        info = ref this[count];
        if (info.Empty)
          continue;
        Refresher.DoRefresh(this, count, info.GetWCoord3());
      }
    }

    public void LoadStep(TagCompound data)
    {
      int length = Infos.Length;
      //大数组先整段收下来, 后面逐格只做下标和赋值
      int[] collisions = data.GetArray<int>("Collision");
      TagSeq nameList = data.GetSeq("KernelNames");
      int[] slots = data.GetArray<int>("KernelSlots");
      string[] nameTable = new string[nameList?.Count ?? 0];
      for (int i = 0; i < nameTable.Length; i++)
        nameTable[i] = nameList.GetString(i, string.Empty);
      Array.Clear(Kernals, 0, Kernals.Length); //租来的数组可能有上一位区块的旧账, 先清干净
      int repairedTiles = 0;
      string firstBadName = null;
      for (int count = 0; count < length; count++)
      {
        ref TileInfo info = ref this[count];
        //位置类字段跟生成时一样按索引现算, 档里只存真状态, 不存推得出来的冗余
        info.Index = count;
        info.ICoordX = (short)(count % (Tile.Context.ChunkWidth * Tile.Context.ChunkHeight) % Tile.Context.ChunkWidth);
        info.ICoordY = (short)(count % (Tile.Context.ChunkWidth * Tile.Context.ChunkHeight) / Tile.Context.ChunkWidth);
        info.ICoordZ = (short)(count / (Tile.Context.ChunkWidth * Tile.Context.ChunkHeight));
        info.WCoordX = CoordX * Tile.Context.ChunkWidth + info.ICoordX;
        info.WCoordY = CoordY * Tile.Context.ChunkHeight + info.ICoordY;
        info.Collision = collisions is not null && count < collisions.Length ? (TileSolid)collisions[count] : default;
        //空档约定: 行为槽位为负就是空格子, Empty 不再单独落盘
        int slot = slots is not null && count < slots.Length ? slots[count] : -1;
        info.Empty = slot < 0;
        if (slot >= 0)
        {
          string typeName = slot < nameTable.Length ? nameTable[slot] : null;
          TileKernel kernel = typeName is null ? null : CodeResources<TileKernel>.GetFromTypeName(typeName);
          if (kernel is not null)
          {
            Kernals[count] = kernel;
            kernel.Tile = Tile;
            kernel.OnInitialize(Tile, this, info.Index); //执行行为初始化放置
          }
          else
          {
            // 行为名在注册表里对不上号, 一般是存档带着已经删除或改名的物块类型
            // 只能把格子按空的修复, 不然这区块以后存档的时候必然炸
            firstBadName ??= typeName ?? "?";
            repairedTiles++;
          }
        }
      }
      if (repairedTiles > 0)
        Console.Log(ConsoleTextType.Error, "TileChunk", string.Concat("区块(", CoordX, ",", CoordY, ")有 ", repairedTiles, " 个格子的物块行为已失效, 已按空格子修复, 未知行为: ", firstBadName));
      // Handler 各占一个键(键名是 Handler 类名): 档里有的才读, 档里没有的(新加的 Handler)保持默认,
      // 档里多出来的键(被删掉的 Handler)没人读就自然跳过, 都不会像以前那样校验失败当场炸
      TagCompound handlers = data.GetCompound("Handlers");
      for (int i = 0; i < Handler.Count; i++)
      {
        TagCompound handlerData = handlers?.GetCompound(Handler[i].GetType().Name);
        if (handlerData is not null)
          Handler[i].LoadStep(handlerData);
      }
    }

    public void AsyncSaveChunk(string path)
    {
      if (Infos is null || _saving)
        return;
      _saving = true;
      Task.Run(() =>
      {
        try
        {
          DataIO.DoSave(path, this, true);
          // 先还池再清标志, 中间不留窗口: 还池之后 Infos 已是 null, 走 Infos 的守卫
          // 若先清标志再还池, 主线程会在两步之间看到 可存档 状态, 又叠出一个空瓦片档
          ReleaseArrays();
        }
        catch (Exception exception)
        {
          // 存档失败(典型是文件被上一轮任务占用): 数组留在原地不还池, 宁可漏存不可写坏
          _saving = false;
          Console.Log(ConsoleTextType.Error, "TileChunk", string.Concat("区块存档失败, 本区块保持未卸载状态: ", path, ", 原因: ", exception.Message));
          return;
        }
        _saving = false;
      });
    }

    /// <summary>
    /// 把区块的两块大数组归还进数组池, 然后把字段置空.
    /// <br>只能在确定没有任何线程再读写这块区块之后调用, 现在唯一的合法入口是异步存档落盘完成.</br>
    /// <br>置空是故意的: 卸载之后再有人碰这块区块会当场空引用炸出来, 好过拿着陈旧数组悄悄出错.</br>
    /// </summary>
    private void ReleaseArrays()
    {
      TileInfo[] infos = Infos;
      TileKernel[] kernals = Kernals;
      if (infos is null && kernals is null)
        return;
      Infos = null;
      Kernals = null;
      TileChunkArrayPool.Return(infos, kernals);
    }

    public void SaveChunk(string path)
    {
      _saving = true;
      DataIO.DoSave(path, this);
      _saving = false;
    }

    public void SaveStep(TagCompound data)
    {
      Span<TileInfo> infoSpan = Infos;
      int length = infoSpan.Length;
      //逐格数据全部摊成批量数组整存整取: 一格一组键值对这种事在物块这里是想都不要想的
      int[] collisions = new int[length];
      int[] slots = new int[length];
      Dictionary<string, int> nameIds = new Dictionary<string, int>();
      List<string> nameList = new List<string>();
      TileKernel tCom;
      int repairedTiles = 0;
      for (int count = 0; count < length; count++)
      {
        tCom = Kernals[count];
        if (infoSpan[count].Empty is false && tCom is null)
        {
          // 格子有内容但行为缺失, 只能按空格子落盘, 不然存档中途就炸, 格子内容反正也读不回来
          TileInfo repaired = infoSpan[count];
          repaired.Empty = true;
          infoSpan[count] = repaired;
          repairedTiles++;
        }
        collisions[count] = (int)infoSpan[count].Collision;
        if (infoSpan[count].Empty is false)
        {
          // 行为类型直接存类型名, 谁先出现谁占号, 整份档里每种行为只记一份全文
          string name = tCom.Identifier;
          if (nameIds.TryGetValue(name, out int id) is false)
          {
            id = nameList.Count;
            nameIds[name] = id;
            nameList.Add(name);
          }
          slots[count] = id;
        }
        else
        {
          // 空档约定: 槽位负数就是空格子, Empty 不再单独落盘
          slots[count] = -1;
        }
      }
      if (repairedTiles > 0)
        Console.Log(ConsoleTextType.Error, "TileChunk", string.Concat("区块(", CoordX, ",", CoordY, ")有 ", repairedTiles, " 个格子的行为缺失, 已按空格子写入存档"));
      data["Collision"] = collisions;
      data["KernelSlots"] = slots;
      TagSeq nameTags = new TagSeq(TagType.String);
      for (int i = 0; i < nameList.Count; i++)
        nameTags.Add(nameList[i]);
      data["KernelNames"] = nameTags;
      // Handler 各占一个键(键名是 Handler 类名), 谁的数据谁自己往里塞
      TagCompound handlers = new TagCompound();
      for (int i = 0; i < Handler.Count; i++)
      {
        TileHandler handler = Handler[i];
        TagCompound handlerData = new TagCompound();
        handler.SaveStep(handlerData);
        handlers[handler.GetType().Name] = handlerData;
      }
      data["Handlers"] = handlers;
      //2025.2.22 的设计沿用: 区块行为与物块本身行为区分, 空物块也能在 Handler 里有数据.
    }

    /// <summary>
    /// 判断同层指定坐标的物块行为与具有指定偏移位置处的物块行为是否相同.
    /// </summary>
    public bool IsSame(Point3 own, Point3 offset)
    {
      var ownCom = Kernals[GetIndex(own)];
      Point3 tarCoord = ConvertWorld(own + offset);
      if (InChunk(tarCoord))
      {
        var tarCom = Kernals[GetIndex(own + offset)];
        if (ownCom is null || tarCom is null)
          return false;
        else
          return ownCom.Equals(tarCom);
      }
      else
      {
        var tarCom = Tile.GetHandler(tarCoord);
        if (ownCom is null || tarCom is null)
          return false;
        else
          return ownCom.Equals(tarCom);
      }
    }

    public int GetSeed()
    {
      return CoordX * 137 + CoordY;
    }
  }
}