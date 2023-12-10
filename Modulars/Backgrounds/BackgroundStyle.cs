namespace Colin.Core.Modulars.Backgrounds
{
    /// <summary>
    /// 背景样式.
    /// </summary>
    public class BackgroundStyle
    {
        /// <summary>
        /// 背景图层.
        /// <br>[!] 建议每一层的大小都一样, 否则可能会出现无法预料的后果.</br>
        /// </summary>
        public List<BackgroundLayer> Layers = new List<BackgroundLayer>();

        /// <summary>
        /// 固定图层在绘制时的整体偏移量.
        /// </summary>
        public Vector2 FixLayerOverallOffset;

        /// <summary>
        /// 固定图层在绘制时的整体缩放量.
        /// </summary>
        public Vector2 FixLayerScale;

        /// <summary>
        /// 循环图层在绘制时的整体偏移量.
        /// </summary>
        public Vector2 LoopLayerOffset;

        /// <summary>
        /// 循环图层在绘制时的起点坐标.
        /// </summary>
        public Vector2 LoopLayerDrawPosition;

        public virtual void SetDefault() { }

        public virtual void UpdateStyle() { }

        protected Texture2D GetBackgroundTexture(string path)
        {
            return TextureAssets.Get( string.Concat( "Backgrounds/", path ) );
        }

    }
}