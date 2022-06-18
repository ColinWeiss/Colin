namespace Colin.Common.Code.Tiled
{
    /// <summary>
    /// 表示一个可作为物块实体的对象.
    /// </summary>
    public interface ITileEntity : IEngineElement
    {
        /// <summary>
        /// 该实体绑定的物块.
        /// </summary>
        public Tile Tile { get; }
    }
}