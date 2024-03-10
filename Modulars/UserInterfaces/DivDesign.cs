namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 为划分元素指定设计样式.
    /// </summary>
    public struct DivDesign
    {
        public Color Color;

        private float anchorX;
        /// <summary>
        /// 指示划分元素的锚点 X 值.
        /// </summary>
        public float AnchorX
        {
            get => anchorX;
            set => anchor.X = anchorX = value;
        }

        private float anchorY;
        /// <summary>
        /// 指示划分元素的锚点 Y 值.
        /// </summary>
        public float AnchorY
        {
            get => anchorY;
            set => anchor.Y = anchorY = value;
        }

        private Vector2 anchor;
        /// <summary>
        /// 指示划分元素的锚点坐标.
        /// </summary>
        public Vector2 Anchor
        {
            get => anchor;
            set => SetAnchor(value);
        }
        /// <summary>
        /// 设置划分元素的锚点坐标.
        /// </summary>
        /// <param name="anchorX">锚点 X 值.</param>
        /// <param name="anchorY">锚点 Y 值.</param>
        public void SetAnchor(float anchorX, float anchorY)
        {
            anchor.X = this.anchorX = anchorX;
            anchor.Y = this.anchorY = anchorY;
        }
        /// <summary>
        /// 设置划分元素的锚点坐标.
        /// </summary>
        /// <param name="anchor">锚点坐标.</param>
        public void SetAnchor(Vector2 anchor) => SetAnchor(anchor.X, anchor.Y);

        public DivDesign()
        {
            Color = Color.White;
        }
    }
}
