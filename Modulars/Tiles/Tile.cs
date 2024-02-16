namespace Colin.Core.Modulars.Tiles
{
    public class Tile : ISceneModule
    {
        public int Depth { get; private set; }

        public Scene Scene { get; set; }

        private bool _enable = false;
        public bool Enable
        {
            get => _enable;
            set => _enable = value;
        }

        public Tile()
        {
            TileInfo._null.Tile = this;
        }

        /// <summary>
        /// 区块字典.
        /// <br>用于存储当前已被加载的区块.</br>
        /// <br>键: 区块坐标.</br>
        /// <br>值: 区块.</br>
        /// </summary>
        public Dictionary<Point, TileChunk> Chunks = new Dictionary<Point, TileChunk>();

        /// <summary>
        /// 物块指针字典, 允许你通过某个物块访问其他物块.
        /// </summary>
        public Dictionary<Point, Point> TilePointers = new Dictionary<Point, Point>();

        /// <summary>
        /// 创建物块模块.
        /// </summary>
        /// <param name="depth"></param>
        public void Create(int depth) => Depth = depth;

        public void DoInitialize() { }
        public void Start() { }
        public void DoUpdate(GameTime time)
        {

        }
        public void Dispose()
        {
        }

        /// <summary>
        /// 指示默认（玩家所处）的量子层.
        /// <br>在探索时拓展未知区块时会拓展同量子层的区块</br>
        /// </summary>
        public int QuantumLayer = 0;

        /// <summary>
        /// 从指定坐标获取物块区块对象.
        /// </summary>
        /// <returns>若成功获取, 返回对象; 否则返回 <see langword="null"/>.</returns>
        public TileChunk GetChunk(int x, int y)
        {
            if (Chunks.TryGetValue(new Point(x, y), out TileChunk chunk))
                return chunk;
            else
                return null;
        }
        /// <summary>
        /// 从指定坐标获取物块区块对象.
        /// </summary>
        /// <returns>若成功获取, 返回对象; 否则返回 <see langword="null"/>.</returns>
        public TileChunk GetChunk(Point coord) => GetChunk(coord.X, coord.Y);
        /// <summary>
        /// 从世界物块坐标获取其坐标所在的区块对象.
        /// </summary>
        /// <returns>若成功获取, 返回对象; 否则返回 <see langword="null"/>.</returns>
        public TileChunk GetChunkForWorldCoord(int worldCoordX, int worldCoordY)
        {
            int indexX = worldCoordX >= 0 ? worldCoordX / TileOption.ChunkWidth : (worldCoordX + 1) / TileOption.ChunkWidth - 1;
            int indexY = worldCoordY >= 0 ? worldCoordY / TileOption.ChunkHeight : (worldCoordY + 1) / TileOption.ChunkHeight - 1;
            return GetChunk(indexX, indexY);
        }
        /// <summary>
        /// 索引器: 根据世界物块坐标获取指定位置的物块.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public ref TileInfo this[int x, int y, int z]
        {
            get
            {
                int indexX = x >= 0 ? x % TileOption.ChunkWidth : ((x + 1) % TileOption.ChunkWidth) + (TileOption.ChunkWidth - 1);
                int indexY = y >= 0 ? y % TileOption.ChunkHeight : ((y + 1) % TileOption.ChunkHeight) + (TileOption.ChunkHeight - 1);
                TileChunk target = GetChunkForWorldCoord(x, y);
                if (target is not null)
                    return ref target[indexX, indexY, z];
                else
                    return ref TileInfo.Null;
            }
        }
        /// <summary>
        /// 索引器: 根据世界物块坐标获取指定位置的物块.
        /// </summary>
        public ref TileInfo this[Point3 coord] => ref this[coord.X, coord.Y, coord.Z];
        /// <summary>
        /// 判断指定坐标的区块是否存在.
        /// </summary>
        public bool HasChunk(int x, int y) => Chunks.ContainsKey(new Point(x, y));
        /// <summary>
        /// 判断指定坐标的区块是否存在.
        /// </summary>
        public bool HasChunk(Point coord) => Chunks.ContainsKey(coord);
        /// <summary>
        /// 从世界坐标获取区块坐标与物块区块坐标.
        /// </summary>
        public (Point cCoord, Point tCoord) GetCoords(int worldCoordX, int worldCoordY)
        {
            int chunkCoordX = worldCoordX >= 0 ? worldCoordX / TileOption.ChunkWidth : (worldCoordX + 1) / TileOption.ChunkWidth - 1;
            int chunkCoordY = worldCoordY >= 0 ? worldCoordY / TileOption.ChunkHeight : (worldCoordY + 1) / TileOption.ChunkHeight - 1;
            int tileCoordX = worldCoordX >= 0 ? worldCoordX % TileOption.ChunkWidth : (worldCoordX + 1) % TileOption.ChunkWidth + (TileOption.ChunkWidth - 1);
            int tileCoordY = worldCoordY >= 0 ? worldCoordY % TileOption.ChunkHeight : (worldCoordY + 1) % TileOption.ChunkHeight + (TileOption.ChunkHeight - 1);
            return (new Point(chunkCoordX, chunkCoordY), new Point(tileCoordX, tileCoordY));
        }
        /// <summary>
        /// 从指定位置获取世界物块坐标.
        /// </summary>
        public Point GetWorldCoordForPosition(Vector2 position)
        {
            int coordX = (int)Math.Floor(position.X / TileOption.TileWidth);
            int coordY = (int)Math.Floor(position.Y / TileOption.TileHeight);
            return new Point(coordX, coordY);
        }

        /// <summary>
        /// 使用世界物块坐标在指定位置放置物块.
        /// </summary>
        public bool Place<T>(int x, int y, int z, bool doEvent = true, bool doRefresh = true) where T : TileBehavior, new()
        {
            var coords = GetCoords(x, y);
            TileChunk targetChunk = GetChunk(coords.cCoord.X, coords.cCoord.Y);
            if (targetChunk is not null)
                return targetChunk.Place<T>(coords.tCoord.X, coords.tCoord.Y, z, doEvent, doRefresh);
            else
                return false;
        }
        /// <summary>
        /// 使用世界物块坐标在指定位置放置物块.
        /// </summary>
        public bool Place(TileBehavior behavior, int x, int y, int z, bool doEvent = true, bool doRefresh = true)
        {
            var coords = GetCoords(x, y);
            TileChunk targetChunk = GetChunk(coords.cCoord.X, coords.cCoord.Y);
            if (targetChunk is not null)
                return targetChunk.Place(behavior, coords.tCoord.X, coords.tCoord.Y, z, doEvent, doRefresh);
            else
                return false;
        }

        /// <summary>
        /// 使用世界物块坐标破坏指定位置的物块.
        /// </summary>
        public bool Destruction(int x, int y, int z, bool doEvent = true, bool doRefresh = true)
        {
            var coords = GetCoords(x, y);
            TileChunk targetChunk = GetChunk(coords.cCoord.X, coords.cCoord.Y);
            if (targetChunk is not null)
                return targetChunk.Destruction(coords.tCoord.X, coords.tCoord.Y, z, doEvent, doRefresh);
            else
                return false;
        }

        /// <summary>
        /// 在指定坐标新创建一个空区块.
        /// <br>[!] 这个行为会强制覆盖指定坐标的区块, 无论它是否已经加载.</br>
        /// </summary>
        public void CreateEmptyChunk(int x, int y, int? quantumLayer = null)
        {
            TileChunk chunk = new TileChunk( this );
            chunk.CoordX = x;
            chunk.CoordY = y;
            chunk.QuantumLayer = quantumLayer ?? QuantumLayer;
            chunk.DoInitialize();
            Chunks.Add(chunk.Coord, chunk);
        }

        /// <summary>
        /// 从指定路径的文件加载区块到指定坐标.
        /// </summary>
        public void LoadChunk(int x, int y, string path, int? quantumLayer = null)
        {
            if (File.Exists(path))
            {
                TileChunk chunk = new TileChunk( this);
                chunk.LoadChunk(path);
                chunk.CoordX = x;
                chunk.CoordY = y;
                chunk.QuantumLayer = quantumLayer ?? QuantumLayer;
                Chunks.Add(chunk.Coord, chunk);
            }
            else
                EngineConsole.WriteLine(ConsoleTextType.Error, string.Concat("加载 (", x, ",", y, ") 处的区块时出现异常."));
        }
        /// <summary>
        /// 保存指定坐标的区块至指定路径.
        /// </summary>
        public void SaveChunk(int x, int y, string path)
        {
            TileChunk chunk = GetChunk(x, y);
            if (chunk is not null)
                chunk.SaveChunk(path);
            else
                EngineConsole.WriteLine(ConsoleTextType.Error, string.Concat("卸载 (", x, ",", y, ") 处的区块时出现异常."));
        }
    }
}