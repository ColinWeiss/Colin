namespace Colin.Core.Modulars.Tiles
{
  public enum TileSolid : int
  {
    None,
    Sturdy,
    Loading,
    SlopeLeftUp,    // 左下到右上的45°斜坡
    SlopeRightUp,   // 右下到左上的45°斜坡
    SlopeLeftDown,  // 左上到右下的45°斜坡（用于天花板）
    SlopeRightDown  // 右上到左下的45°斜坡（用于天花板）
  }
}