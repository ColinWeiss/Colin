using Colin.Common.Code.Fecs;
using Colin.Common.Code.Physics.Dynamics;
using Microsoft.Xna.Framework;

namespace Colin.Common.Code.Tiled
{
    /// <summary>
    /// 瓦片地图.
    /// </summary>
    public class TileMap : Entity
    {
        public virtual int GridSize => 16;

        internal World _world;

        protected Tile[ , ] tiles;
        public Tile[ , ] Tiles => tiles;

        public int Width;

        public int Height;

        public TileMap( int width,int height, World world )
        {
            Width = width;
            Height = height;
            _world = world;
        }

        /// <summary>
        /// 对地图进行初始化操作.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void InitializeMap( )
        {
            tiles = new Tile[Width,Height];
            for( int _xIndex = 0; _xIndex < Width; _xIndex++ )
                for( int _yIndex = 0; _yIndex < Height; _yIndex++ )
                    ClearGrid(_xIndex,_yIndex);
        }

        public Rectangle UpdateScope;

        public void Update( GameTime gameTime )
        {
            UpdateSelf( );
            for( int _updateCountX = UpdateScope.X; _updateCountX < UpdateScope.Width; _updateCountX++ )
            {
                for( int _updateCountY = UpdateScope.Y; _updateCountY < UpdateScope.Height; _updateCountY++ )
                {
                    Tiles[_updateCountX,_updateCountY].DoUpdate( );
                }
            }
        }

        public void Render( GameTime gameTime )
        {
            for( int _updateCountY = UpdateScope.Y; _updateCountY < UpdateScope.Height; _updateCountY++ )
            {
                for( int _updateCountX = UpdateScope.X; _updateCountX < UpdateScope.Width; _updateCountX++ )
                {
                    Tiles[_updateCountX, _updateCountY].RenderTexture( );
                    Tiles[_updateCountX, _updateCountY].RenderBorder( );
                }
            }
        }

        public void PlaceTile( Tile tile,int coordinateX,int coordinateY,bool replace = false )
        {
            if( coordinateX < 0 || coordinateY < 0 || coordinateX > Width - 1 || coordinateY > Height - 1 )
                return;
            if( tiles[coordinateX,coordinateY].Empty || replace )
            {
                ClearGrid(coordinateX,coordinateY);
                tiles[coordinateX,coordinateY] = tile;
                tiles[coordinateX,coordinateY].TileMap = this;
                tiles[coordinateX,coordinateY].DoInitialize( );
                tiles[coordinateX,coordinateY].TileData.SetCoordinate(coordinateX,coordinateY);
                tiles[coordinateX,coordinateY].DoPlaceTileEvent( );
            }
        }

        /// <summary>
        /// 清空指定坐标的物块格内的物块.
        /// </summary>
        /// <param name="coordinateX">物块格的横坐标.</param>
        /// <param name="coordinateY">物块格的纵坐标.</param>
        public void ClearGrid( int coordinateX,int coordinateY )
        {
            tiles[coordinateX,coordinateY] = new Tile( );
            tiles[coordinateX,coordinateY].DoInitialize( );
            tiles[coordinateX,coordinateY].Empty = true;
            tiles[coordinateX,coordinateY].TileMap = this;
        }
    }
}