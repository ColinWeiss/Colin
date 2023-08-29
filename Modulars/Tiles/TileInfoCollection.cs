using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 瓦片信息集合.
    /// </summary>
    [Serializable]
    public struct TileInfoCollection
    {
        public int Width { get; }
        public int Height { get; }

        public int Length => _tiles.Length;

        private TileInfo[] _tiles;

        public ref TileInfo this[int index] => ref _tiles[index];
        public ref TileInfo this[int x, int y] => ref _tiles[x + y * Width];

        public TileInfoCollection(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new TileInfo[Width * Height];
            TileInfo _emptyTile = new TileInfo();
            _emptyTile.Empty = true;
            Span<TileInfo> _map = _tiles;
            _map.Fill(_emptyTile);
        }
        public TileInfoCollection(Point size)
        {
            Width = size.X;
            Height = size.Y;
            _tiles = new TileInfo[Width * Height];
            TileInfo _emptyTile = new TileInfo();
            _emptyTile.Empty = true;
            Span<TileInfo> _map = _tiles;
            _map.Fill(_emptyTile);
        }

        internal void CreateTileDefaultInfo(int coordinateX, int coordinateY)
        {
            int id = coordinateX + coordinateY * Width;
            if (_tiles[id].Empty)
            {
                _tiles[id].CoordinateX = coordinateX;
                _tiles[id].CoordinateY = coordinateY;
                _tiles[id].ID = id;
                _tiles[id].Empty = false;
            }
        }

        internal void DeleteTileInfo(int coordinateX, int coordinateY)
        {
            int id = coordinateX + coordinateY * Width;
            if (!_tiles[id].Empty)
            {
                _tiles[id].ID = 0;
                _tiles[id].Empty = true;
            }
        }
    }
}