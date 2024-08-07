﻿using Colin.Core.Modulars.Ecses.Components;
using Colin.Core.Resources;

namespace Colin.Core.Modulars.Ecses
{
  /// <summary>
  /// 切片.
  /// </summary>
  public class Section : IGameContent<Section>, ICodeResource
  {
    internal Dictionary<Type, ISectionCom> _components;
    /// <summary>
    /// 切片组件列表.
    /// </summary>
    public Dictionary<Type, ISectionCom> Components => _components;
    public bool HasComponent<T>() where T : ISectionCom => _components.ContainsKey(typeof(T));
    public T GetComponent<T>() where T : ISectionCom => (T)_components.GetValueOrDefault(typeof(T), null);
    public T RegisterCom<T>() where T : class, ISectionCom, new() => Register(new T()) as T;
    public bool RemoveComponent<T>() where T : ISectionCom => _components.Remove(typeof(T));
    public ISectionCom Register(ISectionCom component)
    {
      if (component is EcsComScript script)
      {
        script.SetSection(this);
      }
      if (Components.ContainsKey(component.GetType()) is false)
        Components.Add(component.GetType(), component);
      return component;
    }
    public Ecs Ecs { get; internal set; }

    /// <summary>
    /// 获取切片运行时 ID.
    /// </summary>
    public int ID;

    /// <summary>
    /// 指示该切片需要被清除.
    /// </summary>
    public bool NeedClear;

    /// <summary>
    /// 指示该切片是否还存在于 Ecs系统的 对象池 中.
    /// </summary>
    public bool Active
    {
      get
      {
        if (Ecs.Sections[ID] is not null)
          return Ecs.Sections[ID].Equals(this);
        else
          return false;
      }
    }

    public EcsComDoc _comDoc;
    public EcsComDoc Document => _comDoc;
    public void AddTag(string tag) => _comDoc.Tags.Add(tag);

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
      _components = new Dictionary<Type, ISectionCom>();
      _comDoc = RegisterCom<EcsComDoc>();
      _comDoc.Section = this;
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