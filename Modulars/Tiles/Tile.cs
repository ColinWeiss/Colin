﻿using System.Text.Json;

namespace Colin.Core.Modulars.Tiles
{
    public class Tile : ISceneModule
    {
        private int _depth = 0;
        public int Depth => _depth;

        private Scene _scene;
        public Scene Scene
        {
            get => _scene;
            set => _scene = value;
        }

        private bool _enable = false;
        public bool Enable
        {
            get => _enable;
            set => _enable = value;
        }

        /// <summary>
        /// 区块字典.
        /// <br>用于存储当前已被加载进内存的区块.</br>
        /// <br>键: 区块坐标.</br>
        /// <br>值: 区块.</br>
        /// </summary>
        public Dictionary<Point, TileChunk> Chunks = new Dictionary<Point, TileChunk>();
        public TileChunk GetChunk( int x , int y )
        {
            if(Chunks.TryGetValue( new Point( x, y ), out TileChunk chunk ))
                return chunk;
            else
                return null;
        }
        public TileChunk GetChunk( Point coord )
        {
            return GetChunk( coord.X , coord.Y );
        }

        /// <summary>
        /// 在指定坐标新注册一个区块.
        /// </summary>
        public void RegisterChunk( int x , int y )
        {
            TileChunk chunk = new TileChunk();
            chunk.CoordX = x;
            chunk.CoordY = y;
            chunk.Tile = this;
            chunk.Create();
            Chunks.Add( chunk.Coord , chunk );
        }

        public Tile()
        {
            TileInfo._null.Tile = this;
        }

        /// <summary>
        /// 创建物块模块.
        /// </summary>
        /// <param name="depth"></param>
        public void Create( int depth )
        {
            _depth = depth;
        }
        public void DoInitialize() { }
        public void Start() { }
        public void DoUpdate( GameTime time ) { }
        public void Dispose()
        {
        }

        /// <summary>
        /// 索引器: 根据世界物块坐标获取指定位置的物块.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public ref TileInfo this[int x , int y , int z]
        {
            get
            {
                TileChunk targetChunk = GetChunk( x / TileOption.ChunkWidth, y / TileOption.ChunkHeight );
                if(targetChunk is not null)
                    return ref targetChunk[x % TileOption.ChunkWidth, y % TileOption.ChunkHeight, z];
                else
                    return ref TileInfo.Null;
            }
        }
        /// <summary>
        /// 索引器: 根据世界物块坐标获取指定位置的物块.
        /// </summary>
        public ref TileInfo this[Point3 coord]
        {
            get
            {
                TileChunk targetChunk = GetChunk( coord.X / TileOption.ChunkWidth, coord.Y / TileOption.ChunkHeight );
                if(targetChunk is not null)
                    return ref targetChunk[coord.X % TileOption.ChunkWidth, coord.Y % TileOption.ChunkHeight, coord.Z];
                else
                    return ref TileInfo.Null;
            }
        }

        /// <summary>
        /// 使用世界物块坐标在指定位置放置物块.
        /// </summary>
        public void Place<T>( int x, int y, int z ) where T : TileBehavior, new()
        {
            TileChunk targetChunk = GetChunk( x / TileOption.ChunkWidth , y / TileOption.ChunkHeight );
            if( targetChunk is not null )
                targetChunk.Place<T>( x % TileOption.ChunkWidth , y % TileOption.ChunkHeight , z );
        }

        /// <summary>
        /// 使用世界物块坐标在指定位置放置物块.
        /// </summary>
        public void Place( TileBehavior behavior, int x, int y, int z )
        {
            TileChunk targetChunk = GetChunk( x / TileOption.ChunkWidth, y / TileOption.ChunkHeight );
            if(targetChunk is not null)
                targetChunk.Place( behavior , x % TileOption.ChunkWidth, y % TileOption.ChunkHeight, z );
        }

        /// <summary>
        /// 使用世界物块坐标破坏指定位置的物块.
        /// </summary>
        public void Destruction( int x, int y, int z )
        {
            TileChunk targetChunk = GetChunk( x / TileOption.ChunkWidth, y / TileOption.ChunkHeight );
            if(targetChunk is not null)
                targetChunk.Destruction( x % TileOption.ChunkWidth, y % TileOption.ChunkHeight, z );
        }
    }
}