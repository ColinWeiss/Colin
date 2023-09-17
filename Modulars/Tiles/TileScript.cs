﻿using Colin.Core.Modulars.Tiles;
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
        public int ID { get; internal set; }
        public ref TileInfo Info => ref Tile.Infos[ID];
        public int CoordX => Info.CoordX;
        public int CoordY => Info.CoordY;
        public int CoordZ => Info.CoordZ;
        /// <summary>
        /// 在第一次放置时执行.
        /// </summary>
        public virtual void OnPlace() { }
        /// <summary>
        /// 于物块刷新时执行.
        /// </summary>
        public virtual void OnRefresh() { }
        internal void LoadStep( BinaryReader reader ) { }
        internal void SaveStep( BinaryWriter writer ) { }
    }
}