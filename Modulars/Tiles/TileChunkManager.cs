﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Tiles
{
    public class TileChunkManager
    {
        public Tile Tile { get; internal set; }

        /// <summary>
        /// 区块字典.
        /// <br>用于存储当前已被加载的区块.</br>
        /// <br>键: 区块坐标.</br>
        /// <br>值: 区块.</br>
        /// </summary>
        public Dictionary<Point, TileChunk> Chunks = new Dictionary<Point, TileChunk>();
        public TileChunk GetChunk( int x, int y )
        {
            if(Chunks.TryGetValue( new Point( x, y ), out TileChunk chunk ))
                return chunk;
            else
                return null;
        }
        public TileChunk GetChunk( Point coord )
        {
            return GetChunk( coord.X, coord.Y );
        }
        /// <summary>
        /// 从世界坐标获取区块坐标与物块位于区块内的坐标.
        /// </summary>
        /// <param name="coordX"></param>
        /// <param name="coordY"></param>
        /// <returns></returns>
        public (Point, Point) GetConvertWorldCoord( int coordX, int coordY )
        {
            int chunkCoordX = coordX >= 0 ? coordX / TileOption.ChunkWidth : (coordX + 1) / TileOption.ChunkWidth - 1;
            int chunkCoordY = coordY >= 0 ? coordY / TileOption.ChunkHeight : (coordY + 1) / TileOption.ChunkHeight - 1;
            int tileCoordX = coordX >= 0 ? coordX % TileOption.ChunkWidth : (coordX + 1) % TileOption.ChunkWidth + (TileOption.ChunkWidth - 1);
            int tileCoordY = coordY >= 0 ? coordY % TileOption.ChunkHeight : (coordY + 1) % TileOption.ChunkHeight + (TileOption.ChunkHeight - 1);
            return (new Point( chunkCoordX, chunkCoordY ), new Point( tileCoordX, tileCoordY ));
        }
        public TileChunk GetChunkForTileCoord( int tileCoordX, int tileCoordY )
        {
            int indexX = tileCoordX >= 0 ? tileCoordX / TileOption.ChunkWidth : (tileCoordX + 1) / TileOption.ChunkWidth - 1;
            int indexY = tileCoordY >= 0 ? tileCoordY / TileOption.ChunkHeight : (tileCoordY + 1) / TileOption.ChunkHeight - 1;
            return GetChunk( indexX, indexY );
        }
        public ref TileInfo GetTile( int x, int y, int z )
        {
            int indexX = x >= 0 ? x % TileOption.ChunkWidth : ((x + 1) % TileOption.ChunkWidth) + (TileOption.ChunkWidth - 1);
            int indexY = y >= 0 ? y % TileOption.ChunkHeight : ((y + 1) % TileOption.ChunkHeight) + (TileOption.ChunkHeight - 1);
            TileChunk target = GetChunkForTileCoord( x, y );
            if(target is not null)
                return ref target[indexX, indexY, z];
            else
                return ref TileInfo.Null;
        }

        public void DoUpdate() { }
        /// <summary>
        /// 在指定坐标新创建一个空区块.
        /// <br>[!] 这个行为会强制覆盖指定坐标的区块, 无论它是否已经加载.</br>
        /// </summary>
        public void CreateChunk( int x, int y )
        {
            TileChunk chunk = new TileChunk();
            chunk.CoordX = x;
            chunk.CoordY = y;
            chunk.Tile = Tile;
            chunk.Create();
            chunk.Manager = this;
            Chunks.Add( chunk.Coord, chunk );
        }
        public void LoadChunk( int x, int y, string path )
        {
            if(File.Exists( path ))
            {
                TileChunk chunk = new TileChunk();
                chunk.Tile = Tile;
                chunk.LoadChunk( path );
                chunk.CoordX = x;
                chunk.CoordY = y;
                chunk.Manager = this;
                Chunks.Add( chunk.Coord, chunk );
            }
            else
                EngineConsole.WriteLine( ConsoleTextType.Error, string.Concat( "加载 (", x, ",", y, ") 处的区块时出现异常." ) );
        }
        public void SaveChunk( int x, int y, string path )
        {
            TileChunk chunk = GetChunk( x, y );
            if(chunk is not null)
                chunk.SaveChunk( path );
            else
                EngineConsole.WriteLine( ConsoleTextType.Error, string.Concat( "卸载 (", x, ",", y, ") 处的区块时出现异常." ) );
        }
    }
}