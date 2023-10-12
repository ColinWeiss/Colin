using Colin.Core.Assets.GameAssets;
using DeltaMachine.Core.GameContents.GameSystems;
using System;
using System.Reflection;
using System.Text.Json;

namespace Colin.Core.Modulars.Tiles
{
    /// <summary>
    /// 物块区块类.
    /// </summary>
    public class TileChunk
    {
        /// <summary>
        /// 获取物块区块所属的世界物块模块.
        /// </summary>
        public Tile Tile { get; internal set; } = null;

        /// <summary>
        /// 获取区块的宽度.
        /// <br>它与 <see cref="TileOption.ChunkWidth"/> 的值相等.</br>
        /// </summary>
        public int Width => TileOption.ChunkWidth;

        /// <summary>
        /// 获取区块的宽度.
        /// <br>它与 <see cref="TileOption.ChunkHeight"/> 的值相等.</br>
        /// </summary>
        public int Height => TileOption.ChunkHeight;

        /// <summary>
        /// 获取区块的深度.
        /// <br>它与 <see cref="Tile.Depth"/> 的值相等.</br>
        /// </summary>
        public int Depth => Tile.Depth;

        /// <summary>
        /// 指示区块的横坐标.
        /// </summary>
        public int CoordX;

        /// <summary>
        /// 指示区块的纵坐标.
        /// </summary>
        public int CoordY;

        private TileChunk temp;
        public TileChunk Top
        {
            get
            {
                temp = GetOffset( 0 , -1 );
                if(temp is not null)
                    return temp;
                else
                    return null;
            }
        }
        public TileChunk Bottom
        {
            get
            {
                temp = GetOffset( 0, 1 );
                if(temp is not null)
                    return temp;
                else
                    return null;
            }
        }
        public TileChunk Left
        {
            get
            {
                temp = GetOffset( -1, 0 );
                if(temp is not null)
                    return temp;
                else
                    return null;
            }
        }

        public TileChunk Right
        {
            get
            {
                temp = GetOffset( 1, 0 );
                if(temp is not null)
                    return temp;
                else
                    return null;
            }
        }

        /// <summary>
        /// 根据指定偏移量获取相对于该区块坐标偏移的区块.
        /// </summary>
        public TileChunk GetOffset( int offsetX , int offsetY )
        {
            return Tile.GetChunk( CoordX + offsetX , CoordY + offsetY );
        }

        /// <summary>
        /// 区块内的物块信息.
        /// </summary>
        public TileInfo[] Infos;

        /// <summary>
        /// 索引器: 根据索引获取物块信息.
        /// </summary>
        public ref TileInfo this[int index] => ref Infos[index];

        /// <summary>
        /// 索引器: 根据索引获取物块信息.
        /// </summary>
        public ref TileInfo this[int x, int y, int z]
        {
            get
            {
                if(x < 0)
                    x = Width + x;
                if(y < 0)
                    y = Height + y;
                return ref Infos[z * Width * Height + x + y * Width];
            }
        }

        public TileChunk()
        {
            CoordX = 0;
            CoordY = 0;
            Infos = new TileInfo[1];
        }

        /// <summary>
        /// 创建一个区块.
        /// </summary>
        /// <param name="depth"></param>
        public void Create( )
        {
            Infos = new TileInfo[ Width * Height * Depth ];
            int coord;
            for(int count = 0; count < Infos.Length; count++)
            {
                coord = count % (Width * Height);
                Infos[count] = new TileInfo();
                Infos[count].Tile = Tile;
                Infos[count].ID = count;
                Infos[count].CoordX = coord % Width;
                Infos[count].CoordY = coord / Width;
                Infos[count].CoordZ = count / (Width * Height);
            }
        }

        /// <summary>
        /// 根据坐标向区块内放置物块.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Place<T>( int x, int y, int z ) where T : TileBehavior, new()
        {
            if(this[x, y, z].Empty)
            {
                CreateTileDefaultInfo( x, y, z );
                Set<T>( x, y, z );
                this[x, y, z].Behavior.OnPlace( ref this[x, y, z] );
                this[x, y, z].Behavior.DoRefresh( ref this[x, y, z], 1 );
            }
        }

