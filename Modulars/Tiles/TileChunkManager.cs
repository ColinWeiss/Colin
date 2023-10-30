using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Tiles
{
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

        /// <summary>
        /// 此处存放需要处理卸载操作的区块.
        /// <br>请于业务逻辑中处理本队列内容.</br>
        /// </summary>
        public Queue<TileChunk> UnLoadQueue = new Queue<TileChunk>();

        public void DoUpdate()
        {
            TileChunk current;
            for(int count = 0; count < Chunks.Count; count++)
            {
                current = Chunks.ElementAt( count ).Value;
                if( current.Importance )
                    continue;
                current.ActiveTimer -= Time.UnscaledDeltaTime;
                if(current.ActiveTimer <= 0)
                {
                    UnLoadQueue.Enqueue( current );
                }
            }
        }
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
                chunk.LoadChunk( path );
                chunk.CoordX = x;
                chunk.CoordY = y;
                chunk.Tile = Tile;
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