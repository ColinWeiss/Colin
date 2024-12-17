using Colin.Core.IO;
using System.Collections.Concurrent;

namespace Colin.Core.Modulars.Tiles
{
  public class Tile : ISceneModule, IRenderableISceneModule, IOStep
  {
    public int Depth { get; private set; }

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

    public Tile()
    {
      TileInfo._null.Tile = this;
    }

    /// <summary>
    /// 区块字典.
    /// <br>用于存储当前已被加载的区块.</br>
    /// <br>键: 区块坐标.</br>
    /// <br>值: 区块.</br>
    /// </summary>
    public ConcurrentDictionary<Point, TileChunk> Chunks = new ConcurrentDictionary<Point, TileChunk>();

    /// <summary>
    /// 获取指定坐标物块指向的引用块; 若引用不存在, 则返回 <see cref="TileInfo.Null"/>.
    /// </summary>
    /// <param name="coord"></param>
    /// <returns></returns>
    public ref TileInfo GetInfoReference(Point3 coord)
    {
      ref TileInfo info = ref this[coord];
      if (info.PointTo is not null)
        return ref this[info.PointTo.Value];
      return ref TileInfo.Null;
    }

    public void AddInfoReference(Point3 coord, Point3 coreCoord)
    {
      ref TileInfo info = ref this[coord];
      info.SetPointTo(coreCoord);
    }

    public void RemoveInfoReference(Point3 coord)
    {
      ref TileInfo info = ref this[coord];
      info.RemovePointTo();
    }

    public bool HasInfoReference(Point3 coord)
    {
      ref TileInfo info = ref this[coord];
      if (info.PointTo is not null)
        return true;
      else
        return false;
    }

    /// <summary>
    /// 创建物块模块.
    /// </summary>
    /// <param name="depth"></param>
    public void Create(int depth) => Depth = depth;

    public void DoInitialize() { }
    public void Start() { }

    /// <summary>
    /// 对齐后的渲染左上角位置，这个位置要求恰好对齐物块左上角
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
    /// 指示默认（玩家所处）的量子层.
    /// <br>在探索时拓展未知区块时会拓展同量子层的区块</br>
    /// </summary>
    public int QuantumLayer = 0;

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
      int indexX = worldCoordX >= 0 ? worldCoordX / TileOption.ChunkWidth : (worldCoordX + 1) / TileOption.ChunkWidth - 1;
      int indexY = worldCoordY >= 0 ? worldCoordY / TileOption.ChunkHeight : (worldCoordY + 1) / TileOption.ChunkHeight - 1;
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
        int indexX = x >= 0 ? x % TileOption.ChunkWidth : ((x + 1) % TileOption.ChunkWidth) + (TileOption.ChunkWidth - 1);
        int indexY = y >= 0 ? y % TileOption.ChunkHeight : ((y + 1) % TileOption.ChunkHeight) + (TileOption.ChunkHeight - 1);
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
    public (Point cCoord, Point tCoord) GetCoords(int worldCoordX, int worldCoordY)
    {
      int chunkCoordX = worldCoordX >= 0 ? worldCoordX / TileOption.ChunkWidth : (worldCoordX + 1) / TileOption.ChunkWidth - 1;
      int chunkCoordY = worldCoordY >= 0 ? worldCoordY / TileOption.ChunkHeight : (worldCoordY + 1) / TileOption.ChunkHeight - 1;
      int tileCoordX = worldCoordX >= 0 ? worldCoordX % TileOption.ChunkWidth : (worldCoordX + 1) % TileOption.ChunkWidth + (TileOption.ChunkWidth - 1);
      int tileCoordY = worldCoordY >= 0 ? worldCoordY % TileOption.ChunkHeight : (worldCoordY + 1) % TileOption.ChunkHeight + (TileOption.ChunkHeight - 1);
      return (new Point(chunkCoordX, chunkCoordY), new Point(tileCoordX, tileCoordY));
    }

    public Point GetInnerCoord(int worldCoordX, int worldCoordY)
    {
      int tileCoordX = worldCoordX >= 0 ? worldCoordX % TileOption.ChunkWidth : (worldCoordX + 1) % TileOption.ChunkWidth + (TileOption.ChunkWidth - 1);
      int tileCoordY = worldCoordY >= 0 ? worldCoordY % TileOption.ChunkHeight : (worldCoordY + 1) % TileOption.ChunkHeight + (TileOption.ChunkHeight - 1);
      return new Point(tileCoordX, tileCoordY);
    }