        /// <summary>
        /// 根据索引向区块内放置物块.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Place<T>( int index ) where T : TileBehavior, new()
        {
            CreateTileDefaultInfo( index );
            Set<T>( index );
            this[index].Behavior.OnPlace( ref this[index] );
            this[index].Behavior.DoRefresh( ref this[index], 1 );
        }

        /// <summary>
        /// 根据坐标向区块内放置物块.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Place( TileBehavior behavior, int x, int y, int z )
        {
            CreateTileDefaultInfo( x, y, z );
            Set( behavior, x, y, z );
            this[x, y, z].Behavior.OnPlace( ref this[x, y, z] );
            this[x, y, z].Behavior.DoRefresh( ref this[x, y, z], 1 );
        }

        /// <summary>
        /// 根据索引向区块内放置物块.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Place( TileBehavior behavior, int index )
        {
            CreateTileDefaultInfo( index );
            Set( behavior, index );
            this[index].Behavior.Tile = Tile;
            this[index].Behavior.OnPlace( ref this[index] );
            this[index].Behavior.DoRefresh( ref this[index], 1 );
        }

        /// <summary>
        /// 根据坐标设置区块内物块.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set<T>( int x, int y, int z ) where T : TileBehavior, new()
        {
            this[x, y, z].Behavior = TileAssets.Get<T>();
            this[x, y, z].Behavior.Tile = Tile;
            this[x, y, z].Behavior.OnInitialize( ref this[x, y, z] );
        }

        /// <summary>
        /// 根据索引设置区块内物块.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set<T>( int index ) where T : TileBehavior, new()
        {
            this[index].Behavior = TileAssets.Get<T>();
            this[index].Behavior.Tile = Tile;
            this[index].Behavior.OnInitialize( ref this[index] );
        }

        /// <summary>
        /// 根据坐标设置区块内物块.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set( TileBehavior behavior, int x, int y, int z )
        {
            this[x, y, z].Behavior = behavior;
            this[x, y, z].Behavior.Tile = Tile;
            this[x, y, z].Behavior.OnInitialize( ref this[x, y, z] );
        }

        /// <summary>
        /// 根据索引设置区块内物块.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set( TileBehavior behavior, int index )
        {
            this[index].Behavior = behavior;
            this[index].Behavior.Tile = Tile;
            this[index].Behavior.OnInitialize( ref this[index] );
        }

        /// <summary>
        /// 破坏指定坐标处的物块.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Destruction( int x, int y, int z )
        {
            if(!this[x, y, z].Empty)
            {
                DeleteTileInfo( x, y, z );
                this[x, y, z].Behavior.DoDestruction( ref this[x, y, z] );
                this[x, y, z].Behavior.DoRefresh( ref this[x, y, z] );
                this[x, y, z].Behavior = null;
                this[x, y, z].Scripts.Clear();
            }
        }

        /// <summary>
        /// 在指定坐标处创建物块信息.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        internal void CreateTileDefaultInfo( int x, int y, int z )
        {
            int id = (z * Height * Width) + (x + y * Width);
            Infos[id].Tile = Tile;
            Infos[id].CoordX = x;
            Infos[id].CoordY = y;
            Infos[id].CoordZ = z;
            Infos[id].ID = id;
            Infos[id].Empty = false;
        }

        /// <summary>
        /// 在指定索引处创建物块信息.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        internal void CreateTileDefaultInfo( int index )
        {
            int id = index;
            int coord = index % (Width * Height);
            Infos[id].Tile = Tile;
            Infos[id].Empty = false;
            Infos[id].ID = id;
            Infos[id].CoordX = coord % Width;
            Infos[id].CoordY = coord / Width;
            Infos[id].CoordZ = index / (Width * Height);
        }

        /// <summary>
        /// 在指定坐标处删除物块信息.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        internal void DeleteTileInfo( int x, int y, int z )
        {
            int id = (z * Width * Height) + (x + y * Width);
            Infos[id].Collision = TileCollision.Passable;
            Infos[id].Empty = true;
        }

        /// <summary>
        /// 在指定索引处删除物块信息.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        internal void DeleteTileInfo( int index )
        {
            int id = index;
            Infos[id].Empty = true;
            Infos[id].Collision = TileCollision.Passable;
        }

        internal void LoadStep( BinaryReader reader )
        {
        }

        internal void SaveStep( BinaryWriter writer )
        {
        }
    }
}