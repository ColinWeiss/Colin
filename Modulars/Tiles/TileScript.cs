using Colin.Core.Modulars.Tiles;
using DeltaMachine.Core.GameContents.Scenes.Worlds;

namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 可编程物块行为.
    /// <br> 允许将额外的属性和方法添加给物块.</br>
    /// <br> 存在物块生命周期钩子.</br>
    /// </summary>
    public abstract class TileScript
    {
        public Tile Tile { get; internal set; }
        public TileChunk Chunk { get; internal set; }
        public int Index { get; internal set; }
        public ref TileInfo Info => ref Chunk[Index];
        public int CoordX => Info.CoordX;
        public int CoordY => Info.CoordY;
        public int CoordZ => Info.CoordZ;
        public Vector2 Coord => new Vector2( CoordX , CoordY );
        public Point WorldCoord => Info.WorldCoord2;
        /// <summary>
        /// 在第一次放置时执行.
        /// </summary>
        public virtual void OnPlace() { }
        /// <summary>
        /// 于物块初始化时执行.
        /// </summary>
        public virtual void OnInitialize() { }
        /// <summary>
        /// 于物块刷新时执行.
        /// </summary>
        public virtual void OnRefresh() { }
        /// <summary>
        /// 于物块被破坏时执行.
        /// </summary>
        public virtual void OnDestruction() { }
        public virtual void LoadStep( BinaryReader reader ) { }
        public virtual void SaveStep( BinaryWriter writer ) { }
    }
}