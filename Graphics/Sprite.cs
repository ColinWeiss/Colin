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
        public Vector2 Half => new Vector2(Source.Width / 2, Source.Height / 2);

        /// <summary>
        /// 大小.
        /// </summary>
        public Vector2 SizeF => new Vector2(Source.Width, Source.Height);

        /// <summary>
        /// 大小.
        /// </summary>
        public Point Size => new Point(Source.Width, Source.Height);

        public int Width => Source.Width;

        public int Height => Source.Height;

        /// <summary>
        /// 指示该 Sprite 的帧格刷新是否交由自动化运行程序.
        /// </summary>
        public bool AutoUpdateFrame = true;

        /// <summary>
        /// 该纹理内置的帧格选取.
        /// </summary>
        public Frame Frame;

        /// <summary>
        /// 纹理批绘制参数.
        /// </summary>
        public float Depth { get; internal set; }

        public string Name => Source.Name;

        private void AddThisToGraphicCoreSpritePool()
        {
            if (SpritePool.Instance.ContainsKey(Source.Name))
            {
                Sprite _sprite;
                SpritePool.Instance.TryGetValue(Source.Name, out _sprite);
                Depth = _sprite.Depth;
            }
            else
            {
                SpritePool.Instance.Add(Source.Name, this);
            }
        }

        public static void New(Texture2D texture2D)
        {
            new Sprite(texture2D);
        }

        public Sprite(Texture2D texture)
        {
            Source = texture;
            Frame.Width = texture.Width;
            Frame.Height = texture.Height;
            Frame.Direction = Direction.Portrait;
            AddThisToGraphicCoreSpritePool();
        }

        public Sprite(Texture2D texture, int frameMax = 1, bool isLoop = true, bool isPlay = true, Direction direction = Direction.Portrait)
        {
            Source = texture;
            Frame.Width = texture.Width;
            Frame.Height = texture.Height;
            Frame.IsLoop = isLoop;
            Frame.IsPlay = isPlay;
            Frame.Direction = direction;
            if (Frame.Direction is Direction.Portrait)
                Frame.Height = texture.Height / frameMax;
            if (Frame.Direction is Direction.Transverse)
                Frame.Width = texture.Width / frameMax;
            AddThisToGraphicCoreSpritePool();
        }

        public static Sprite Get(Texture2D texture)
        {
            if (SpritePool.Instance.TryGetValue(texture.Name, out Sprite sprite))
                return sprite;
            else
                return new Sprite(texture);
        }

        public static Sprite Get(string path)
        {
            string realPath = path.Replace('/', Path.DirectorySeparatorChar);
            if (SpritePool.Instance.TryGetValue(Path.Combine("Textures", realPath), out Sprite sprite))
                return sprite;
            else
                return new Sprite(TextureAssets.Get(realPath));
        }
        public static Sprite Get(params string[] paths) => Get(Path.Combine(paths));
    }
}