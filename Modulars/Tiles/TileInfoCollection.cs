using System;
using System.Collections;
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

        public int Length => Tiles.Length;

        public TileInfo[] Tiles;

        public ref TileInfo this[int index] => ref Tiles[index];
        public ref TileInfo this[int x, int y] => ref Tiles[x + y * Width];

        public TileInfoCollection(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new TileInfo[Width * Height];
            TileInfo _emptyTile = new TileInfo();
            _emptyTile.Empty = true;
            Span<TileInfo> _map = Tiles;
            _map.Fill(_emptyTile);
        }
        public TileInfoCollection(Point size)
        {
            Width = size.X;
            Height = size.Y;
            Tiles = new TileInfo[Width * Height];
            TileInfo _emptyTile = new TileInfo();
            _emptyTile.Empty = true;
            Span<TileInfo> _map = Tiles;
            _map.Fill(_emptyTile);
        }

        internal void CreateTileDefaultInfo(int coordinateX, int coordinateY)
        {
            int id = coordinateX + coordinateY * Width;
            if (Tiles[id].Empty)
            {
                Tiles[id].CoordinateX = coordinateX;
                Tiles[id].CoordinateY = coordinateY;
                Tiles[id].ID = id;
                Tiles[id].Empty = false;
            }
        }

        internal void DeleteTileInfo(int coordinateX, int coordinateY)
        {
            int id = coordinateX + coordinateY * Width;
            if (!Tiles[id].Empty)
            {
                Tiles[id].ID = 0;
                Tiles[id].Collision = TileCollision.Passable;
                Tiles[id].Empty = true;
            }
        }
    }
}