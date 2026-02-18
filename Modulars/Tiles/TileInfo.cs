using Colin.Core.IO;

namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 表示瓦片地图中的单个瓦片的基本信息.
  /// </summary>
  public struct TileInfo : IOStep
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

    public bool Loading;

    private bool _isNull;
    public bool IsNull => _isNull;

    public void LoadStep(StoreBox box)
    {
      Collision = (TileSolid)box.GetInt("C");
      WCoordX = box.GetInt("WX");
      WCoordY = box.GetInt("WY");
      ICoordX = box.GetShort("IX");
      ICoordY = box.GetShort("IY");
      ICoordZ = box.GetShort("IZ");
      Empty = box.GetBool("E");
      Index = box.GetInt("I");
    }

    public StoreBox SaveStep()
    {
      StoreBox box = new StoreBox();
      box.Add("C", (int)Collision);
      box.Add("WX", WCoordX);
      box.Add("WY", WCoordY);
      box.Add("IX", ICoordX);
      box.Add("IY", ICoordY);
      box.Add("IZ", ICoordZ);
      box.Add("E", Empty);
      box.Add("I", Index);
      return box;
    }

    internal static TileInfo _null = new TileInfo()
    {
      _isNull = true
    };
    public static ref TileInfo Null => ref _null;
  }
}