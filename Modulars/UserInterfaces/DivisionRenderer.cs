namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 划分元素渲染器.
    /// </summary>
    public abstract class DivisionRenderer
    {
        internal Division _division;
        public Division Division => _division;
        public abstract void RendererInit();
        public abstract void DoRender(SpriteBatch batch);
    }
}