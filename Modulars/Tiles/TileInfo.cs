using DeltaMachine.Core.Common.Tiles.Scripts;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 表示瓦片地图中的单个瓦片的基本信息.
  /// </summary>
  public struct TileInfo
  {
    /// <summary>
    /// 指示物块的空状态.
    /// </summary>
    public bool Empty;

    /// <summary>
    /// 指示物块是否为一个物块指针.
    /// </summary>
    public bool IsPointer
    {
      get
      {
        return !IsNull && _pointTo is not null;
      }
    }

    /// <summary>
    /// 指示物块在区块内的索引.
    /// </summary>
    public int Index;

    /// <summary>
    /// 指示物块在区块内的横坐标.
    /// </summary>
    public int ChunkCoordX => Index % (Tile.Option.ChunkWidth * Tile.Option.ChunkHeight) % Tile.Option.ChunkWidth;

    /// <summary>
    /// 指示物块在区块内的纵坐标.
    /// </summary>
    public int ChunkCoordY => Index % (Tile.Option.ChunkWidth * Tile.Option.ChunkHeight) / Tile.Option.ChunkWidth;

    /// <summary>
    /// 指示物块所处的深度.
    /// </summary>
    public int CoordZ => Index / (Tile.Option.ChunkWidth * Tile.Option.ChunkHeight);

    private Point _chunkCoord2;
    /// <summary>
    /// 指示物块在所属区块中的坐标.
    /// </summary>
    public Point ChunkCoord2
    {
      get
      {
        if (_chunkCoord2 == Point.Zero)
          _chunkCoord2 = new Point(ChunkCoordX, ChunkCoordY);
        return _chunkCoord2;
      }
    }

    private Point3 _chunkCoord3;
    /// <summary>
    /// 指示物块在所属区块中的坐标.
    /// </summary>
    public Point3 ChunkCoord3
    {
      get
      {
        if (_chunkCoord3 == Point3.Zero)
          _chunkCoord3 = new Point3(ChunkCoordX, ChunkCoordY, CoordZ);
        return _chunkCoord3;
      }
    }

    private Point? _worldCoord2 = null;
    /// <summary>
    /// 指示物块在世界内的坐标.
    /// </summary>
    public Point WorldCoord2
    {
      get
      {
        if (_worldCoord2 is null)
          _worldCoord2 = new Point(Chunk.CoordX * Tile.Option.ChunkWidth + ChunkCoord2.X, Chunk.CoordY * Tile.Option.ChunkHeight + ChunkCoord2.Y);
        return _worldCoord2.Value;
      }
    }

    private Point3 _worldCoord3;
    /// <summary>
    /// 指示物块在世界内的坐标.
    /// </summary>
    public Point3 WorldCoord3
    {
      get
      {
        if (_worldCoord3 == Point3.Zero)
        {
          _worldCoord3 = new Point3(
              Chunk.CoordX * Tile.Option.ChunkWidth + ChunkCoord2.X,
              Chunk.CoordY * Tile.Option.ChunkHeight + ChunkCoord2.Y,
              CoordZ);
        }
        return _worldCoord3;
      }
    }

    /// <summary>
    /// 指示物块纹理帧格.
    /// </summary>
    public TileFrame Frame;

    /// <summary>
    /// 指示物块的碰撞信息.
    /// </summary>
    public TileCollision Collision;

    public Tile Tile { get; internal set; }

    /// <summary>
    /// 指示物块所属区块.
    /// </summary>
    public TileChunk Chunk;

    public ref readonly TileInfo Top
    {
      get
      {
        if (Chunk is null)
          return ref _null;
        else
          return ref Tile[WorldCoord2.X, WorldCoord2.Y - 1, CoordZ];
      }
    }

    public ref readonly TileInfo Bottom
    {
      get
      {
        if (Chunk is null)
          return ref _null;
        else
          return ref Tile[WorldCoord2.X, WorldCoord2.Y + 1, CoordZ];
      }
    }

    public ref readonly TileInfo Left
    {
      get
      {
        if (Chunk is null)
          return ref _null;
        else
          return ref Tile[WorldCoord2.X - 1, WorldCoord2.Y, CoordZ];
      }
    }

    public ref readonly TileInfo Right
    {
      get
      {
        if (Chunk is null)
          return ref _null;
        else
          return ref Tile[WorldCoord2.X + 1, WorldCoord2.Y, CoordZ];
      }
    }

    public ref readonly TileInfo Front
    {
      get
      {
        if (Chunk is null)
          return ref _null;
        else if (CoordZ - 1 >= 0)
          return ref Tile[WorldCoord2.X, WorldCoord2.Y, CoordZ - 1];
        else
          return ref _null;
      }
    }

    public ref readonly TileInfo Behind
    {
      get
      {
        if (Chunk is null)
          return ref _null;
        else if (CoordZ + 1 < Chunk.Depth)
          return ref Tile[WorldCoord2.X, WorldCoord2.Y, CoordZ + 1];
        else
          return ref _null;
      }
    }

    public void ClearScript() => Scripts = null;

    public Dictionary<Type, TileScript> Scripts;
    public LiquidScript LiquidScript;

    // 属于空物块也要有的特殊组件 这里直接特判处理
    public LiquidScript AddLiquidScript()
    {
      LiquidScript = new LiquidScript();
      LiquidScript.Tile = Tile;
      LiquidScript.Chunk = Chunk;
      LiquidScript.Index = Index;
      LiquidScript.Info = this;
      return LiquidScript;
    }

    public T AddScript<T>() where T : TileScript, new()
    {
      T t = new T();
      t.Tile = Tile;
      t.Chunk = Chunk;
      t.Index = Index;
      Scripts.Add(t.GetType(), t);
      return t;
    }

    public T GetScript<T>() where T : TileScript
    {
      if (IsPointer)
        return Tile.GetInfoReference(WorldCoord3).GetScript<T>();
      if (Scripts is null)
        return null;
      if (Scripts.TryGetValue(typeof(T), out TileScript script))
        return script as T;
      else
        return null;
    }

    public TileBehavior Behavior = null;

    private RectangleF _hitBox;
    public RectangleF HitBox
    {
      get
      {
        if (_hitBox.X == 0 && _hitBox.Y == 0 && _hitBox.Width == 0 && _hitBox.Height == 0)
          _hitBox = new RectangleF(WorldCoord2.ToVector2() * Tile.Option.TileSizeF, Tile.Option.TileSizeF);
        return _hitBox;
      }
    }

    public TileInfo()
    {
      Index = 0;
      Empty = true;
      Tile = null;
      Chunk = null;
      _pointTo = null;
      Frame = new TileFrame(-1, -1);
      Collision = TileCollision.Passable;
      _chunkCoord2 = Point.Zero;
      _chunkCoord3 = Point3.Zero;
      _worldCoord2 = null;
      _worldCoord3 = Point3.Zero;
      _hitBox = RectangleF.Empty;
      Scripts = new Dictionary<Type, TileScript>();
    }

    private Point3? _pointTo = null;
    public Point3? PointTo => _pointTo;
    public void SetPointTo(Point3 coord) => _pointTo = coord;
    public void RemovePointTo() => _pointTo = null;

    internal void LoadStep(BinaryReader reader)
    {
      string pointExist = reader.ReadString();
      if (pointExist is "X")
        _pointTo = new Point3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
      else
        _pointTo = null;
      Empty = reader.ReadBoolean();
      if (!Empty)
      {
        Frame.LoadStep(reader);
        Collision = (TileCollision)reader.ReadInt32();
      }
      LiquidScript.LoadStep(reader);
    }

    internal void SaveStep(BinaryWriter writer)
    {
      if (_pointTo is not null)
      {
        writer.Write("X");
        writer.Write(_pointTo.Value.X);
        writer.Write(_pointTo.Value.Y);
        writer.Write(_pointTo.Value.Z);
      }
      else
      {
        writer.Write("M");
      }
      writer.Write(Empty);
      if (!Empty)
      {
        Frame.SaveStep(writer);
        writer.Write((int)Collision);
      }
      LiquidScript.SaveStep(writer);
    }

    /// <summary>
    /// 判断同层指定相对于该物块坐标具有指定偏移位置处的物块是否相同.
    /// </summary>
    /// <param name="dx">偏移的X坐标.</param>
    /// <param name="dy">偏移的Y坐标.</param>
    /// <returns></returns>
    public bool IsSame(int dx, int dy)
    {
      TileInfo info = Tile[WorldCoord2.X + dx, WorldCoord2.Y + dy, CoordZ];
      if (info.Behavior is null || Behavior is null)
        return false;
      else
        return info.Behavior.Equals(Behavior);
    }

    private bool _isNull;
    public bool IsNull => _isNull;
    internal static TileInfo _null = new TileInfo()
    {
      _isNull = true
    };
    public static ref TileInfo Null => ref _null;
  }
}