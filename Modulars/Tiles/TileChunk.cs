using Colin.Core.Resources;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms.Design.Behavior;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 区块遍历委托.
  /// </summary>
  public delegate void TileChunkForEachDelegate(ref TileInfo info);

  /// <summary>
  /// 物块区块类.
  /// </summary>
  public class TileChunk
  {
    /// <summary>
    /// 获取物块区块所属的世界物块模块.
    /// </summary>
    public readonly Tile Tile;

    /// <summary>
    /// 获取物块区块所属的物块放置模块.
    /// </summary>
    public readonly TilePlacer Placer;

    /// <summary>
    /// 获取物块区块所属的物块破坏模块.
    /// </summary>
    public readonly TileDestructor Destructor;

    /// <summary>
    /// 获取物块区块所属的物块刷新模块.
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

    /// <summary>
    /// 指示区块的量子层.
    /// <br>同一二维位置可以存在不同量子层的区块, 用于无缝子世界</br>
    /// </summary>
    public int QuantumLayer;

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

    /// <summary>
    /// 区块内的物块信息.
    /// </summary>
    public TileInfo[] Infos;

    /// <summary>
    /// 区块内的物块内核.
    /// </summary>
    public TileKernel[] TileKernel;

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
    /// 根据目标坐标为指定坐标物块设置指针.
    /// <br>[!] own 使用内部坐标.</br>
    /// <br>[!] target 使用世界坐标.</br>
    /// </summary>
    public void SetPointer(Point3 own, Point3 tar)
    {
      ref TileInfo info = ref this[own];
      info.SetPointTo(tar);
    }

    /// <summary>
    /// 删除目标坐标物块指针.
    /// <br>[!] 使用内部坐标.</br>
    /// </summary>
    public void RemovePointer(Point3 iCoord)
    {
      ref TileInfo info = ref this[iCoord];
      info.RemovePointTo();
    }

    /// <summary>
    /// 检查指定坐标的物块是否为指针.
    /// <br>[!] 使用内部坐标.</br>
    /// </summary>
    public bool CheckPointer(Point3 iCoord)
    {
      ref TileInfo info = ref this[iCoord];
      return info.IsPointer;
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
    public int GetIndex(Point3 coord)
      => GetIndex(coord.X, coord.Y, coord.Z);

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
      Placer = tile.Scene.GetModule<TilePlacer>();
      Destructor = tile.Scene.GetModule<TileDestructor>();
      Refresher = tile.Scene.GetModule<TileRefresher>();
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
        Console.WriteLine("Error", "区块遍历异常, 请检查输入参数合理性.");
      }
    }

    /// <summary>
    /// 执行初始化操作.
    /// <br>该操作会令区块初始化其整个物块信息的数组.</br>
    /// </summary>
    public void DoInitialize()
    {
      int length = Width * Height * Depth;
      Infos = new TileInfo[length];
      TileKernel = new TileKernel[length];
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
    public bool Place(TileKernel behavior, int x, int y, int z)
    {
      Point3 wCoord = ConvertWorld(new Point3(x, y, z));
      //  if (behavior.CanPlaceMark(Tile, this, GetIndex(x, y, z), ConvertWorld(x, y, z)))
      {
        Placer.Mark(wCoord, behavior);
        return true;
      }
      //  else
      //     return false;
    }

    /// <summary>
    /// 根据区块内坐标和指定类型放置物块.
    /// [!] 使用内部坐标.
    /// </summary>
    public bool Place<T>(int x, int y, int z) where T : TileKernel, new()
    {
      TileKernel behavior = CodeResources<TileKernel>.GetFromType(typeof(T));
      return Place(behavior, x, y, z);
    }

    public void Destruct(int index, bool doEvent = true)
    {
    }

    /// <summary>
    /// 破坏指定坐标的物块.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void Destruct(int x, int y, int z, bool doEvent = true)
    {
      ref TileInfo info = ref this[x, y, z];
      if (info.IsNull)
        return;
      TileKernel comport = TileKernel[GetIndex(x, y, z)];
      if (Tile.CheckPointer(info.GetWCoord3()))
      {
        info = Tile.GetPointTo(info.GetWCoord3());
        if (info.Empty is false && !info.IsNull)
        {
          //        if (comport.CanPlaceMark(Tile, this, info.Index, info.GetWCoord3()))
          Destructor.Mark(info.GetWCoord3(), doEvent);
        }
      }
      else if (!Destructor.Queue.Contains((info.GetWCoord3(), doEvent)))
      {
        if (!info.Empty)
          //      if (comport.CanPlaceMark(Tile, this, info.Index, info.GetWCoord3()))
          Destructor.Mark(info.GetWCoord3(), doEvent);
      }
    }

    private bool _loading = false;
    public bool Loading => _loading;

    private bool _saved = false;
    public bool Saved => _saved;

    public void AsyncLoadChunk(string path)
    {
      _loading = true;
      DoInitialize();
      Task.Run(() =>
      {
        DoLoad(path);
        _loading = false;
      });
    }

    public void LoadChunk(string path)
    {
      DoInitialize();
      DoLoad(path);
    }

    private void DoLoad(string path)
    {
      using (FileStream fileStream = new FileStream(path, FileMode.Open))
      {
        using (BinaryReader reader = new BinaryReader(fileStream))
        {
          ref TileInfo info = ref this[0, 0, 0];
          string typeName;
          TileHandler cCom;
          for (int count = 0; count < Infos.Length; count++)
          {
            info = ref this[count];
            info.LoadStep(reader);
            if (!info.Empty)
            {
              typeName = CodeResources<TileKernel>.GetTypeNameFromHash(reader.ReadInt32());
              if (typeName is not null)
              {
                TileKernel[count] = CodeResources<TileKernel>.GetFromTypeName(typeName);
                TileKernel[count].Tile = Tile;
                TileKernel[count].OnInitialize(Tile, this, info.Index); //执行行为初始化放置
                Refresher.Mark(info.GetWCoord3(), 0);
              }
            }
          }
          // 加入Named Tag，保证TileHandler变动时其他模块能够正常读取
          int handlerCount = reader.ReadInt32();
          Dictionary<string, TileHandler> namedTag = new();
          for (int i = 0; i < Handler.Count; i++)
            namedTag[Handler[i].GetType().Name] = Handler[i];
          for (int i = 0; i < handlerCount; i++)
          {
            int check = reader.ReadInt32();
            if (check != 20250225)
            {
              Debug.Fail("校验码失败，区块存档格式损坏");
            }
            string name = reader.ReadString();
            if (namedTag.TryGetValue(name, out var matchedHandler))
            {
              matchedHandler.LoadStep(reader);
            }
            else
            {
              Debug.Fail("找不到Named Tag：" + name);
            }
          }
        }
      }
    }

    public void AsyncSaveChunk(string path)
    {
      _saved = false;
      Task.Run(() =>
      {
        DoSave(path);
        _saved = true;
      });
    }

    public void SaveChunk(string path)
    {
      try
      {
        DoSave(path);
        _saved = true;
      }
      catch
      {

      }
    }

    private void DoSave(string path)
    {
      using (FileStream fileStream = new FileStream(path, FileMode.Create))
      {
        using (BinaryWriter writer = new BinaryWriter(fileStream))
        {
          int? hash;
          TileKernel tCom;
          TileHandler cCom;
          Span<TileInfo> infoSpan = Infos;
          for (int count = 0; count < infoSpan.Length; count++)
          {
            infoSpan[count].SaveStep(writer);
            tCom = TileKernel[count];
            if (!infoSpan[count].Empty && tCom is not null)
            {
              hash = CodeResources<TileKernel>.GetHashFromTypeName(tCom.Identifier);
              if (hash.HasValue)
              {
                writer.Write(hash.Value);
              }
            }
          }
          // 加入Named Tag，保证TileHandler变动时其他模块能够正常读取
          writer.Write(Handler.Count);
          for (int i = 0; i < Handler.Count; i++)
          {
            writer.Write(20250225);
            cCom = Handler.ElementAt(i);
            string name = cCom.GetType().Name;
            writer.Write(name);
            cCom.SaveStep(writer);
          }
          //2025.2.22: 将区块行为与物块本身行为区分, 以支持更自由的数据存储.
          //例如之前不允许空物块存储数据, 但现在允许于 ChunkScript 存储.
        }
      }
    }

    /// <summary>
    /// 判断同层指定坐标的物块行为与具有指定偏移位置处的物块行为是否相同.
    /// </summary>
    public bool IsSame(Point3 own, Point3 offset)
    {
      var ownCom = TileKernel[GetIndex(own)];
      Point3 tarCoord = ConvertWorld(own + offset);
      if (InChunk(tarCoord))
      {
        var tarCom = TileKernel[GetIndex(own + offset)];
        if (ownCom is null || tarCom is null)
          return false;
        else
          return ownCom.Equals(tarCom);
      }
      else
      {
        var tarCom = Tile.GetTileComport(tarCoord);
        if (ownCom is null || tarCom is null)
          return false;
        else
          return ownCom.Equals(tarCom);
      }
    }
  }
}