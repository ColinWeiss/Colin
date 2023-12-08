using Colin.Core.Resources;
using System.Threading.Tasks;

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
        /// 获取物块区块所属的物块刷新模块.
        /// </summary>
        public TileRefresher TileRefresher => Tile.Scene.GetModule<TileRefresher>();

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

        /// <summary>
        /// 指示区块的量子层.
        /// <br>同一二维位置可以存在不同量子层的区块，用于无缝子世界</br>
        /// </summary>
        public int QuantumLayer;

        public Point Coord => new Point( CoordX, CoordY );

        private TileChunk temp;
        public TileChunk Top
        {
            get
            {
                temp = GetOffset( 0, -1 );
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
        public TileChunk GetOffset( int offsetX, int offsetY )
        {
            return Tile.GetChunk( CoordX + offsetX, CoordY + offsetY );
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
        /// 执行初始化操作.
        /// <br>该操作会令区块初始化其整个物块信息的数组.</br>
        /// </summary>
        public void DoInitialize()
        {
            Infos = new TileInfo[Width * Height * Depth];
            for(int count = 0; count < Infos.Length; count++)
                CreateInfo( count );
        }

        /// <summary>
        /// 清除区块.
        /// </summary>
        public void Clear() => DoInitialize();

        /// <summary>
        /// 在指定索引处创建空物块信息.
        /// </summary>
        public void CreateInfo( int index )
        {
            Infos[index] = new TileInfo();
            Infos[index].Tile = Tile;
            Infos[index].Chunk = this;
            Infos[index].Empty = true;
            Infos[index].Index = index;
            Infos[index].Scripts = new Dictionary<Type, TileScript>();
        }
        /// <summary>
        /// 在指定坐标处创建物块信息.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void CreateInfo( int x, int y, int z ) => CreateInfo( (z * Height * Width) + x + y * Width );

        /// <summary>
        /// 根据索引和指定类型设置区块内物块的 <see cref="TileBehavior"/> 并执行其初始化.
        /// </summary>
        public void Set<T>( int index ) where T : TileBehavior, new()
        {
            ref TileInfo info = ref this[index];
            info.Behavior = CodeResources<TileBehavior>.Get<T>();
            info.Behavior.Tile = Tile;
            info.Behavior.OnInitialize( ref info );
        }
        /// <summary>
        /// 根据坐标和指定类型设置区块内物块的 <see cref="TileBehavior"/> 并执行其初始化.
        /// </summary>
        public void Set<T>( int x, int y, int z ) where T : TileBehavior, new() => Set<T>( z * Width * Height + x + y * Width );
        /// <summary>
        /// 根据索引和引用设置区块内物块的 <see cref="TileBehavior"/> 并执行其初始化.
        /// </summary>
        public void Set( TileBehavior behavior, int index )
        {
            ref TileInfo info = ref this[index];
            info.Behavior = behavior;
            info.Behavior.Tile = Tile;
            info.Behavior.OnInitialize( ref this[index] );
        }
        /// <summary>
        /// 根据坐标和引用设置区块内物块的 <see cref="TileBehavior"/> 并执行其初始化.
        /// </summary>
        public void Set( TileBehavior behavior, int x, int y, int z ) => Set( behavior, z * Width * Height + x + y * Width );

        /// <summary>
        /// 根据索引和指定类型放置物块.
        /// </summary>
        /// <param name="doEvent">指示是否触发放置事件.</param>
        /// <param name="doRefresh">指示是否触发物块刷新事件.</param>
        public void Place<T>( int index, bool doEvent = true, bool doRefresh = true ) where T : TileBehavior, new()
        {
            ref TileInfo info = ref this[index];
            if(info.Empty)
            {
                info.Empty = false;
                Set<T>( index );
                if(doEvent)
                {
                    info.Behavior.OnPlace( ref info );
                    foreach(var script in info.Scripts.Values)
                        script.OnPlace();
                }
                if(doRefresh)
                {
                    TileRefresher.RefreshMark( info.WorldCoord3, 1 );
                }
            }
        }
        /// <summary>
        /// 根据坐标和指定类型放置物块.
        /// </summary>
        /// <param name="doEvent">指示是否触发放置事件.</param>
        /// <param name="doRefresh">指示是否触发物块刷新事件.</param>
        public void Place<T>( int x, int y, int z, bool doEvent = true, bool doRefresh = true ) where T : TileBehavior, new()
            => Place<T>( z * Width * Height + x + y * Width, doEvent, doRefresh );
        /// <summary>
        /// 根据索引和引用放置物块.
        /// </summary>
        /// <param name="doEvent">指示是否触发放置事件.</param>
        /// <param name="doRefresh">指示是否触发物块刷新事件.</param>
        public void Place( TileBehavior behavior, int index, bool doEvent = true, bool doRefresh = true )
        {
            ref TileInfo info = ref this[index];
            if(info.Empty)
            {
                info.Empty = false;
                Set( behavior, index );
                if(doEvent)
                {
                    info.Behavior.OnPlace( ref info );
                    foreach(var script in info.Scripts.Values)
                        script.OnPlace();
                }
                if(doRefresh)
                {
                    TileRefresher.RefreshMark( info.WorldCoord3, 1 );
                }
            }
        }
        /// <summary>
        /// 根据坐标向区块内放置物块.
        /// </summary>
        /// <param name="doEvent">指示是否触发放置事件.</param>
        /// <param name="doRefresh">指示是否触发物块刷新事件.</param>
        public void Place( TileBehavior behavior, int x, int y, int z, bool doEvent = true, bool doRefresh = true )
            => Place( behavior, z * Width * Height + x + y * Width, doEvent, doRefresh );

        /// <summary>
        /// 根据索引破坏区块内物块.
        /// </summary>
        /// <param name="doEvent">指示执行破坏时是否触发物块行为和脚本的破坏行为.</param>
        /// <param name="doRefresh">指示执行破坏时是否触发物块行为和脚本的刷新行为.</param>
        public void Destruction( int index, bool doEvent = true, bool doRefresh = true )
        {
            ref TileInfo info = ref this[index];
            if(!info.Empty)
            {
                info.Empty = true;
                if(doEvent)
                {
                    info.Behavior.OnDestruction( ref info );
                    foreach(var script in info.Scripts.Values)
                        script.OnDestruction();
                }
                if(doRefresh)
                {
                    TileRefresher.RefreshMark( info.WorldCoord3, 1 );
                }
            }
        }
        /// <summary>
        /// 根据坐标破坏区块内物块.
        /// </summary>
        /// <param name="doEvent">指示执行破坏时是否触发物块行为和脚本的破坏行为.</param>
        /// <param name="doRefresh">指示执行破坏时是否触发物块行为和脚本的刷新行为.</param>
        public void Destruction( int x, int y, int z, bool doEvent = true, bool doRefresh = true )
            => Destruction( z * Width * Height + x + y * Width, doEvent, doRefresh );

        public void LoadChunk( string path )
        {
            using(FileStream fileStream = new FileStream( path, FileMode.Open ))
            {
                using(BinaryReader reader = new BinaryReader( fileStream ))
                {
                    DoInitialize();
                    for(int count = 0; count < Infos.Length; count++)
                    {
                        Infos[count].LoadStep( reader );
                        if(!Infos[count].Empty)
                            if(CodeResources<TileBehavior>.HashMap.TryGetValue( reader.ReadInt32(), out string value ))
                            {
                                Set( CodeResources<TileBehavior>.Get( value ), count );
                                TileRefresher.RefreshMark(Infos[count].WorldCoord3, 0);
                            }
                    }
                }
            }
        }
        public async Task LoadChunkAsync( string path )
            => await Task.Run( () => LoadChunk( path ) );

        public void SaveChunk( string path )
        {
            using(FileStream fileStream = new FileStream( path, FileMode.Create ))
            {
                using(BinaryWriter writer = new BinaryWriter( fileStream ))
                {
                    TileBehavior behavior;
                    for(int count = 0; count < Infos.Length; count++)
                    {
                        behavior = Infos[count].Behavior;
                        Infos[count].SaveStep( writer );
                        if(!Infos[count].Empty && behavior is not null)
                            if(CodeResources<TileBehavior>.SerializeTable.TryGetValue( behavior.Identifier, out int value ))
                            {
                                writer.Write( value );
                            }
                    }
                }
            }
        }
        public async void SaveChunkAsync( string path )
            => await Task.Run( () => SaveChunk( path ) );
    }
}