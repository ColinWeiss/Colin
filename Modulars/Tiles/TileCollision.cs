﻿namespace Colin.Core.Modulars.Tiles
{
  /// <summary>
  /// 控制物块的碰撞检测和响应行为.
  /// </summary>
  public enum TileCollision : int
  {
    /// <summary>
    /// 可通行.
    /// </summary>
    Passable = 0,

    /// <summary>
    /// 实心方块.
    /// </summary>
    Solid = 1,

    /// <summary>
    /// 平台.
    /// </summary>
    Platform = 2,
  }
}