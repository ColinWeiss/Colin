namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 为划分元素指定设计样式.
    /// </summary>
    public struct DesignStyle
    {
        public Color Color;

        public Vector2 Scale;

        public float Rotation;

        public Vector2 Anchor;

        public DesignStyle()
        {
            Color = Color.White;
            Scale = Vector2.One;
            Rotation = 0f;
            Anchor = Vector2.Zero;
        }
    }
}
