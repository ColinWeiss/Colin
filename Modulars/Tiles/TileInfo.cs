namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 表示瓦片地图中的单个瓦片的基本信息.
  /// </summary>
  public struct TileInfo
  {
    public TileSolid Collision;

    public int PointToX;

    public int PointToY;

    public int PointToZ;

    public bool IsPointer => PointToX != 0 || PointToY != 0 || PointToZ != 0;

    public Point3 GetPointTo() => new Point3(PointToX, PointToY, PointToZ);

    public void SetPointTo(Point3 target)
    {
      PointToX = target.X;
      PointToY = target.Y;
      PointToZ = target.Z;
    }

    public void RemovePointTo()
    {
      PointToX = 0;
      PointToY = 0;
      PointToZ = 0;
    }

    public int WCoordX;

    public int WCoordY;

    public Point GetWCoord2() => new Point(WCoordX, WCoordY);

    public Point3 GetWCoord3() => new Point3(WCoordX, WCoordY, ICoordZ);

    public int Index;

    public short ICoordX;

    public short ICoordY;

    public short ICoordZ;

    public Point GetICoord2() => new Point(ICoordX, ICoordY);

    public Point3 GetICoord3() => new Point3(ICoordX, ICoordY, ICoordZ);

    public bool Empty;

    public bool Loading;

    private bool _isNull;
    public bool IsNull => _isNull;

    public void LoadStep(BinaryReader reader)
    {
      Collision = (TileSolid)reader.ReadInt32();
      PointToX = reader.ReadInt32();
      PointToY = reader.ReadInt32();
      PointToZ = reader.ReadInt32();
      WCoordX = reader.ReadInt32();
      WCoordY = reader.ReadInt32();
      ICoordX = reader.ReadInt16();
      ICoordY = reader.ReadInt16();
      ICoordZ = reader.ReadInt16();
      Empty = reader.ReadBoolean();
      Index = reader.ReadInt32();
    }

    public void SaveStep(BinaryWriter writer)
    {
      writer.Write((int)Collision);
      writer.Write(PointToX);
      writer.Write(PointToY);
      writer.Write(PointToZ);
      writer.Write(WCoordX);
      writer.Write(WCoordY);
      writer.Write(ICoordX);
      writer.Write(ICoordY);
      writer.Write(ICoordZ);
      writer.Write(Empty);
      writer.Write(Index);
    }

    internal static TileInfo _null = new TileInfo()
    {
      _isNull = true
    };
    public static ref TileInfo Null => ref _null;
  }
}