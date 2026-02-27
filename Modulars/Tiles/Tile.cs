using Colin.Core.IO;
using System.Collections.Concurrent;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块模块.
  /// <br>提供基于区块化的物块管理办法.</br>
  /// </summary>
  public class Tile : ISceneModule, IRenderableISceneModule
  {
    public Scene Scene { get; set; }

    private bool _enable = false;
    public bool Enable
    {
      get => _enable;
      set => _enable = value;
    }

    public RenderTarget2D RawRt { get; set; }

    public bool RawRtVisible { get; set; }

    public bool Presentation { get; set; } = false;

    public ITileContext Context;

    /// <summary>
    /// 区块字典.
    /// <br>用于存储当前已被加载的区块.</br>
    /// <br>键: 区块坐标.</br>
    /// <br>值: 区块.</br>
    /// </summary>
    public ConcurrentDictionary<Point, TileChunk> Chunks = new ConcurrentDictionary<Point, TileChunk>();

    /// <summary>
    /// 指针缓存集合; 用于管理暂存的物块指针数据.
    /// </summary>
    public TilePointerSet PointerSet = new TilePointerSet();

    /// <summary>
    /// 获取指定坐标的物块格指针列表的末尾指针所指向的引用格; 若引用格为空, 则返回 <see cref="TileInfo.Null"/>.
    /// </summary>
    public ref TileInfo GetPointTo(Point3 wCoord)
    {
      ref TileInfo info = ref TileInfo.Null;
      if (PointerSet.Cache.ContainsKey(wCoord) is false)
        return ref TileInfo.Null;
      TilePointer pointTo = PointerSet.Cache[wCoord].First();
      info = ref this[pointTo.PointTo];
      if (info.Empty)
        return ref TileInfo.Null;
      else
        return ref info;
    }
    public ref TileInfo GetPointTo(Point wCoord) => ref GetPointTo(new Point3(wCoord, 0));
    public ref TileInfo GetPointTo(int wCoordX, int wCoordY) => ref GetPointTo(new Point3(wCoordX, wCoordY, 0));

    /// <summary>
    /// 获取指定坐标的物块格的指针列表.
    /// </summary>
    public List<TilePointer> GetPointers(Point3 wCoord)
    {
      return PointerSet.Cache[wCoord];
    }

    /// <summary>
    /// 为指定坐标的物块格添加指针.
    /// <br>[!] 使用世界坐标.</br>
    /// </summary>
    public void AddPointer(Point3 own, TilePointer target)
    {
      PointerSet.AddPointer(own, target);
    }

    /// <summary>
    /// 删除指定坐标的物块格内的指定指针.
    /// </summary>
    public void RemovePointer(Point3 wCoord, TilePointer target)
    {
      PointerSet.RemovePointer(wCoord, target);
    }

    /// <summary>
    /// 检查指定坐标的物块格是否拥有指针.
    /// </summary>
    public bool HasPointer(Point3 wCoord)
    {
      return PointerSet.Cache.ContainsKey(wCoord) && PointerSet.Cache[wCoord].Count > 0;
    }

    public void DoInitialize() { }

    public void Start()
    {
      if (Context is null)
        Console.WriteLine("Error", "物块模块信息设置为 NULL.");
    }

    /// <summary>
    /// 对齐后的渲染左上角位置, 这个位置要求恰好对齐物块左上角
    /// </summary>
    public Vector2 AlignedTopLeft;

    public Vector2 lastTopLeft;

    public void DoUpdate(GameTime time)
    {
    }
    public void Dispose()
    {
    }

    /// <summary>
    /// 从指定坐标获取物块区块对象.
    /// </summary>
    /// <returns>若成功获取, 返回对象; 否则返回 <see langword="null"/>.</returns>
    public TileChunk GetChunk(int x, int y)
    {
      if (Chunks.TryGetValue(new Point(x, y), out TileChunk chunk))
        return chunk;
      else
        return null;
    }
    /// <summary>
    /// 从指定坐标获取物块区块对象.
    /// </summary>
    /// <returns>若成功获取, 返回对象; 否则返回 <see langword="null"/>.</returns>
    public TileChunk GetChunk(Point coord) => GetChunk(coord.X, coord.Y);
    /// <summary>
    /// 从世界物块坐标获取其坐标所在的区块对象.
    /// </summary>
    /// <returns>若成功获取, 返回对象; 否则返回 <see langword="null"/>.</returns>
    public TileChunk GetChunkForWorldCoord(int worldCoordX, int worldCoordY)
    {
      int indexX = worldCoordX >= 0 ? worldCoordX / Context.ChunkWidth : (worldCoordX + 1) / Context.ChunkWidth - 1;
      int indexY = worldCoordY >= 0 ? worldCoordY / Context.ChunkHeight : (worldCoordY + 1) / Context.ChunkHeight - 1;
      return GetChunk(indexX, indexY);
    }
    /// <summary>
    /// 索引器: 根据世界物块坐标获取指定位置的物块.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public ref TileInfo this[int x, int y, int z]
    {
      get
      {
        int indexX = x >= 0 ? x % Context.ChunkWidth : ((x + 1) % Context.ChunkWidth) + (Context.ChunkWidth - 1);
        int indexY = y >= 0 ? y % Context.ChunkHeight : ((y + 1) % Context.ChunkHeight) + (Context.ChunkHeight - 1);
        TileChunk target = GetChunkForWorldCoord(x, y);
        if (target is not null)
          return ref target[indexX, indexY, z];
        else
          return ref TileInfo.Null;
      }
    }

    /// <summary>
    /// 索引器: 根据世界物块坐标获取指定位置的物块.
    /// </summary>
    public ref TileInfo this[Point3 coord] => ref this[coord.X, coord.Y, coord.Z];

    public TileKernel GetHandler(int x, int y, int z, bool includeLoadingChunk = false)
    {
      int indexX = x >= 0 ? x % Context.ChunkWidth : ((x + 1) % Context.ChunkWidth) + (Context.ChunkWidth - 1);
      int indexY = y >= 0 ? y % Context.ChunkHeight : ((y + 1) % Context.ChunkHeight) + (Context.ChunkHeight - 1);
      TileChunk target = GetChunkForWorldCoord(x, y);
      if (target is not null && (includeLoadingChunk || !target.InOperation))
        return target.Kernals[target.GetIndex(indexX, indexY, z)];
      else
        return null;
    }

    public TileKernel GetHandler(Point3 coord)
      => GetHandler(coord.X, coord.Y, coord.Z);

    public T GetHandler<T>(ref TileInfo info) where T : TileHandler
    {
      return GetChunk(ref info).GetHandler<T>();
    }

    public ref TileInfo GetRelative(Point3 wCoord, TileRelative relative)
    {
      ref TileInfo info = ref this[wCoord];
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
      if (temp.Z < 0 || temp.Z >= Context.Depth)
        return ref TileInfo.Null;
      else
        return ref this[info.GetWCoord3() + temp];
    }

    public int GetNeighborCount(Point3 wCoord)
    {
      int result = 0;
      if (GetRelative(wCoord, TileRelative.Front).Empty is false)
        result++;
      if (GetRelative(wCoord, TileRelative.Behind).Empty is false)
        result++;
      if (GetRelative(wCoord, TileRelative.Left).Empty is false)
        result++;
      if (GetRelative(wCoord, TileRelative.Right).Empty is false)
        result++;
      if (GetRelative(wCoord, TileRelative.Up).Empty is false)
        result++;
      if (GetRelative(wCoord, TileRelative.Down).Empty is false)
        result++;
      return result;
    }

    /// <summary>
    /// 判断指定坐标的区块是否存在.
    /// </summary>
    public bool HasChunk(int x, int y) => Chunks.ContainsKey(new Point(x, y));
    /// <summary>
    /// 判断指定坐标的区块是否存在.
    /// </summary>
    public bool HasChunk(Point coord) => Chunks.ContainsKey(coord);
    /// <summary>
    /// 从世界坐标获取区块坐标与物块区块坐标.
    /// </summary>
    public (Point cCoord, Point tCoord) GetCoords(int wCoordX, int wCoordY)
    {
      int cCoordX = wCoordX >= 0 ? wCoordX / Context.ChunkWidth : (wCoordX + 1) / Context.ChunkWidth - 1;
      int cCoordY = wCoordY >= 0 ? wCoordY / Context.ChunkHeight : (wCoordY + 1) / Context.ChunkHeight - 1;
      int tCoordX = wCoordX >= 0 ? wCoordX % Context.ChunkWidth : (wCoordX + 1) % Context.ChunkWidth + (Context.ChunkWidth - 1);
      int tCoordY = wCoordY >= 0 ? wCoordY % Context.ChunkHeight : (wCoordY + 1) % Context.ChunkHeight + (Context.ChunkHeight - 1);
      return (new Point(cCoordX, cCoordY), new Point(tCoordX, tCoordY));
    }

    public TileChunk GetChunk(ref TileInfo info)
    {
      int worldCoordX = info.WCoordX;
      int worldCoordY = info.WCoordY;
      int chunkCoordX = worldCoordX >= 0 ? worldCoordX / Context.ChunkWidth : (worldCoordX + 1) / Context.ChunkWidth - 1;
      int chunkCoordY = worldCoordY >= 0 ? worldCoordY / Context.ChunkHeight : (worldCoordY + 1) / Context.ChunkHeight - 1;
      return GetChunk(chunkCoordX, chunkCoordY);
    }
    /// <summary>
    /// 从指定位置获取世界物块坐标.
    /// </summary>
    public Point GetWorldCoordForPosition(Vector2 position)
    {
      int coordX = (int)Math.Floor(position.X / Context.TileLength);
      int coordY = (int)Math.Floor(position.Y / Context.TileLength);
      return new Point(coordX, coordY);
    }

    public Point GetChunkCoordForWorldCoord(int worldCoordX, int worldCoordY)
    {
      int chunkCoordX = worldCoordX >= 0 ? worldCoordX / Context.ChunkWidth : (worldCoordX + 1) / Context.ChunkWidth - 1;
      int chunkCoordY = worldCoordY >= 0 ? worldCoordY / Context.ChunkHeight : (worldCoordY + 1) / Context.ChunkHeight - 1;
      return new Point(chunkCoordX, chunkCoordY);
    }

    public bool Place<T>(int x, int y, int z) where T : TileKernel, new()
    {
      var coords = GetCoords(x, y);
      TileChunk targetChunk = GetChunk(coords.cCoord.X, coords.cCoord.Y);
      if (targetChunk is not null && !targetChunk.InOperation)
        return targetChunk.Place<T>(coords.tCoord.X, coords.tCoord.Y, z);
      else
        return false;
    }

    public bool Place(TileKernel behavior, int x, int y, int z)
    {
      var coords = GetCoords(x, y);
      TileChunk targetChunk = GetChunk(coords.cCoord.X, coords.cCoord.Y);
      if (targetChunk is not null && !targetChunk.InOperation)
        return targetChunk.Place(behavior, coords.tCoord.X, coords.tCoord.Y, z);
      else
        return false;
    }

    /// <summary>
    /// 从世界坐标获取对应物块在本身区块的索引.
    /// </summary>
    /// <returns></returns>
    public int GetIndex(int wCoordX, int wCoordY, int z)
    {
      int tCoordX = wCoordX >= 0 ? wCoordX % Context.ChunkWidth : (wCoordX + 1) % Context.ChunkWidth + (Context.ChunkWidth - 1);
      int tCoordY = wCoordY >= 0 ? wCoordY % Context.ChunkHeight : (wCoordY + 1) % Context.ChunkHeight + (Context.ChunkHeight - 1);
      return z * Context.ChunkWidth * Context.ChunkHeight + tCoordX + tCoordY * Context.ChunkWidth;
    }
    public int GetIndex(Point3 wCoord) => GetIndex(wCoord.X, wCoord.Y, wCoord.Z);

    /// <summary>
    /// 使用世界物块坐标破坏指定位置的物块.
    /// <br>如果该坐标为物块指针, 则针对其指向的物块操作.</br>
    /// </summary>
    public void Destruct(int x, int y, int z)
    {
      var coords = GetCoords(x, y);
      TileChunk targetChunk = GetChunk(coords.cCoord.X, coords.cCoord.Y);
      if (targetChunk is not null)
        targetChunk.Destruct(coords.tCoord.X, coords.tCoord.Y, z);
    }

    public void Destruct(Point p)
      => Destruct(p.X, p.Y, 0);



    public void CreateEmptyChunk(Point coord, int? quantumLayer = null) => CreateEmptyChunk(coord.X, coord.Y, quantumLayer);

    /// <summary>
    /// 在指定坐标新创建一个空区块.
    /// <br>[!] 这个行为会强制覆盖指定坐标的区块, 无论它是否已经加载.</br>
    /// </summary>
    public void CreateEmptyChunk(int x, int y, int? quantumLayer = null)
    {
      TileChunk chunk = new TileChunk(this, new Point(x, y));
      chunk.DoInitialize();
      Chunks[chunk.Coord] = chunk;
    }

    /// <summary>
    /// 从指定路径的文件加载区块到指定坐标.
    /// </summary>
    public void LoadChunk(int x, int y, string path, int? quantumLayer = null)
    {
      if (File.Exists(path))
      {
        TileChunk chunk = new TileChunk(this, new Point(x, y));
        chunk.AsyncLoadChunk(path);
        Chunks.TryAdd(chunk.Coord, chunk);
      }
      else
        Console.WriteLine("Error", string.Concat("加载 (", x, ",", y, ") 处的区块时出现异常."));
    }

    public void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      lastTopLeft = AlignedTopLeft;
      AlignedTopLeft = Vector2.Floor(Scene.Camera.ConvertToWorld(Vector2.Zero) / Context.TileSizeF) * Context.TileSizeF;
    }

    public void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch)
    {
    }
  }
}