namespace Colin.Core.Modulars.Ecses.Components
{
    /// <summary>
    /// 行为脚本组件，提供更多相关接口，如允许访问 Section 与 Ecs.
    /// </summary>
    public abstract class EcsComScript : ISectionComponent
    {
        public Section Section { get; private set; }
        public Ecs Ecs => Section.Ecs;
        public void SetSection(Section section) { Section = section; }
        /// <summary>
        /// 通过  Script 访问同 <see cref="Section"/> 的其他 <see cref="ISectionComponent"/>.
        /// </summary>
        public T GetComponent<T>() where T : ISectionComponent => Section.GetComponent<T>();
        public bool HasComponent<T>() where T : ISectionComponent => Section.HasComponent<T>();
        public virtual void DoInitialize() { }
        public virtual void Start() { }
        public virtual void Reset() { }
        public virtual void DoUpdate() { }
        public virtual void DoRender( GraphicsDevice device, SpriteBatch batch ) { }
    }
}