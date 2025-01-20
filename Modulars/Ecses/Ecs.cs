using Colin.Core.Events;
using Colin.Core.Resources;

namespace Colin.Core.Modulars.Ecses
{
  /// <summary>
  /// ECS: Entity Component System.
  /// <br>但我将称它为 Environmental Control Entity.</br>
  /// <br>「环境控制实体」</br>
  /// <br>这一系列会影响到环境的游戏元素, 我们统称为 <see cref="Entity"/>.</br>
  /// </summary>
  public class Ecs : ISceneModule, IRenderableISceneModule
  {
    public Scene Scene { get; set; }

    public bool Enable { get; set; }

    public bool RawRtVisible { get; set; }

    public bool Presentation { get; set; }

    public RenderTarget2D RawRt { get; set; }

    public EnvironmentalController Controller;

    private Dictionary<Type, Entitiesystem> _systems;
    public Dictionary<Type, Entitiesystem> Systems => _systems;

    public T RegisterSystem<T>() where T : Entitiesystem, new()
    {
      T system = new T();
      system._ecs = this;
      system.DoInitialize();
      _systems.Add(typeof(T), system);
      return system;
    }
    public T GetSystem<T>() where T : Entitiesystem => (T)_systems.GetValueOrDefault(typeof(T));

    public Entity[] Entities;

    public KeysEventResponder KeysEvent;

    public void DoInitialize()
    {
      Controller = new EnvironmentalController();
      Controller.DoInitialize();
      KeysEvent = new KeysEventResponder("Ecs.KeysEvent");
      Scene.Events.KeysEvent.Register(KeysEvent);
      Entities = new Entity[2000];
      _systems = new Dictionary<Type, Entitiesystem>();
    }
    public void Start()
    {
      Entitiesystem _system;
      for (int sCount = 0; sCount < _systems.Values.Count; sCount++)
      {
        _system = _systems.Values.ElementAt(sCount);
        _system.Start();
      }
    }

    public void DoUpdate(GameTime time)
    {
      Controller.Reset();
      Entity _Entity;
      Entitiesystem _currentSystem;
      for (int count = 0; count < Systems.Count; count++)
      {
        _currentSystem = Systems.ElementAt(count).Value;
        _currentSystem.Reset();
      }
      for (int count = 0; count < Systems.Count; count++)
      {
        _currentSystem = Systems.ElementAt(count).Value;
        _currentSystem.DoUpdate();
      }
      Dictionary<Type, IEntityCom>.ValueCollection coms;
      IEntityCom com;
      for (int count = 0; count < Entities.Length; count++)
      {
        _Entity = Entities[count];
        if (_Entity is null)
          continue;
        if (_Entity.NeedClear)
        {
          coms = _Entity._components.Values;
          for (int i = 0; i < coms.Count; i++)
          {
            com = coms.ElementAt(i);
          }
          Entities[count] = null;
          continue;
        }
      }
    }
    public void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      device.Clear(Color.Transparent);
      Entitiesystem _system;
      for (int count = 0; count < _systems.Values.Count; count++)
      {
        _system = _systems.Values.ElementAt(count);
        _system.DoRender(device, batch);
      }
    }
    public void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch)
    {
      // device.SetRenderTarget(LightingAdpter.RawRt);
    }

    /// <summary>
    /// 将指定对象格清空.
    /// </summary>
    /// <param name="index"></param>
    public void SetNull(int index)
    {
      Entities[index].NeedClear = true;
    }
    public T Create<T>() where T : Entity, new()
    {
      T result;
      Entity Entity;
      for (int count = 0; count < Entities.Length; count++)
      {
        Entity = Entities[count];
        if (Entity is null)
        {
          result = new T();
          result.Ecs = this;
          result.ID = count;
          result.DoInitialize();
          Entities[count] = result;
          return result;
        }
      }
      return null;
    }

    public Entity Put(Entity Entity)
    {
      for (int count = 0; count < Entities.Length; count++)
      {
        if (Entities[count] is null)
        {
          Entities[count] = Entity;
          Entity.ID = count;
          Entity.Ecs = this;
          Entity.DoInitialize();
          return Entity;
        }
      }
      return null;
    }

    public Entity Copy(Entity entity)
    {
      for (int count = 0; count < Entities.Length; count++)
      {
        if (Entities[count] is null)
        {
          entity = CodeResources<Entity>.GetFromType(entity.GetType());
          entity.ID = count;
          entity.Ecs = this;
          entity.DoInitialize();
          Entities[count] = entity;
          return Entities[count];
        }
      }
      return null;
    }

    public void Dispose()
    {
      for (int count = 0; count < Entities.Length; count++)
      {
        if (Entities[count] is IDisposable disposable)
        {
          disposable.Dispose();
        }
      }
    }
  }
}