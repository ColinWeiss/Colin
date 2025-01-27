namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// 行为脚本组件, 提供更多相关接口, 如允许访问 Entity 与 Ecs.
  /// </summary>
  public abstract class EcsComScript : IEntityCom, IResetable
  {
    public Entity Entity { get; private set; }
    private Ecs _ecs;
    public Ecs Ecs => _ecs ??= Entity.Ecs;

    public bool ResetEnable { get; set; } = true;
    public void SetEntity(Entity entitiy) => Entity = entitiy;

    /// <summary>
    /// 通过  Script 访问同 <see cref="Entity"/> 的其他 <see cref="IEntityCom"/>.
    /// </summary>
    public T GetComponent<T>() where T : IEntityCom => Entity.GetCom<T>();
    public bool HasComponent<T>() where T : IEntityCom => Entity.HasCom<T>();
    public virtual void DoInitialize() { }
    public virtual void Reset() { }

    internal bool _updateStarted = false;
    public virtual void UpdateStart() { }

    public bool UpdateEnable { get; set; } = true;
    public virtual void DoUpdate() { }
  }
}