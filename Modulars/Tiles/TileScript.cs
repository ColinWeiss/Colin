﻿namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 可编程物块行为.
  /// <br> 允许将额外的属性和方法添加给物块.</br>
  /// <br> 存在物块生命周期钩子.</br>
  /// </summary>
  public abstract class TileScript
  {
    public Tile Tile { get; internal set; }
    public TileChunk Chunk { get; internal set; }
    public int Index { get; internal set; }
    public ref TileInfo Info => ref Chunk[Index];
    private int? _coordX;
    public int CoordX
    {
      get
      {
        if (_coordX is null)
          _coordX = Info.ChunkCoordX;
        return _coordX.Value;
      }
    }
    private int? _coordY;
    public int CoordY
    {
      get
      {
        if (_coordY is null)
          _coordY = Info.ChunkCoordY;
        return _coordY.Value;
      }
    }
    private int? _coordZ;
    public int CoordZ
    {
      get
      {
        if (_coordZ is null)
          _coordZ = Info.CoordZ;
        return _coordZ.Value;
      }
    }
    public Vector2 Coord => new Vector2(CoordX, CoordY);
    private Point? _worldCoord;
    public Point WorldCoord2
    {
      get
      {
        if (_worldCoord is null)
          _worldCoord = Info.WorldCoord2;
        return _worldCoord.Value;
      }
    }

    /// <summary>
    /// 判断是否允许放置.
    /// </summary>
    /// <returns></returns>
    public virtual bool CanPlace() => true;
    /// <summary>
    /// 在第一次放置时执行.
    /// </summary>
    public virtual void OnPlace(TilePlacer placer) { }
    /// <summary>
    /// 于物块初始化时执行.
    /// </summary>
    public virtual void OnInitialize() { }
    /// <summary>
    /// 于物块刷新时执行.
    /// </summary>
    public virtual void OnRefresh(TileRefresher refresher) { }
    /// <summary>
    /// 于物块被破坏时执行.
    /// </summary>
    public virtual void OnDestruction(TileDestructor destructor) { }
    public virtual void LoadStep(BinaryReader reader) { }
    public virtual void SaveStep(BinaryWriter writer) { }

    //public virtual void R(BinaryReader reader) { }
    //public virtual void S(BinaryWriter writer) { }

    /// <summary>
    /// 判断同层指定相对于该物块坐标具有指定偏移位置处的物块是否具有相同的行为方式.
    /// </summary>
    /// <param name="dx">偏移的X坐标.</param>
    /// <param name="dy">偏移的Y坐标.</param>
    /// <returns></returns>
    public bool IsSame(int dx, int dy)
    {
      return Info.IsSame(dx, dy);
    }
  }
}