﻿namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 表示瓦片地图中的单个瓦片.
  /// </summary>
  public struct TileInfo
  {
    /// <summary>
    /// 指示物块的空状态.
    /// </summary>
    public bool Empty;

    public bool IsPointer
    {
      get
      {
        return !IsNull && Tile.TilePointers.ContainsKey(WorldCoord2);
      }
    }

    /// <summary>
    /// 指示物块在区块内的索引.
    /// </summary>
    public int Index;

    /// <summary>
    /// 指示物块的横坐标.
    /// </summary>
    public int CoordX => Index % (TileOption.ChunkWidth * TileOption.ChunkHeight) % TileOption.ChunkWidth;

    /// <summary>
    /// 指示物块的纵坐标.
    /// </summary>
    public int CoordY => Index % (TileOption.ChunkWidth * TileOption.ChunkHeight) / TileOption.ChunkWidth;

    /// <summary>
    /// 指示物块所处的深度.
    /// </summary>
    public int CoordZ => Index / (TileOption.ChunkWidth * TileOption.ChunkHeight);

    private Point _chunkCoord2;
    /// <summary>
    /// 指示物块在所属区块中的坐标.
    /// </summary>
    public Point ChunkCoord2
    {
      get
      {
        if (_chunkCoord2 == Point.Zero)
          _chunkCoord2 = new Point(CoordX, CoordY);
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
          _chunkCoord3 = new Point3(CoordX, CoordY, CoordZ);
        return _chunkCoord3;
      }
    }

    private Point _worldCoord2;
    /// <summary>
    /// 指示物块在世界内的坐标.
    /// </summary>
    public Point WorldCoord2
    {
      get
      {
        if (_worldCoord2 == Point.Zero)
          _worldCoord2 = new Point(Chunk.CoordX * TileOption.ChunkWidth + ChunkCoord2.X, Chunk.CoordY * TileOption.ChunkHeight + ChunkCoord2.Y);
        return _worldCoord2;
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
              Chunk.CoordX * TileOption.ChunkWidth + ChunkCoord2.X,
              Chunk.CoordY * TileOption.ChunkHeight + ChunkCoord2.Y,
              CoordZ);
        }
        return _worldCoord3;
      }
    }

    /// <summary>
    /// 指示物块纹理帧格.
    /// </summary>
    public TileFrame Texture;

    /// <summary>
    /// 指示物块的碰撞信息.
    /// </summary>
    public TileCollision Collision;

    public Tile Tile { get; internal set; }

    /// <summary>
    /// 指示物块所属区块.
    /// </summary>
    public TileChunk Chunk;

    public ref TileInfo Top
    {
      get
      {
        if (Chunk is null)
          return ref _null;
        else
          return ref Tile[WorldCoord2.X, WorldCoord2.Y - 1, CoordZ];
      }
    }

    public ref TileInfo Bottom
    {
      get
      {
        if (Chunk is null)
          return ref _null;
        else
          return ref Tile[WorldCoord2.X, WorldCoord2.Y + 1, CoordZ];
      }
    }

    public ref TileInfo Left
    {
      get
      {
        if (Chunk is null)
          return ref _null;
        else
          return ref Tile[WorldCoord2.X - 1, WorldCoord2.Y, CoordZ];
      }
    }

    public ref TileInfo Right
    {
      get
      {
        if (Chunk is null)
          return ref _null;
        else
          return ref Tile[WorldCoord2.X + 1, WorldCoord2.Y, CoordZ];
      }
    }

    public ref TileInfo Front
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

    public ref TileInfo Behind
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

    public Dictionary<Type, TileScript> Scripts;
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
          _hitBox = new RectangleF(WorldCoord2.ToVector2() * TileOption.TileSizeF, TileOption.TileSizeF);
        return _hitBox;
      }
    }

    public TileInfo()
    {
      Index = 0;
      Empty = true;
      Tile = null;
      Chunk = null;
      Texture = new TileFrame(-1, -1);
      Collision = TileCollision.Passable;
      _chunkCoord2 = Point.Zero;
      _chunkCoord3 = Point3.Zero;
      _worldCoord2 = Point.Zero;
      _worldCoord3 = Point3.Zero;
      _hitBox = RectangleF.Empty;
      Scripts = new Dictionary<Type, TileScript>();
    }

    internal void LoadStep(BinaryReader reader)
    {
      Empty = reader.ReadBoolean();
      if (!Empty)
      {
        Texture.LoadStep(reader);
        Collision = (TileCollision)reader.ReadInt32();
      }
    }

    internal void SaveStep(BinaryWriter writer)
    {
      writer.Write(Empty);
      if (!Empty)
      {
        Texture.SaveStep(writer);
        writer.Write((int)Collision);
      }
    }

    internal static TileInfo _null = new TileInfo()
    {
      _isNull = true
    };
    public static ref TileInfo Null => ref _null;

    private bool _isNull = false;
    public bool IsNull
    {
      get
      {
        return _isNull;
      }
    }
  }
}