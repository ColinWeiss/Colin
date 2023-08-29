using Colin.Core.Assets;

namespace Colin.Core.Graphics
{
    /// <summary>
    /// 标识一张Sprite.
    /// </summary>
    public class Sprite
    {
        /// <summary>
        /// 源纹理.
        /// </summary>
        public Texture2D Source { get; }

        /// <summary>
        /// 大小的一半.
        /// </summary>
        public Vector2 Half => new Vector2( Source.Width / 2, Source.Height / 2 );

        /// <summary>
        /// 大小.
        /// </summary>
        public Vector2 SizeF => new Vector2( Source.Width, Source.Height );

        /// <summary>
        /// 大小.
        /// </summary>
        public Point Size => new Point( Source.Width, Source.Height );

        public int Width => Source.Width;

        public int Height => Source.Height;

        /// <summary>
        /// 选取帧格.
        /// </summary>
        public SpriteRenderInfo spriteFrame;

        /// <summary>
        /// 纹理批绘制参数.
        /// </summary>
        public float Depth { get; internal set; }

        public string Name => Source.Name;

        private void AddThisToGraphicCoreSpritePool( )
        {
            if( SpritePool.Instance.ContainsKey( Source.Name ))
            {
                Sprite _sprite;
                SpritePool.Instance.TryGetValue( Source.Name, out _sprite );
                Depth = _sprite.Depth;
            }
            else
                SpritePool.Instance.Add( Source.Name, this );
        }

        public Sprite( Texture2D texture )
        {
            Source = texture;
            AddThisToGraphicCoreSpritePool( );
        }

        public static Sprite Get( Texture2D texture )
        {
            if( SpritePool.Instance.TryGetValue( texture.Name, out Sprite sprite ) )
                return sprite;
            else
                return new Sprite( texture );
        }

        public static Sprite Get( string path )
        {
            if ( SpritePool.Instance.TryGetValue( string.Concat( "Textures\\", path ), out Sprite sprite ) )
                return sprite;
            else
                return new Sprite( TextureAssets.Get( path ) );
        }
    }
}