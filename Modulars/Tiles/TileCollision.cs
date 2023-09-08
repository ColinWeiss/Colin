namespace Colin.Core.Modulars.Tiles
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
        /// 不允许通过.
        /// </summary>
        Impassable = 1,

        /// <summary>
        /// 平台.
        /// </summary>
        Platform = 2,
    }
}