    public Point GetChunkCoord(int worldCoordX, int worldCoordY)
    {
      int chunkCoordX = worldCoordX >= 0 ? worldCoordX / TileOption.ChunkWidth : (worldCoordX + 1) / TileOption.ChunkWidth - 1;
      int chunkCoordY = worldCoordY >= 0 ? worldCoordY / TileOption.ChunkHeight : (worldCoordY + 1) / TileOption.ChunkHeight - 1;
      return new Point(chunkCoordX, chunkCoordY);
    }

    /// <summary>
    /// 从指定位置获取世界物块坐标.
    /// </summary>
    public Point GetWorldCoordForPosition(Vector2 position)
    {
      int coordX = (int)Math.Floor(position.X / TileOption.TileWidth);
      int coordY = (int)Math.Floor(position.Y / TileOption.TileHeight);
      return new Point(coordX, coordY);
    }

    public Point GetChunkCoordForWorldCoord(int worldCoordX, int worldCoordY)
    {
      int chunkCoordX = worldCoordX >= 0 ? worldCoordX / TileOption.ChunkWidth : (worldCoordX + 1) / TileOption.ChunkWidth - 1;
      int chunkCoordY = worldCoordY >= 0 ? worldCoordY / TileOption.ChunkHeight : (worldCoordY + 1) / TileOption.ChunkHeight - 1;
      return new Point(chunkCoordX, chunkCoordY);
    }

    public bool Place<T>(int x, int y, int z) where T : TileBehavior, new()
    {
      var coords = GetCoords(x, y);
      TileChunk targetChunk = GetChunk(coords.cCoord.X, coords.cCoord.Y);
      if (targetChunk is not null)
        return targetChunk.Place<T>(coords.tCoord.X, coords.tCoord.Y, z);
      else
        return false;
    }

    public bool Place(TileBehavior behavior, int x, int y, int z)
    {
      var coords = GetCoords(x, y);
      TileChunk targetChunk = GetChunk(coords.cCoord.X, coords.cCoord.Y);
      if (targetChunk is not null)
        return targetChunk.Place(behavior, coords.tCoord.X, coords.tCoord.Y, z);
      else
        return false;
    }

    /// <summary>
    /// 使用世界物块坐标破坏指定位置的物块.
    /// </summary>
    public void Destruction(int x, int y, int z)
    {
      var coords = GetCoords(x, y);
      TileChunk targetChunk = GetChunk(coords.cCoord.X, coords.cCoord.Y);
      if (targetChunk is not null)
        targetChunk.Destruction(coords.tCoord.X, coords.tCoord.Y, z);
    }

    public void CreateEmptyChunk(Point coord, int? quantumLayer = null) => CreateEmptyChunk(coord.X, coord.Y, quantumLayer);

    /// <summary>
    /// 在指定坐标新创建一个空区块.
    /// <br>[!] 这个行为会强制覆盖指定坐标的区块, 无论它是否已经加载.</br>
    /// </summary>
    public void CreateEmptyChunk(int x, int y, int? quantumLayer = null)
    {
      TileChunk chunk = new TileChunk(this);
      chunk.CoordX = x;
      chunk.CoordY = y;
      chunk.QuantumLayer = quantumLayer ?? QuantumLayer;
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
        TileChunk chunk = new TileChunk(this);
        chunk.AsyncLoadChunk(path);
        chunk.CoordX = x;
        chunk.CoordY = y;
        chunk.QuantumLayer = quantumLayer ?? QuantumLayer;
        Chunks.TryAdd(chunk.Coord, chunk);
      }
      else
        Console.WriteLine("Error", string.Concat("加载 (", x, ",", y, ") 处的区块时出现异常."));
    }
    /// <summary>
    /// 保存指定坐标的区块至指定路径.
    /// </summary>
    public void SaveChunk(int x, int y, string path)
    {
      TileChunk chunk = GetChunk(x, y);
      if (chunk is not null)
        chunk.SaveChunk(path);
      else
        Console.WriteLine("Error", string.Concat("卸载 (", x, ",", y, ") 处的区块时出现异常."));
    }

    public void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      lastTopLeft = AlignedTopLeft;
      AlignedTopLeft = Vector2.Floor(Scene.SceneCamera.ConvertScreenToWorld(Vector2.Zero) / TileOption.TileSizeF) * TileOption.TileSizeF;
    }

    public void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch)
    {
    }

    public void LoadStep(BinaryReader reader)
    {

    }

    public void SaveStep(BinaryWriter writer)
    {

    }
  }
}