namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 物块纹理版式.
  /// <br>记录一组, 物块+边框+转角, 在纹理中的位置, 以及每组的高度.</br>
  /// <br>如果纹理图像的高度是多组的高度, 则随机从中取出一组作为物块帧.</br>
  /// </summary>
  public class TileSpriteSheet
  {
    public enum TileSpriteFormat : int
    {
      Normal = 0,
      Max = 1,
    };

    protected static TileSpriteSheet[] TileSpriteRepository = new TileSpriteSheet[(int)TileSpriteFormat.Max];

    private static bool inited = false;

    public Point CornerTextureOffset;

    public Point BorderTextureOffset;

    public Point SolidTextureOffset;

    public int Height;

    public static TileSpriteSheet Query(TileSpriteFormat key)
    {
      if (!inited)
        LoadTileSpriteSheets();
      return TileSpriteRepository[(int)key];
    }

    public static void LoadTileSpriteSheets()
    {
      // TODO: 通过加载配置文件初始化排版方式, 目前这里硬编码几个用于测试
      RegisterTileSpriteSheet(TileSpriteFormat.Normal, new Point(112, 16), new Point(64, 16), new Point(16, 16), 48);
      inited = true;
    }

    public static void RegisterTileSpriteSheet(TileSpriteFormat key, Point cornerOffset, Point borderOffset, Point solidOffset, int height)
    {
      TileSpriteRepository[(int)key] = new TileSpriteSheet(cornerOffset, borderOffset, solidOffset, height);
    }

    protected TileSpriteSheet(Point cornerOffset, Point borderOffset, Point solidOffset, int height)
    {
      CornerTextureOffset = cornerOffset;
      BorderTextureOffset = borderOffset;
      SolidTextureOffset = solidOffset;
      Height = height;
    }
  }
}