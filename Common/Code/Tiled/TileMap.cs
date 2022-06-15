using Colin.Common.Code.Fecs;
using Colin.Common.Code.Physics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Common.Code.Tiled
{
    /// <summary>
    /// 瓦片地图.
    /// </summary>
    public class TileMap : Entity
    {
        public World world;
        public World World => world;

        public virtual int GridSize => 16;

        private Tile[ , ] tile;
        public Tile[ , ] Tile => tile;

        public int Width;

        public int Height;

        /// <summary>
        /// 对地图进行初始化操作.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void InitializeMap( int width, int height )
        {
            Width = width;
            Height = height;
            tile = new Tile[ Width, Height ];
            for ( int _xIndex = 0; _xIndex < Width; _xIndex++ )
                for ( int _yIndex = 0; _yIndex < Height; _yIndex++ )
                    ClearGrid( _xIndex, _yIndex );
        }

        public Rectangle UpdateScope;

        public void Update( GameTime gameTime )
        {
            for ( int _updateCountX = UpdateScope.X; _updateCountX < UpdateScope.Width; _updateCountX++ )
            {
                for ( int _updateCountY = UpdateScope.Y; _updateCountY < UpdateScope.Height; _updateCountY++ )
                {
                    Tile[ _updateCountX, _updateCountY ].DoUpdate( );
                }
            }
        }

        public void Render( GameTime gameTime )
        {
            for ( int _updateCountX = UpdateScope.X; _updateCountX < UpdateScope.Width; _updateCountX++ )
            {
                for ( int _updateCountY = UpdateScope.Y; _updateCountY < UpdateScope.Height; _updateCountY++ )
                {
                    Tile[ _updateCountX, _updateCountY ].RenderTexture( );
                }
            }
            for ( int _updateCountX = UpdateScope.X; _updateCountX < UpdateScope.Width; _updateCountX++ )
            {
                for ( int _updateCountY = UpdateScope.Y; _updateCountY < UpdateScope.Height; _updateCountY++ )
                {
                    Tile[ _updateCountX, _updateCountY ].RenderBorder( );
                }
            }
        }

        /// <summary>
        /// 清空指定坐标的物块格内的物块.
        /// </summary>
        /// <param name="coordinateX">物块格的横坐标.</param>
        /// <param name="coordinateY">物块格的纵坐标.</param>
        public void ClearGrid( int coordinateX, int coordinateY )
        {
            tile[ coordinateX, coordinateY ].DoInitialize( );
            tile[ coordinateX, coordinateY ].Empty = true;
            tile[ coordinateX, coordinateY ].tileMap = this;
        }

        /// <summary>
        /// 清空指定坐标的物块格内的物块.
        /// </summary>
        /// <param name="coordinate">坐标.</param>
        public void ClearGrid( Point coordinate )
        {
            ClearTile( coordinate.X, coordinate.Y );
        }
    }
}