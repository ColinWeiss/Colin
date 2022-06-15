using Colin.Common.Code.Fecs;
using Colin.Common.Code.Physics.Collision.Shapes;
using Colin.Common.Code.Physics.Dynamics;
using Colin.Common.Code.Physics.Factories;
using Colin.Common.Code.Physics.Shared;
using Colin.Common.Code.Physics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Common.Code.Tiled
{
    /// <summary>
    /// 瓦片地图中的元素: 瓦片.
    /// </summary>
    public class Tile : Entity, IEngineElement
    {
        private Vertices vertices;
        public Vertices Shape => vertices;

        private Body body;
        public Body Body => body;

        private List<string> userDatas;
        public List<string> UserDatas => userDatas;

        public TileMap TileMap { get; }

        public TileData TileData { get; }

        public int CoordinateX => TileData.CoordinateX;

        public int CoordinateY => TileData.CoordinateY;

        public Point Coordinate => new Point( TileData.CoordinateX, TileData.CoordinateY );

        /// <summary>
        /// 获取该物块上方的物块.
        /// </summary>
        public Tile? Top
        {
            get
            {
                if ( CoordinateY <= 0 )
                    return null;
                return TileMap.Tile[ CoordinateX, CoordinateY - 1 ];
            }
        }

        /// <summary>
        /// 获取该物块下方的物块.
        /// </summary>
        public Tile? Bottom
        {
            get
            {
                if ( CoordinateY >= TileMap.Height - 1 )
                    return null;
                return TileMap.Tile[ CoordinateX, CoordinateY + 1 ];
            }
        }

        /// <summary>
        /// 获取该物块左方的物块.
        /// </summary>
        public Tile? Left
        {
            get
            {
                if ( CoordinateX <= 0 )
                    return null;
                return TileMap.Tile[ CoordinateX - 1, CoordinateY ];
            }
        }

        /// <summary>
        /// 获取该物块右方的物块.
        /// </summary>
        public Tile? Right
        {
            get
            {
                if ( CoordinateX >= TileMap.Width - 1 )
                    return null;
                return TileMap.Tile[ CoordinateX + 1, CoordinateY ];
            }
        }

        public virtual Texture2D Texture => null;

        /// <summary>
        /// 指示该物块是否进行纹理绘制.
        /// </summary>
        public bool TextureVisible = true;

        public virtual void CreateVertices( ref Vertices vertices )
        {
            vertices = new Vertices( );
            vertices.Add( new Vector2( -TileMap.GridSize / 2, -TileMap.GridSize / 2 ) );
            vertices.Add( new Vector2( TileMap.GridSize / 2, -TileMap.GridSize / 2 ) );
            vertices.Add( new Vector2( -TileMap.GridSize / 2, TileMap.GridSize / 2 ) );
            vertices.Add( new Vector2( TileMap.GridSize / 2, TileMap.GridSize / 2 ) );
        }

        public virtual void CreateBody( ref Body body )
        {
            body = BodyFactory.CreatePolygon( TileMap.world, vertices, 1f );
            body.BodyType = BodyType.Static;
        }

        public virtual void CreateUserData( ref List<string> datas )
        {
        }

        public override void DoInitialize( )
        {
            CreateVertices( ref vertices );
            CreateBody( ref body );
            if ( body != null )
            {
                body.UserData = userDatas;
                userDatas.Add( "Layer:Tile" );
            }
            CreateUserData( ref userDatas );

            base.DoInitialize( );
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
            if ( !TextureVisible )
                return;

            if ( Texture != null )
            {
                EngineInfo.SpriteBatch.Draw(
                Texture, Coordinate.ToVector2( ) * TileMap.GridSize,
                TileData.TextureFrame.Frame,
                Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, TextureLayerDepth );
            }
        }

        public event Action OnPlaceTile;
        /// <summary>
        /// 执行在放置物块时发生的事件.
        /// </summary>
        public void DoPlaceTileEvent( )
        {

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