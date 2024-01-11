namespace Colin.Core.Modulars.Ecses
{
    public class SectionSystem
    {
        internal Ecs _ecs;
        public Ecs Ecs => _ecs;
        internal Section _current;
        /// <summary>
        /// 获取当前正在操作的切片.
        /// </summary>
        public Section Current => _current;
        /// <summary>
        /// 执行系统初始化.
        /// <br>[!] 在该方法内你无法获取 <see cref="Current"/>.</br>
        /// </summary>
        public virtual void DoInitialize() { }
        public virtual void Start() { }
        public virtual void Reset() { }
        public virtual void DoUpdate() { }
        public virtual void DoRender(GraphicsDevice device, SpriteBatch batch) { }
    }
}