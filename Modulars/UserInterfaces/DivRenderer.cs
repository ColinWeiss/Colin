namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 划分元素渲染器.
    /// </summary>
    public class DivRenderer
    {
        internal Div div;
        public Div Div => div;
        public virtual void OnBinded() { }
        public virtual void OnDivInitialize() { }
        public virtual void DoRender(GraphicsDevice device, SpriteBatch batch) { }
    }
}