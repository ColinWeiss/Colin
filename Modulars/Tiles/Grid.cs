using Colin.Core.Resources;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Colin.Core.Modulars.Tiles
{
  public enum CellSolid : int
  {
    None,
    Sturdy,
    Top,    //未实现
    Bottom, //未实现
    Left,   //未实现
    Right   //未实现
  }

  [StructLayout(LayoutKind.Explicit)]
  public struct Cell
  {
    [FieldOffset(0)]
    public bool Empty;

    [FieldOffset(1)]
    public bool Loaded;

    [FieldOffset(2)]
    public short CoordX;

    [FieldOffset(4)]
    public short CoordY;

    [FieldOffset(6)]
    public short CoordZ;

    [FieldOffset(8)]
    public int WCoordX;

    [FieldOffset(12)]
    public int WCoordY;

    [FieldOffset(16)]
    public int WCoordZ;

    [FieldOffset(20)]
    public CellSolid Solid;

    [FieldOffset(24)]
    public int PointToX;

    [FieldOffset(28)]
    public int PointToY;

    [FieldOffset(32)]
    public int PointToZ;

    public void LoadStep(BinaryReader reader)
    {
      Empty = reader.ReadBoolean();
      CoordX = reader.ReadInt16();
      CoordY = reader.ReadInt16();
      CoordZ = reader.ReadInt16();
      WCoordX = reader.ReadInt32();
      WCoordY = reader.ReadInt32();
      WCoordZ = reader.ReadInt32();
      Solid = (CellSolid)reader.ReadByte();
    }
    public void SaveStep(BinaryWriter writer)
    {
      writer.Write(Empty);
      writer.Write(CoordX);
      writer.Write(CoordY);
      writer.Write(CoordZ);
      writer.Write(WCoordX);
      writer.Write(WCoordY);
      writer.Write(WCoordZ);
      writer.Write((byte)Solid);
    }
  }

  /// <summary>
  /// 表示一个物块脚本.
  /// <br>建议脚本中针对区块进行数据存储.</br>
  /// </summary>
  public interface ICellScript
  {
    public Grid Grid { get; set; }
    public ref Cell[] Cells => ref Grid.Cells;
  }

  /// <summary>
  /// 表示物块纹理格式.
  /// </summary>
  public interface ICellSpriteFormat
  {
    public Point Solid { get; }
    public Point Border { get; }
    public Point Corner { get; }
  }

  /// <summary>
  /// 默认的物块纹理格式.
  /// </summary>
  /// <param name="Solid">读取实心纹理的区域坐标.</param>
  /// <param name="Border">读取边框纹理的区域坐标.</param>
  /// <param name="Corner">读取内转角纹理的区域坐标.</param>
  /// <param name="LineHeight">支持多行选择, 此为指示一行高度.</param>
  public record CellSpriteNormalFormat(Point Solid, Point Border, Point Corner, int LineHeight) : ICellSpriteFormat;

  /// <summary>
  /// 物块行为.
  /// <br>允许自定义物块.</br>
  /// </summary>
  public class CellBehavior
  {
    /// <summary>
    /// 获取所属 <see cref="Tiles.Grid"/>.
    /// </summary>
    public readonly Grid Grid;

    /// <summary>
    /// 执行于判断物块放置标记前.
    /// <br>若结果为 <see langword="true"/>, 则允许进行标记, 否则不进行标记.</br>
    /// </summary>
    public virtual bool CanPlaceMark(in Grid grid, int x, int y, int z) => true;

    /// <summary>
    /// 执行于物块初始化; 此时准备初始数据, 允许于此处添加 <see cref="ICellScript"/>.
    /// </summary>
    public virtual void OnInitialize(in Grid grid, int x, int y, int z) { }

    /// <summary>
    /// 执行于物块放置时.
    /// </summary>
    public virtual void OnPlace(in Grid grid, int x, int y, int z) { }

    /// <summary>
    /// 执行于物块刷新时.
    /// </summary>
    public virtual void OnRefresh(in Grid grid, int x, int y, int z) { }

    /// <summary>
    /// 执行于物块被破坏时.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="conduct"></param>
    public virtual void OnDestruction(in Grid grid, int x, int y, int z) { }
  }

  /// <summary>
  /// 由物块集合表示的区块.
  /// </summary>
  public class Grid
  {
    /// <summary>
    /// 获取区块所属的世界物块模块.
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
    /// 获取区块宽度.
    /// </summary>
    public int Width => Tile.Option.ChunkWidth;

    /// <summary>
    /// 获取区块高度.
    /// </summary>
    public int Height => Tile.Option.ChunkHeight;

    /// <summary>
    /// 获取区块深度.
    /// </summary>
    public readonly int Depth;

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
    public Point Coord => _coord;

    /// <summary>
    /// 物块信息集合.
    /// </summary>
    public Cell[] Cells;

    /// <summary>
    /// 物块行为集合.
    /// </summary>
    public CellBehavior[] Behaviors;

    /// <summary>
    /// 物块脚本.
    /// </summary>
    public Dictionary<Type, ICellScript> Script;

    public Grid(Tile tile, Point coord)
    {
      Tile = tile;
      Placer = tile.Scene.GetModule<TilePlacer>();
      Destructor = tile.Scene.GetModule<TileDestructor>();
      Refresher = tile.Scene.GetModule<TileRefresher>();
      Depth = tile.Depth;
      _coordX = coord.X;
      _coordY = coord.Y;
      _coord = coord;
    }

    /// <summary>
    /// 执行初始化操作.
    /// <br>该操作会令区块初始化其整个物块信息的数组.</br>
    /// </summary>
    public void DoInitialize()
    {
      Cells = new Cell[Width * Height * Depth];
      for (int count = 0; count < Cells.Length; count++)
        CreateInfo(count);
      Behaviors = new CellBehavior[Cells.Length];
      Script = new Dictionary<Type, ICellScript>();
    }

    public void CreateInfo(int index)
    {
      Cells[index] = new Cell();
      Cells[index].Empty = true;
      Cells[index].CoordX = (short)(index % (Tile.Option.ChunkWidth * Tile.Option.ChunkHeight) % Tile.Option.ChunkWidth);
      Cells[index].CoordY = (short)(index % (Tile.Option.ChunkWidth * Tile.Option.ChunkHeight) / Tile.Option.ChunkWidth);
      Cells[index].CoordZ = (short)(index / (Tile.Option.ChunkWidth * Tile.Option.ChunkHeight));
    }

    public int GetIndex(Point3 coord)
    {
      return coord.Z * Width * Height + coord.X + coord.Y * Width;
    }

  }

  public class GridOption
  {

  }
}