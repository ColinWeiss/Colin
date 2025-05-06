using Colin.Core.IO;
using Colin.Core.Modulars.Ecses.Components;
using Colin.Core.Resources;
using DeltaMachine.Core;
using System.Diagnostics;

namespace Colin.Core.Modulars.Ecses
{
  /// <summary>
  /// 实体.
  /// </summary>
  public class Entity : IGameContent<Entity>, ICodeResource
  {
    private string _identifier;
    public string Identifier
    {
      get
      {
        if (_identifier is null || _identifier == string.Empty)
          _identifier = GetType().FullName;
        return _identifier;
      }
    }

    internal Dictionary<Type, IEntityCom> _components;
    /// <summary>
    /// 实体组件列表.
    /// </summary>
    public Dictionary<Type, IEntityCom> Components => _components;
    public bool HasCom<T>() where T : IEntityCom => _components.ContainsKey(typeof(T));
    public T GetCom<T>() where T : IEntityCom => (T)_components.GetValueOrDefault(typeof(T), null);
    public T RegisterCom<T>() where T : class, IEntityCom, new() => RegisterCom(new T()) as T;
    public IEntityCom RegisterCom(IEntityCom component)
    {
      if (component is IEntityBindableCom bind)
        bind.Entity = this;
      if (Components.ContainsKey(component.GetType()) is false)
        Components.Add(component.GetType(), component);
      return component;
    }
    public bool RemoveCom<T>() where T : IEntityCom => _components.Remove(typeof(T));
    public void AddCom(IEntityCom com)
    {
      if (com is IEntityBindableCom bind)
        bind.Entity = this;
      if (Components.ContainsKey(com.GetType()) is false)
        Components.Add(com.GetType(), com);
    }

    public Ecs Ecs { get; internal set; }

    /// <summary>
    /// 获取实体运行时 ID.
    /// </summary>
    public int ID;

    /// <summary>
    /// 指示该实体需要从Ecs中被清除.
    /// </summary>
    public bool NeedClear;

    /// <summary>
    /// 指示该实体是否还存在于Ecs系统的对象池 中.
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

    /// <summary>
    /// 指示该实体是否需要执行存储/读取操作.
    /// </summary>
    public bool NeedSaveAndLoad;

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
      _comTransform = RegisterCom<EcsComTransform>();
      ComAdded();
      for (int count = 0; count < _components.Count; count++)
        _components.Values.ElementAt(count).DoInitialize();
      SetDefaults();
    }
    /// <summary>
    /// 在此处添加组件.
    /// </summary>
    public virtual void ComAdded() { }

    /// <summary>
    /// 在此处处理组件数据.
    /// </summary>
    public virtual void SetDefaults() { }

    public void SaveStep(BinaryWriter writer)
    {
      for (int i = 0; i < Components.Count; i++)
      {
        if (Components.ElementAt(i).Value is IEcsComIO io)
          io.SaveStep(writer);
      }
    }
    public void LoadStep(BinaryReader reader)
    {
      for (int i = 0; i < Components.Count; i++)
      {
        if (Components.ElementAt(i).Value is IEcsComIO io)
          io.LoadStep(reader);
      }
    }
  }
}