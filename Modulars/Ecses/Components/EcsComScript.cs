namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// 行为脚本组件, 提供更多相关接口, 如允许访问 Entity 与 Ecs.
  /// </summary>
  public abstract class EcsComScript : IEntityCom, IEntityBindableCom, IResetable
  {
    public Entity Entity { get; set; }

    private Ecs _ecs;
    public Ecs Ecs => _ecs ??= Entity.Ecs;

    public bool ResetEnable { get; set; } = true;

    /// <summary>
    /// 通过  Script 访问同 <see cref="Entity"/> 的其他 <see cref="IEntityCom"/>.
    /// </summary>
    public T GetCom<T>() where T : IEntityCom => Entity.GetCom<T>();
    public bool HasCom<T>() where T : IEntityCom => Entity.HasCom<T>();
    public virtual void DoInitialize() { }
    public virtual void Reset() { }

    internal bool _updateStarted = false;
    public virtual void UpdateStart() { }

    public bool UpdateEnable { get; set; } = true;
    public virtual void DoUpdate() { }
  }
}