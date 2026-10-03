using Colin.Core.IO;
using Colin.Core.Mathematical;
using Colin.Core.Modulars.Ecses.Components;
using Colin.Core.Resources;

namespace Colin.Core.Modulars.Ecses
{
  public static class Identifier<T>
  {
    public static string TypeName => typeof(T).FullName;

    public static int Hash => TypeName.GetMsnHashCode();
  }
  /// <summary>
  /// 实体.
  /// </summary>
  public class Entity : IGameContent<Entity>, ICodeRes, IDoBlockable
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

    internal TypeSnapshotTable<IEcsCom> _components;
    /// <summary>
    /// 实体组件列表.
    /// </summary>
    public TypeSnapshotTable<IEcsCom> Components => _components;
    public bool HasCom<T>() where T : IEcsCom => _components.ContainsKey(typeof(T));
    public T GetCom<T>() where T : IEcsCom
    {
      if (_components is null)
        return default;
      return _components.TryGetValue(typeof(T), out IEcsCom component) ? (T)component : default;
    }
    public T RegisterCom<T>() where T : class, IEcsCom, new() => RegisterCom(new T()) as T;
    public IEcsCom RegisterCom(IEcsCom component)
    {
      if (component is IEcsComBindable bind)
        bind.Entity = this;
      if (Components.ContainsKey(component.GetType()) is false)
        Components.Add(component.GetType(), component);
      return component;
    }
    public bool RemoveCom<T>() where T : IEcsCom => _components.Remove(typeof(T));
    public void AddCom(IEcsCom com)
    {
      if (com is IEcsComBindable bind)
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

    public void SetNeedClear()
    {
      Ecs.NeedClear[ID] = true;
    }

    /// <summary>
    /// 指示该实体是否需要执行存储/读取操作.
    /// </summary>
    public bool NeedSaveAndLoad;

    public EcsComDoc _comDoc;
    public EcsComDoc Document => _comDoc;

    private EcsComTransform _comTransform;
    public EcsComTransform Transform => _comTransform;

    public RectangleF Bounds => Transform.LocalBound;

    public Vector2 Pos => Transform.Pos;

    private EcsComRenderData _renderData;
    public EcsComRenderData RenderData => _renderData ??= GetCom<EcsComRenderData>();

    public void SetSize(Vector2 size) => _comTransform.SetSize(size);
    public void SetSize(int width, int height) => _comTransform.SetSize(width, height);

    public bool Inited;
    public void DoInitialize()
    {
      if (Inited)
        return;
      Inited = true;
      _components = new TypeSnapshotTable<IEcsCom>();
      _comDoc = RegisterCom<EcsComDoc>();
      _comTransform = RegisterCom<EcsComTransform>();
      ComAdded();
      for (int count = 0; count < _components.Count; count++)
        _components[count].DoInitialize();
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

    public T Clone<T>() where T : Entity, new()
    {
      T t = new T();
      t.DoInitialize();
      Type type;
      for (int i = 0; i < Components.Count; i++)
      {
        type = Components[i].GetType();
        if (t.Components[type] is IEcsComCloneable cloneCom)
        {
          cloneCom.Clone(Components[type]);
        }
      }
      return t;
    }
    public Entity Clone()
    {
      Entity result = CodeResources<Entity>.CreateNewInstance(this);
      result.Ecs = Ecs;
      result.DoInitialize();
      Type type;
      for (int i = 0; i < Components.Count; i++)
      {
        type = Components[i].GetType();
        if (result.Components[type] is IEcsComCloneable cloneCom)
        {
          cloneCom.Clone(Components[type]);
        }
      }
      return result;
    }

    public void SaveStep(TagCompound data)
    {
      //组件各占一个键, 键名就是组件类型全名; 组件集合变了, 档里多出来的键没人读就自然跳过
      for (int i = 0; i < Components.Count; i++)
      {
        if (Components[i] is IEcsComIO io)
        {
          TagCompound comData = new TagCompound();
          io.SaveStep(comData);
          data[Components[i].GetType().FullName] = comData;
        }
      }
    }
    public void LoadStep(TagCompound data)
    {
      for (int i = 0; i < Components.Count; i++)
      {
        if (Components[i] is IEcsComIO io)
        {
          TagCompound comData = data.GetCompound(Components[i].GetType().FullName);
          if (comData is null)
            continue; //档里没有这个组件的键(新组件碰上老存档), 让它保持默认值
          io.LoadStep(comData);
        }
      }
    }

    public float GetDistance(Entity entity)
    {
      return Vector2.Distance(Transform.Translation, entity.Transform.Translation);
    }

    /// <summary>
    /// 减速.
    /// </summary>
    /// <param name="speed"></param>
    public void Decelerate(float speed)
    {
      if (Transform.HorizontalDirection == Direction.Left)
      {
        float _cache = Transform.Vel.X;
        _cache += speed * Time.DeltaTime;
        _cache = Math.Clamp(_cache, -int.MaxValue, 0);
        Transform.Vel.X = _cache;
      }
      else if (Transform.HorizontalDirection == Direction.Right)
      {
        float _cache = Transform.Vel.X;
        _cache -= speed * Time.DeltaTime;
        _cache = Math.Clamp(_cache, 0, int.MaxValue);
        Transform.Vel.X = _cache;
      }
    }

    public bool Equals(Entity other)
    {
      return Document.Equals(other.Document);
    }

    public static Entity CreatFromHash(int hash) => CodeResources<Entity>.CreateNewInstance(hash);
  }
}