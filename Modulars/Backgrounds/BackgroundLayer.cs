namespace Colin.Core.Modulars.Backgrounds
{
    /// <summary>
    /// 背景层.
    /// </summary>
    public class BackgroundLayer
    {
        /// <summary>
        /// 该背景层使用的纹理.
        /// </summary>
        public Sprite Sprite { get; }

        /// <summary>
        /// 指示该背景层是否属于循环背景层.
        /// </summary>
        public bool IsLoop = false;

        /// <summary>
        /// 指示该背景是否为锁定背景层.
        /// </summary>
        public bool IsFix = false;

        /// <summary>
        /// 指示该背景层的循环样式.
        /// </summary>
        public BackgroundLoopStyle LoopStyle;

        /// <summary>
        /// 指示该层的背景视差值.
        /// </summary>
        public Vector2 Parallax = new Vector2( 1f, 1f );

        /// <summary>
        /// 指示平移偏移量.
        /// </summary>
        public Vector3 Translation => Transform.Translation;

        /// <summary>
        /// 转换矩阵.
        /// </summary>
        public Matrix Transform;

        public BackgroundLayer( Sprite sprite )
        {
            Sprite = sprite;
        }

    }
}