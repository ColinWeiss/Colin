namespace Colin.Core.Modulars.Tiles
{
  public interface ITileSpriteFormat
  {
    public Point Solid { get; }
    public Point Border { get; }
    public Point Corner { get; }
    public int LineHeight { get; }
  }

  /// <summary>
  /// 默认的物块纹理格式.
  /// </summary>
  /// <param name="Solid">读取实心纹理的区域坐标.</param>
  /// <param name="Border">读取边框纹理的区域坐标.</param>
  /// <param name="Corner">读取内转角纹理的区域坐标.</param>
  /// <param name="LineHeight">支持多行选择, 此为指示一行高度.</param>
  public record NormalTileSpriteFormat(Point Solid, Point Border, Point Corner, int LineHeight) : ITileSpriteFormat;

}