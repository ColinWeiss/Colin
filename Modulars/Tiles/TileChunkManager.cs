using Colin.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 物块区块管理器.
    /// </summary>
    public class TileChunkManager
    {
        public Tile Tile { get; internal set; }

        /// <summary>
        /// 区块字典.
        /// <br>用于存储当前已被加载进内存的区块.</br>
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

        public Queue<Point> UpdateQueue = new Queue<Point>();

        public void DoUpdate( )
        {
            TileChunk current;
            for(int count = 0; count < Chunks.Count; count++)
            {
                current = Chunks.ElementAt( count ).Value;

                if(current.Importance)
                    continue;
                current.ActiveTimer -= Time.UnscaledDeltaTime;
                if(current.ActiveTimer <= 0)
                {
                    Chunks.Remove( current.Coord );
                    count--;
                }
            }
        }
        /// <summary>
        /// 在指定坐标新注册一个区块.
        /// </summary>
        public void RegisterChunk( int x, int y )
        {
            TileChunk chunk = new TileChunk();
            chunk.CoordX = x;
            chunk.CoordY = y;
            chunk.Tile = Tile;
            chunk.Create();
            chunk.Manager = this;
            Chunks.Add( chunk.Coord, chunk );
        }
        public void LoadChunk( )
        {

        }
        public void UnLoadChunk( Point coord )
        {

        }

    }
}