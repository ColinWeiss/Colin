using Colin.Core.Common.Debugs;
using Colin.Core.Events;
using Colin.Core.IO;
using Colin.Core.Mathematical;
using Colin.Core.Resources;
using SharpDX;
using System.Collections.Concurrent;

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
    private TypeSnapshotTable<EcsSystem> _systems;
    public TypeSnapshotTable<EcsSystem> Systems => _systems;

    public T RegisterSystem<T>() where T : EcsSystem, new()
    {
      T system = new T();
      system._ecs = this;
      system.DoInitialize();
      _systems.Add(typeof(T), system);
      return system;
    }
    public T GetSystem<T>() where T : EcsSystem => _systems.TryGetValue(typeof(T), out EcsSystem system) ? (T)system : default;

    public Entity[] Entities;

    public bool[] NeedClear;

    private readonly HashSet<int> _reservedSlots = new HashSet<int>();

    public KeysEventNode KeysEvent;

    public override void DoInitialize()
    {
      KeysEvent = new KeysEventNode();
      Scene.Events.Keys.Register(KeysEvent);
      Entities = new Entity[2049];
      NeedClear = new bool[2049];
      _reservedSlots.Clear();
      _systems = new TypeSnapshotTable<EcsSystem>();
      CreateCommands = new ConcurrentQueue<EcsCreateCommand>();
    }
    public override void Start()
    {
      EcsSystem _system;
      for (int sCount = 0; sCount < _systems.Count; sCount++)
      {
        _system = _systems[sCount];
        _system.Start();
      }
    }

    /// <summary>
    /// 当一个实体格被真正地赋为 Null 时发生.
    /// </summary>
    public event Action<Entity> OnNull;

    public override void DoUpdate(GameTime time)
    {
      {
        Entity _entity;
        EcsSystem _currentSystem;
        EcsCreateCommand cmd;
        while (CreateCommands.Count > 0)
        {
          if(CreateCommands.TryDequeue(out cmd))
            HandleCreate(cmd);
        }
          for (int count = 0; count < Systems.Count; count++)
        {
          _currentSystem = Systems[count];
          _currentSystem.Reset();
        }
        for (int count = 0; count < Systems.Count; count++)
        {
          _currentSystem = Systems[count];
          _currentSystem.DoUpdate();
        }
        IEcsCom com;
        for (int count = 0; count < Entities.Length; count++)
        {
          _entity = Entities[count];
          if (_entity is null)
            continue;
          if (NeedClear[_entity.ID])
          {
            for (int i = 0; i < _entity._components.Count; i++)
            {
              com = _entity._components[i];
              if (com is IEcsComFinalize unLoadCom)
                unLoadCom.DoFinalize();
            }
            if (NeedClear[_entity.ID] is false) //OnFinalize 有可能拒绝 Clear.
              continue;
            OnNull?.Invoke(Entities[count]);
            Entities[count] = null;
            NeedClear[count] = false;
            continue;
          }
        }
      }
    }
    public override void DoRawRender(GraphicsDevice device, SpriteBatch batch)
    {
      device.Clear(Color.Transparent);
      EcsSystem _system;
      for (int count = 0; count < _systems.Count; count++)
      {
        _system = _systems[count];
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
      NeedClear[index] = true;
    }

    /// <summary>
    /// 当一个实体通过该系统创建时发生.
    /// </summary>
    public event Action<Entity> OnCreate;

    public ConcurrentQueue<EcsCreateCommand> CreateCommands;

    public T MarkCreate<T>() where T : Entity, new()
    {
      T result;
      EcsCreateCommand cmd;
      for (int count = 0; count < Entities.Length; count++)
      {
        if (Entities[count] is not null || _reservedSlots.Contains(count))
          continue;
        result = new T();
        result.Ecs = this;
        result.ID = count;
        result.DoInitialize();
        _reservedSlots.Add(count);
        cmd = new EcsCreateCommand(this, result, count);
        CreateCommands.Enqueue(cmd);
        return result;
      }
      return null;
    }

    public void HandleCreate(EcsCreateCommand cmd)
    {
      if (Entities[cmd.ID] is null)
      {
        Entity result = cmd.Entity;
        OnCreate?.Invoke(result);
        Entities[cmd.ID] = result;
      }
      _reservedSlots.Remove(cmd.ID);
    }

    /// <summary>
    /// 当一个实体被放入该系统时发生.
    /// </summary>
    public event Action<Entity> OnPut;

    public Entity Put(Entity target)
    {
      for (int count = 0; count < Entities.Length; count++)
      {
        if (Entities[count] is null && _reservedSlots.Contains(count) is false)
        {
          Entities[count] = target;
          target.ID = count;
          target.Ecs = this;
          target.DoInitialize();
          OnPut?.Invoke(target);
          return target;
        }
      }
      return null;
    }

    public Entity Copy(Entity entity)
    {
      for (int count = 0; count < Entities.Length; count++)
      {
        if (Entities[count] is null && _reservedSlots.Contains(count) is false)
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

    public void LoadStep(TagCompound data)
    {
      EcsCreateCommand cmd;
      while (CreateCommands.Count > 0)
      {
        if (CreateCommands.TryDequeue(out cmd))
          HandleCreate(cmd);
      }
      //槽位改存稀疏列表: 谁存了档谁才有键, 没提名的槽位维持现状
      TagSeq slots = data.GetSeq("Slots");
      if (slots is null)
        return;
      for (int i = 0; i < slots.Count; i++)
      {
        TagCompound entry = slots.GetCompound(i);
        if (entry is null)
          continue;
        int slot = entry.GetInt("Slot", -1);
        TagCompound entityData = entry.GetCompound("Data");
        if (slot < 0 || slot >= Entities.Length || entityData is null)
          continue;
        LoadEntity(entityData, ref Entities[slot]);
      }
    }

    /// <summary>
    /// 引擎级通用口子: 读档遇到认不出的实体类型时, 把类型名和整包键值数据一起交给外部去接.
    /// <br>引擎不关心"接住之后是什么"(游戏侧一般拿它造占位实体); 没人接或者返回 null, 这格就空着, 读档不炸.</br>
    /// </summary>
    public static Func<string, TagCompound, Entity> CreateUnknownEntity;

    /// <summary>
    /// 实体从存档读回来之后给它一次"换身"的机会.
    /// <br>占位实体(模组缺席期间的替身)在模组装回来后, 靠它复原成真身;
    /// 返回 null 表示维持原样, 返回新实体则顶替原实体进槽位。</br>
    /// </summary>
    public static Func<Entity, Entity> UpgradeLoadedEntity;

    /// <summary>
    /// 把一个实体存成键值组; 实体为空或声明不需要存取时返回 null(调用方自己决定要不要放进列表).
    /// <br>实体类型直接存类型名字符串, 不再走哈希——哈希这东西谁都能算, 名字才认得出来是谁。</br>
    /// </summary>
    public static TagCompound SaveEntity(Entity entity)
    {
      if (entity is null || entity.NeedSaveAndLoad is false)
        return null;
      TagCompound data = new TagCompound();
      data["Type"] = entity.Identifier;
      entity.SaveStep(data);
      return data;
    }

    /// <summary>
    /// 从键值组把实体读进指定槽位.
    /// <br>槽里已有实体且类型对得上就原位续读, 对不上就按档里的类型名新造一个;
    /// 类型谁都不认识时交给 <see cref="CreateUnknownEntity"/>, 没人接这格就空着。</br>
    /// </summary>
    public static void LoadEntity(TagCompound data, ref Entity entity)
    {
      if (data is null)
        return;
      string typeName = data.GetString("Type");
      if (string.IsNullOrEmpty(typeName))
        return;
      //槽位里的旧实体和存档里这一格的类型对不上(模组被卸载/换过), 别硬往里读, 当空槽处理
      if (entity is not null && entity.Identifier != typeName)
        entity = null;
      if (entity is null)
      {
        entity = CodeResources<Entity>.TryCreateFromTypeName(typeName);
        if (entity is null)
        {
          entity = CreateUnknownEntity?.Invoke(typeName, data);
          if (entity is null)
          {
            Console.Log(ConsoleTextType.Warning, "Ecs",
              string.Concat("读档遇到认不出的实体类型 ", typeName ?? "未知", ", 且没有人接住, 这一格只能空着。"));
            return;
          }
        }
        entity.NeedSaveAndLoad = true;
        entity.DoInitialize();
      }
      entity.LoadStep(data);
      entity = UpgradeLoadedEntity?.Invoke(entity) ?? entity;
    }

    public void SaveStep(TagCompound data)
    {
      TagSeq slots = new TagSeq(TagType.Compound);
      for (int i = 0; i < Entities.Length; i++)
      {
        TagCompound entityData = SaveEntity(Entities[i]);
        if (entityData is null)
          continue;
        TagCompound entry = new TagCompound();
        entry["Slot"] = i;
        entry["Data"] = entityData;
        slots.Add(entry);
      }
      data["Slots"] = slots;
    }
  }
  public record EcsCreateCommand(
    Ecs Ecs,
    Entity Entity,
    int ID
    );
}