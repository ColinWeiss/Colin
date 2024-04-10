namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// 行为脚本组件，提供更多相关接口，如允许访问 Section 与 Ecs.
  /// </summary>
  public abstract class EcsComScript : ISectionComponent, IResetable
  {
    public Section Section { get; private set; }
    private Ecs _ecs;
    public Ecs Ecs
    {
      get
      {
        if (_ecs is null)
          _ecs = Section.Ecs;
        return _ecs;
      }
    }

    public bool ResetEnable { get; set; } = true;

    public void SetSection(Section section) { Section = section; }
    /// <summary>
    /// 通过  Script 访问同 <see cref="Section"/> 的其他 <see cref="ISectionComponent"/>.
    /// </summary>
    public T GetComponent<T>() where T : ISectionComponent => Section.GetComponent<T>();
    public bool HasComponent<T>() where T : ISectionComponent => Section.HasComponent<T>();
    public virtual void DoInitialize() { }
    public virtual void Reset() { }
    internal bool _updateStarted = false;
    public virtual void UpdateStart() { }
    public virtual void DoUpdate() { }
  }
}