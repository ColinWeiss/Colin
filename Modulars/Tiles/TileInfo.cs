using Colin.Core.IO;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 表示瓦片地图中的单个瓦片的基本信息.
  /// <br>[!] 结构体不参与逐格存取: 区块存档把格子摊成批量数组整存整取, 见 <see cref="TileChunk"/>.</br>
  /// </summary>
  public struct TileInfo
  {
    public TileSolid Collision;

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

    private bool _isNull;
    public bool IsNull => _isNull;

    public int GetSeed()
    {
      return WCoordX * 17 + WCoordY + ICoordX * 137 + ICoordY;
    }

    internal static TileInfo _null = new TileInfo()
    {
      _isNull = true
    };
    public static ref TileInfo Null => ref _null;
  }
}