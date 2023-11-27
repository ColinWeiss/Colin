using System.Text.Json;

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

        public TileChunkManager Manager { get; }

        public Tile()
        {
            TileInfo._null.Tile = this;
            Manager = new TileChunkManager();
            Manager.Tile = this;
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
        public void DoUpdate( GameTime time )
        {
            Manager.DoUpdate();
        }
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
        public ref TileInfo this[int x, int y, int z] => ref GetTile( x, y, z );
        /// <summary>
        /// 索引器: 根据世界物块坐标获取指定位置的物块.
        /// </summary>
        public ref TileInfo this[Point3 coord] => ref GetTile( coord.X, coord.Y, coord.Z );
        public void RegisterChunk( int x, int y ) => Manager.CreateChunk( x, y );
        public TileChunk GetChunk( int x, int y ) => Manager.GetChunk( x, y );
        public bool HasChunk( int x, int y ) => Manager.HasChunk( x , y );
        public bool HasChunk( Point coord ) => Manager.HasChunk( coord );
        /// <summary>
        /// 从世界坐标获取区块坐标与物块位于区块内的坐标.
        /// </summary>
        /// <param name="coordX"></param>
        /// <param name="coordY"></param>
        /// <returns></returns>
        public (Point, Point) ConvertWorldCoordToTileCoord( int coordX, int coordY ) => Manager.ConvertWorldCoordToTileCoord( coordX, coordY );
        public Point ConvertPositionToWorldCoord( Vector2 position ) => Manager.ConvertPositionToWorldCoord( position );
        public TileChunk GetChunkForWorldCoord( int worldCoordX, int worldCoordY ) => Manager.GetChunkForWorldCoord( worldCoordX, worldCoordY );
        public ref TileInfo GetTile( int x, int y, int z ) => ref Manager.GetTile( x , y , z );
        /// <summary>
        /// 使用世界物块坐标在指定位置放置物块.
        /// </summary>
        public void Place<T>( int x, int y, int z ) where T : TileBehavior, new()
        {
            var coords = ConvertWorldCoordToTileCoord( x, y );
            TileChunk targetChunk = GetChunk( coords.Item1.X, coords.Item1.Y );
            if(targetChunk is not null)
                targetChunk.Place<T>( coords.Item2.X, coords.Item2.Y, z );
        }

        /// <summary>
        /// 使用世界物块坐标在指定位置放置物块.
        /// </summary>
        public void Place( TileBehavior behavior, int x, int y, int z )
        {
            var coords = ConvertWorldCoordToTileCoord( x, y );
            TileChunk targetChunk = GetChunk( coords.Item1.X, coords.Item1.Y );
            if(targetChunk is not null)
                targetChunk.Place( behavior, coords.Item2.X, coords.Item2.Y, z );
        }

        /// <summary>
        /// 使用世界物块坐标破坏指定位置的物块.
        /// </summary>
        public void Destruction( int x, int y, int z )
        {
            var coords = ConvertWorldCoordToTileCoord( x, y );
            TileChunk targetChunk = GetChunk( coords.Item1.X, coords.Item1.Y );
            if(targetChunk is not null)
                targetChunk.Destruction( coords.Item2.X, coords.Item2.Y, z );
        }
    }
}