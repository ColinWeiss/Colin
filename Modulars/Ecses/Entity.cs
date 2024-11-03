using Colin.Core.Modulars.Ecses.Components;
using Colin.Core.Resources;

namespace Colin.Core.Modulars.Ecses
{
  /// <summary>
  /// 实体.
  /// </summary>
  public class Entity : IGameContent<Entity>, ICodeResource
  {
    internal Dictionary<Type, IEntityCom> _components;
    /// <summary>
    /// 实体组件列表.
    /// </summary>
    public Dictionary<Type, IEntityCom> Components => _components;
    public bool HasComponent<T>() where T : IEntityCom => _components.ContainsKey(typeof(T));
    public T GetComponent<T>() where T : IEntityCom => (T)_components.GetValueOrDefault(typeof(T), null);
    public T RegisterCom<T>() where T : class, IEntityCom, new() => Register(new T()) as T;
    public bool RemoveComponent<T>() where T : IEntityCom => _components.Remove(typeof(T));
    public IEntityCom Register(IEntityCom component)
    {
      if (component is EcsComScript script)
      {
        script.SetEntity(this);
      }
      if (Components.ContainsKey(component.GetType()) is false)
        Components.Add(component.GetType(), component);
      return component;
    }
    public Ecs Ecs { get; internal set; }

    /// <summary>
    /// 获取实体运行时 ID.
    /// </summary>
    public int ID;

    /// <summary>
    /// 指示该实体需要被清除.
    /// </summary>
    public bool NeedClear;

    /// <summary>
    /// 指示该实体是否还存在于 Ecs系统的 对象池 中.
    /// </summary>
    public bool Active
    {
      get
      {
        if (Ecs.Entities[ID] is not null)
          return Ecs.Entities[ID].Equals(this);
        else
          return false;
      }
    }

    public EcsComDoc _comDoc;
    public EcsComDoc Document => _comDoc;

    private EcsComTransform _comTransform;
    public EcsComTransform Transform => _comTransform;

    private EcsComAdvancedRender _comAdvancedRender;
    public EcsComAdvancedRender ComAdvancedRender => _comAdvancedRender;

    private EcsComDeferredRender _comDeferredRender;
    public EcsComDeferredRender ComDeferredRender => _comDeferredRender;

    public void SetSize(Vector2 size) => _comTransform.SetSize(size);
    public void SetSize(int width, int height) => _comTransform.SetSize(width, height);
    public void DoInitialize()
    {
      _components = new Dictionary<Type, IEntityCom>();
      _comDoc = RegisterCom<EcsComDoc>();
      _comDoc.Entity = this;
      _comTransform = RegisterCom<EcsComTransform>();
      SetDefaults();
      for (int count = 0; count < _components.Count; count++)
        _components.Values.ElementAt(count).DoInitialize();
      SetDefaultsComplete();
    }
    public virtual void SetDefaults() { }
    public virtual void SetDefaultsComplete() { }
  }
}