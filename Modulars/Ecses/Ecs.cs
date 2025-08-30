using Colin.Core.Common.Debugs;
using Colin.Core.Events;
using Colin.Core.IO;
using Colin.Core.Resources;
using System.Security.Policy;

namespace Colin.Core.Modulars.Ecses
{
  /// <summary>
  /// ECS: Entity Component System.
  /// <br>但我将称它为 Environmental Control Entity.</br>
  /// <br>「环境控制实体」</br>
  /// <br>这一系列会影响到环境的游戏元素, 我们统称为 <see cref="Entity"/>.</br>
  /// </summary>
  public class Ecs : SceneRenderModule, IOStep
  {
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

    public KeysEventNode KeysEvent;

    public override void DoInitialize()
    {
      KeysEvent = new KeysEventNode();
      Scene.Events.Keys.Register(KeysEvent);
      Entities = new Entity[2049];
      _systems = new Dictionary<Type, Entitiesystem>();
    }
    public override void Start()
    {
      Entitiesystem _system;
      for (int sCount = 0; sCount < _systems.Values.Count; sCount++)
      {
        _system = _systems.Values.ElementAt(sCount);
        _system.Start();
      }
    }

    public override void DoUpdate(GameTime time)
    {
      using (DebugProfiler.Tag("ECS System"))
      {
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
              if (com is IEntityUnloadableCom unLoadCom)
                unLoadCom.OnClear();
            }
            Entities[count] = null;
            continue;
          }
        }
      }
    }
    public override void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      device.Clear(Color.Transparent);
      Entitiesystem _system;
      for (int count = 0; count < _systems.Values.Count; count++)
      {
        _system = _systems.Values.ElementAt(count);
        _system.DoRender(device, batch);
      }
    }
    public override void DoRegenerateRender(GraphicsDevice device, SpriteBatch batch)
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

    public override void Dispose()
    {
      for (int count = 0; count < Entities.Length; count++)
      {
        if (Entities[count] is IDisposable disposable)
        {
          disposable.Dispose();
        }
      }
      base.Dispose();
    }

    public void LoadStep(BinaryReader reader)
    {
      for (int i = 0; i < Entities.Length; i++)
      {
        LoadEntity(reader, ref Entities[i]);
      }
    }

    public static void LoadEntity(BinaryReader reader, ref Entity entity)
    {
      int hashValue;
      if (reader.ReadBoolean())
      {
        hashValue = reader.ReadInt32();
        if (entity is null)
        {
          entity = CodeResources<Entity>.CreateNewInstance(hashValue);
          entity.NeedSaveAndLoad = true;
          entity.DoInitialize();
          entity.LoadStep(reader);
        }
        else
        {
          entity.NeedSaveAndLoad = true;
          entity.LoadStep(reader);
        }
      }
    }
    public static void SaveEntity(BinaryWriter writer, Entity entity)
    {
      int? hash;
      if (entity is null || entity.NeedSaveAndLoad is false)
      {
        writer.Write(false);
        return;
      }
      if (entity.NeedSaveAndLoad)
      {
        writer.Write(true);
        hash = CodeResources<Entity>.GetHashFromTypeName(entity.Identifier);
        writer.Write(hash.Value);
        entity.SaveStep(writer);
      }
    }
    public void SaveStep(BinaryWriter writer)
    {
      for (int i = 0; i < Entities.Length; i++)
      {
        SaveEntity(writer, Entities[i]);
      }
    }
  }
}