using Colin.Common.Code.Physics.Dynamics;
using Colin.Common.Code.Physics.Factories;
using Colin.Common.Code.Physics.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Colin.Common.Code.Tiled
{
    /// <summary>
    /// 瓦片地图中的元素: 瓦片.
    /// </summary>
    public class Tile : IEngineElement
    {
        private Vertices vertices;
        public Vertices Shape => vertices;

        private Body body;
        public Body Body => body;

        private List<string> _userDatas = new List<string>( );
        public List<string> UserDatas => _userDatas;

        public TileMap TileMap;

        public TileData TileData;

        public int CoordinateX => TileData.CoordinateX;

        public int CoordinateY => TileData.CoordinateY;

        public Point Coordinate => new Point(TileData.CoordinateX,TileData.CoordinateY);

        /// <summary>
        /// 获取该物块上方的物块.
        /// </summary>
        public Tile? Top
        {
            get
            {
                if( CoordinateY <= 0 )
                    return null;
                return TileMap.Tiles[CoordinateX,CoordinateY - 1];
            }
        }

        /// <summary>
        /// 获取该物块下方的物块.
        /// </summary>
        public Tile? Bottom
        {
            get
            {
                if( CoordinateY >= TileMap.Height - 1 )
                    return null;
                return TileMap.Tiles[CoordinateX,CoordinateY + 1];
            }
        }

        /// <summary>
        /// 获取该物块左方的物块.
        /// </summary>
        public Tile? Left
        {
            get
            {
                if( CoordinateX <= 0 )
                    return null;
                return TileMap.Tiles[CoordinateX - 1,CoordinateY];
            }
        }

        /// <summary>
        /// 获取该物块右方的物块.
        /// </summary>
        public Tile? Right
        {
            get
            {
                if( CoordinateX >= TileMap.Width - 1 )
                    return null;
                return TileMap.Tiles[CoordinateX + 1,CoordinateY];
            }
        }

        /// <summary>
        /// 指示物块非空状态的值, 其中 <seealso href="true"/> 为非空, <seealso href="false"/>为空.
        /// </summary>
        public bool Empty = true;

        public Texture2D Texture;

        /// <summary>
        /// 指示该物块是否进行纹理绘制的值.
        /// </summary>
        public bool TextureVisible = true;

        /// <summary>
        /// 指示该物块是否进行纹理绘制的值.
        /// </summary>
        public bool BorderVisible = true;

        protected virtual void CreateVertices( ref Vertices vertices )
        {
            if( TileMap != null )
            {
                vertices = new Vertices( );
                vertices.Add(new Vector2(-TileMap.GridSize / 2,-TileMap.GridSize / 2));
                vertices.Add(new Vector2(TileMap.GridSize / 2,-TileMap.GridSize / 2));
                vertices.Add(new Vector2(-TileMap.GridSize / 2,TileMap.GridSize / 2));
                vertices.Add(new Vector2(TileMap.GridSize / 2,TileMap.GridSize / 2));
            }
        }

        protected virtual void CreateBody( ref Body body )
        {
            if( TileMap != null )
            {
                body = BodyFactory.CreatePolygon(TileMap.world,vertices,1f);
                body.BodyType = BodyType.Static;
            }
        }

        protected virtual void CreateUserData( ref List<string> datas )
        {
        }

        protected virtual void SetTileData( ref TileData tileData )
        {
            tileData = new TileData(this);
            tileData.LoopTextureEnable = true;
            tileData.LoopTextureWidth = 256;
            tileData.LoopTextureHeight = 256;
        }

        protected virtual void SetTexture( ref Texture2D texture )
        {

        }

        public void DoInitialize( )
        {
            OnPlaceTile = null;
            OnPlaceTile += PlaceTileEvent;
            CreateVertices(ref vertices);
            CreateBody(ref body);
            if( body != null )
            {
                _userDatas.Add("Layer:Tile");
                body.UserData = _userDatas;
            }
            SetTileData(ref TileData);
            SetTexture(ref Texture);
            CreateUserData(ref _userDatas);
        }

        public void DoUpdate( )
        {
            TileData.UpdateTileData( );
        }

        public void DoRender( ) { }

        public Tile( )
        {

        }

        /// <summary>
        /// 初始化一个瓦片.
        /// </summary>
        /// <param name="tileMap">瓦片地图.</param>
        public Tile( TileMap tileMap )
        {
            TileMap = tileMap;
        }

        /// <summary>
        /// 可用于纹理合批优化的参数, 初始为 0.
        /// </summary>
        public float TextureLayerDepth = 0;

        /// <summary>
        /// 用于边框合批优化的参数, 初始为 0.
        /// </summary>
        public float BorderLayerDepth = 0;

        /// <summary>
        /// 物块的纹理绘制.
        /// <para>[!] 该函数仅 <see cref="Tile.TextureVisible"/> 为 <see href="true"/> 时执行.</para>
        /// </summary>
        public void RenderTexture( )
        {
            if( !TextureVisible )
                return;
            if( Texture != null )
            {
                EngineInfo.SpriteBatch.Draw(
                Texture, Coordinate.ToVector2( ) * 32 - Vector2.One * TileMap.GridSize / 2,
                TileData.TextureFrame.Frame,
                Color.White,0f,Vector2.Zero,1f,SpriteEffects.None,TextureLayerDepth);
            }
        }

        /// <summary>
        /// 物块的边框绘制.
        /// <para>[!] 该函数仅 <see cref="Tile.BorderVisible"/> 为 <see href="true"/> 时执行.</para>
        /// </summary>
        public void RenderBorder( )
        {
            if( !BorderVisible )
                return;
            if( Texture != null )
            {
                EngineInfo.SpriteBatch.Draw(
                    Texture,(Coordinate.ToVector2( ) + TileData.BorderRenderOffSet.ToVector2( )) * TileMap.GridSize - Vector2.One * TileMap.GridSize / 2,
                    TileData.BorderFrame.Frame,
                    Color.White,0f,Vector2.Zero,1f,SpriteEffects.None,BorderLayerDepth);
            }
        }

        public event Action OnPlaceTile;
        /// <summary>
        /// 执行在放置物块时发生的事件.
        /// </summary>
        public void DoPlaceTileEvent( )
        {
            TileData.Refresh( );
            OnPlaceTile?.Invoke( );
        }

        /// <summary>
        /// 重写该函数以在物块被放置进瓦片地图时执行自定义操作.
        /// </summary>
        protected virtual void PlaceTileEvent( )
        {

        }

    }
}