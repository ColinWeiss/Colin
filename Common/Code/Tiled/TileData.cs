using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Common.Code.Tiled
{
    /// <summary>
    /// 一组物块数据.
    /// </summary>
    public struct TileData
    {
        /// <summary>
        /// 物块数据所绑定的物块.
        /// </summary>
        public Tile Tile { get; internal set; }

        /// <summary>
        /// 物块位于所在瓦片地图的横坐标.
        /// </summary>
        public int CoordinateX;

        /// <summary>
        /// 物块位于所在瓦片地图的纵坐标.
        /// </summary>
        public int CoordinateY;

        /// <summary>
        /// 指示物块是否启用循环纹理的值.
        /// </summary>
        public bool LoopTextureEnable;

        /// <summary>
        /// 循环纹理的宽度范围.
        /// </summary>
        public int LoopTextureWidth;

        /// <summary>
        /// 循环纹理的高度范围.
        /// </summary>
        public int LoopTextureHeight;

        /// <summary>
        /// 物块纹理帧格选取.
        /// </summary>
        public TileFrameGrid TextureFrame;

        /// <summary>
        /// 物块边框帧格选取.
        /// </summary>
        public TileFrameGrid BorderFrame;

        /// <summary>
        /// 物块的周围物块的存在状态.
        /// </summary>
        public TileRoundState TileRoundState;

        /// <summary>
        /// 进行一次物块纹理帧格刷新.
        /// </summary>
        public void RefreshTextureFrame( )
        {
            if ( LoopTextureEnable )
            {
                //↓根据物块在瓦片地图的坐标, 取余纹理长宽以达到循环纹理的目的.
                TextureFrame.SetFrame(
                    CoordinateX % LoopTextureWidth / Tile.TileMap.GridSize,
                    CoordinateY % LoopTextureHeight / Tile.TileMap.GridSize, 1, 1 );
            }
        }

    }
}