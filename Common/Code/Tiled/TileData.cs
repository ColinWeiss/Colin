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
        /// 边框绘制时需要用于对齐绘制的偏移量.
        /// </summary>
        public Point BorderRenderOffSet;

        /// <summary>
        /// 物块纹理帧格选取.
        /// </summary>
        public TileFrameGrid TextureFrame;

        /// <summary>
        /// 物块边框帧格选取.
        /// </summary>
        public TileFrameGrid BorderFrame;

        /// <summary>
        /// 边框选取起点.
        /// </summary>
        public TileFrameGrid BorderOriginFrame;

        /// <summary>
        /// 物块的周围物块的存在状态.
        /// </summary>
        public TileRoundState RoundState;

        /// <summary>
        /// 获取物块判断周围非空状态的上一帧的值.
        /// </summary>
        public TileRoundState RoundStateLast;

        /// <summary>
        /// 对必要的数据进行每帧维护.
        /// </summary>
        public void UpdateTileData( )
        {
            RoundStateLast = RoundState;
            if ( !Tile.Top.Empty && !Tile.Left.Empty && !Tile.Bottom.Empty && !Tile.Right.Empty )
                Tile.BorderVisible = false;
            else
                Tile.BorderVisible = true;

        }

        /// <summary>
        /// 进行对物块纹理帧格、边框帧格、边框选取起点的刷新以及周围物块存在状态的检测.
        /// </summary>
        public void Refresh()
        {
            RefreshRoundState( );
            RefreshTextureFrame( );
            RefreshBorderFrame( );
            RefreshBorderOriginFrame( );
        }

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

        /// <summary>
        /// 进行一次物块边框帧格刷新.
        /// </summary>
        public void RefreshBorderFrame( )
        {
            switch ( RoundState )
            {
                case TileRoundState.Center:
                    {
                        BorderFrame.FrameX = 0;
                        BorderFrame.FrameY = 0;
                        BorderFrame.FrameWidth = 3;
                        BorderFrame.FrameHeight = 3;
                        BorderRenderOffSet = new Point( -1, -1 );
                    }
                    break;
                case TileRoundState.Up:
                    {
                        BorderFrame.FrameX = 0;
                        BorderFrame.FrameY = 1;
                         BorderFrame.FrameWidth = 3;
                       BorderFrame.FrameHeight = 2;
                        BorderRenderOffSet = new Point( -1, 0 );
                    }
                    break;
                case TileRoundState.Down:
                    {
                        BorderFrame.FrameX = 0;
                        BorderFrame.FrameY = 0;
                         BorderFrame.FrameWidth = 3;
                       BorderFrame.FrameHeight = 2;
                        BorderRenderOffSet = new Point( -1, -1 );
                    }
                    break;
                case TileRoundState.Left:
                    {
                        BorderFrame.FrameX = 1;
                        BorderFrame.FrameY = 0;
                         BorderFrame.FrameWidth = 2;
                       BorderFrame.FrameHeight = 3;
                        BorderRenderOffSet = new Point( 0, -1 );
                    }
                    break;
                case TileRoundState.Right:
                    {
                        BorderFrame.FrameX = 0;
                        BorderFrame.FrameY = 0;
                         BorderFrame.FrameWidth = 2;
                       BorderFrame.FrameHeight = 3;
                        BorderRenderOffSet = new Point( -1, -1 );
                    }
                    break;
                case TileRoundState.LeftUp:
                    {
                        BorderFrame.FrameX = 1;
                        BorderFrame.FrameY = 1;
                         BorderFrame.FrameWidth = 2;
                       BorderFrame.FrameHeight = 2;
                        BorderRenderOffSet = new Point( 0, 0 );
                    }
                    break;
                case TileRoundState.DownLeft:
                    {
                        BorderFrame.FrameX = 1;
                        BorderFrame.FrameY = 0;
                         BorderFrame.FrameWidth = 2;
                       BorderFrame.FrameHeight = 2;
                        BorderRenderOffSet = new Point( 0, -1 );
                    }
                    break;
                case TileRoundState.RightDown:
                    {
                        BorderFrame.FrameX = 0;
                        BorderFrame.FrameY = 0;
                         BorderFrame.FrameWidth = 2;
                       BorderFrame.FrameHeight = 2;
                        BorderRenderOffSet = new Point( -1, -1 );
                    }
                    break;
                case TileRoundState.UpRight:
                    {
                        BorderFrame.FrameX = 0;
                        BorderFrame.FrameY = 1;
                         BorderFrame.FrameWidth = 2;
                       BorderFrame.FrameHeight = 2;
                        BorderRenderOffSet = new Point( -1, 0 );
                    }
                    break;
                case TileRoundState.UpDown:
                    {
                        BorderFrame.FrameX = 0;
                        BorderFrame.FrameY = 1;
                         BorderFrame.FrameWidth = 3;
                       BorderFrame.FrameHeight = 1;
                        BorderRenderOffSet = new Point( -1, 0 );
                    }
                    break;
                case TileRoundState.LeftRight:
                    {
                        BorderFrame.FrameX = 1;
                        BorderFrame.FrameY = 0;
                         BorderFrame.FrameWidth = 1;
                       BorderFrame.FrameHeight = 3;
                        BorderRenderOffSet = new Point( 0, -1 );
                    }
                    break;
                case TileRoundState.DownLeftUp:
                    {
                        BorderFrame.FrameX = 1;
                        BorderFrame.FrameY = 1;
                         BorderFrame.FrameWidth = 2;
                       BorderFrame.FrameHeight = 1;
                        BorderRenderOffSet = new Point( 0, 0 );
                    }
                    break;
                case TileRoundState.UpRightDown:
                    {
                        BorderFrame.FrameX = 0;
                        BorderFrame.FrameY = 1;
                         BorderFrame.FrameWidth = 2;
                        BorderFrame.FrameHeight = 1;
                        BorderRenderOffSet = new Point( -1, 0 );
                    }
                    break;
                case TileRoundState.RightDownLeft:
                    {
                        BorderFrame.FrameX = 1;
                        BorderFrame.FrameY = 0;
                         BorderFrame.FrameWidth = 1;
                       BorderFrame.FrameHeight = 2;
                        BorderRenderOffSet = new Point( 0, -1 );
                    }
                    break;
                case TileRoundState.LeftUpRight:
                    {
                        BorderFrame.FrameX = 1;
                        BorderFrame.FrameY = 1;
                         BorderFrame.FrameWidth = 1;
                       BorderFrame.FrameHeight = 2;
                        BorderRenderOffSet = new Point( 0, 0 );
                    }
                    break;
            }
            //最后为了统一贴图格式, 把边框和物块放同一个文件里, 边框紧挨在纹理的右侧.
            BorderFrame.FrameX += LoopTextureWidth / Tile.TileMap.GridSize;
        }

       /// <summary>
       /// 进行一次物块边框选取起点随机刷新.
       /// <para>[!] 9个元素的排列组合有很多, 所以我觉得不用更多了.</para>
       /// </summary>
        public void RefreshBorderOriginFrame( )
        {
            BorderOriginFrame.FrameX = LoopTextureWidth / Tile.TileMap.GridSize + new Random( ).Next( 3 ) * 3;
            BorderOriginFrame.FrameY = new Random( ).Next( 3 ) * 3;

        }

        /// <summary>
        /// 进行一次对周围物块的存在状态检测.
        /// </summary>
        public void RefreshRoundState( )
        {
            int _leftIndex = CoordinateX - 1;
            if ( _leftIndex < 0 )
                _leftIndex = 0;
            int _rightIndex = CoordinateX + 1;
            if ( _rightIndex > Tile.TileMap.Width - 1 )
                _rightIndex = Tile.TileMap.Width - 1;
            int _upIndex = CoordinateY - 1;
            if ( _upIndex < 0 )
                _upIndex = 0;
            int _downIndex = CoordinateY + 1;
            if ( _downIndex > Tile.TileMap.Height - 1 )
                _downIndex = Tile.TileMap.Height - 1;
            bool down = !Tile.TileMap.Tile[ CoordinateX, _downIndex ].Empty;
            bool left = !Tile.TileMap.Tile[ _leftIndex, CoordinateY ].Empty;
            bool right = !Tile.TileMap.Tile[ _rightIndex, CoordinateY ].Empty;
            bool up = !Tile.TileMap.Tile[ CoordinateX, _upIndex ].Empty;
            int result = 0;
            result += down ? 1 : 0;
            result += right ? 2 : 0;
            result += up ? 4 : 0;
            result += left ? 8 : 0;
            RoundState = (TileRoundState)result;
        }

    }
}