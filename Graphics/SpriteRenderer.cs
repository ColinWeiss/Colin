namespace Colin.Core.Graphics
{
    public class SpriteRenderer
    {
        /// <summary>
        /// 渲染位置.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// 当前渲染信息.
        /// </summary>
        public SpriteRenderInfo Frame;

        /// <summary>
        /// 旋转.
        /// </summary>
        public float Rotation = 0f;

        /// <summary>
        /// 绘制原点.
        /// </summary>
        public Vector2 Anchor = Vector2.Zero;

        /// <summary>
        /// 缩放.
        /// </summary>
        public float Scale = 1f;

        /// <summary>
        /// 绘制贴图.
        /// </summary>
        public Sprite Sprite;

        public virtual void Render( SpriteBatch batch )
        {
            batch.Draw( Sprite.Source, Position + Anchor, Frame.Frame, Color.White, Rotation, Anchor, Scale, SpriteEffects.None, Sprite.Depth );
        }

        public virtual SpriteRenderer Clone()
        {
            SpriteRenderer result = new SpriteRenderer();
            result.Position = Position;
            result.Frame = Frame;
            result.Rotation = Rotation;
            result.Anchor = Anchor;
            result.Scale = Scale;
            result.Sprite = Sprite;
            return result;
        }
    }
}