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

    public void LoadStep(BinaryReader reader)
    {
      EcsCreateCommand cmd;
      while (CreateCommands.Count > 0)
      {
        if (CreateCommands.TryDequeue(out cmd))
          HandleCreate(cmd);
      }
      for (int i = 0; i < Entities.Length; i++)
      {
        LoadEntity(reader, ref Entities[i]);
      }
    }

    /// <summary>
    /// 游戏侧挂钩:判定一个实体类型是不是"模组源"的。
    /// <br>模组源实体的组件流外面包一层长度前缀,读档端碰到"模组已卸载、查不到类型"
    /// 时才能把载荷整块跳过,转成占位实体;游戏设置(见 ModItem.InstallEcsHooks)。</summary>
    public static Func<Type, bool> IsModSourceType;

    /// <summary>
    /// 游戏侧挂钩:读档遇到查不到的哈希时,用它转出一个占位实体。
    /// <br>传进来的读档流正停在载荷的长度前缀上,兜底方自己负责把长度和载荷读干净;
    /// 没挂这个钩子又查不到哈希,读档就只能报错了。</br>
    /// <br>这条链路对一切实体通用,不挑物品还是别的——查不到就是查不到,怎么接是游戏侧的事。</br>
    /// </summary>
    public static Func<int, BinaryReader, Entity> CreateFallbackEntity;

    /// <summary>
    /// 游戏侧挂钩:实体从存档读回来之后给它一次"换身"的机会。
    /// <br>占位实体(模组缺席期间的替身)在模组装回来后,靠它复原成真身;
    /// 返回 null 表示维持原样,返回新实体则顶替原实体进槽位。</br>
    /// </summary>
    public static Func<Entity, Entity> UpgradeLoadedEntity;

    public static void LoadEntity(BinaryReader reader, ref Entity entity)
    {
      if (reader.ReadBoolean() is false)
        return;
      int hashValue = reader.ReadInt32();
      if (entity is not null)
      {
        //槽位里的旧实体和存档里这一格的类型对不上(模组被卸载/换过),别硬往里读,当空槽处理。
        try
        {
          if (CodeResources<Entity>.GetHashFromTypeName(entity.Identifier) != hashValue)
            entity = null;
        }
        catch
        {
          //旧实体的程序集已经随模组卸载回收了,连类型名都拿不出来,同样当空槽。
          entity = null;
        }
      }
      if (entity is null)
      {
        entity = CodeResources<Entity>.TryCreateNewInstance(hashValue);
        if (entity is not null)
        {
          if (IsModSourceType?.Invoke(entity.GetType()) == true)
            reader.ReadInt32(); //模组源实体的组件流带长度前缀,先读掉再按组件读。
          entity.NeedSaveAndLoad = true;
          entity.DoInitialize();
          entity.LoadStep(reader);
          entity = UpgradeLoadedEntity?.Invoke(entity) ?? entity;
        }
        else if (CreateFallbackEntity is not null)
        {
          entity = CreateFallbackEntity(hashValue, reader);
          if (entity is not null)
            entity.NeedSaveAndLoad = true;
        }
        else
        {
          throw new InvalidDataException($"读档遇到查不到的实体哈希 {hashValue},且没有配置占位转换。");
        }
      }
      else
      {
        if (IsModSourceType?.Invoke(entity.GetType()) == true)
          reader.ReadInt32(); //同上,模组源实体先吃掉长度前缀。
        entity.NeedSaveAndLoad = true;
        entity.LoadStep(reader);
        entity = UpgradeLoadedEntity?.Invoke(entity) ?? entity;
      }
    }
    public static void SaveEntity(BinaryWriter writer, Entity entity)
    {
      if (entity is null || entity.NeedSaveAndLoad is false)
      {
        writer.Write(false);
        return;
      }
      writer.Write(true);
      int? hash = CodeResources<Entity>.GetHashFromTypeName(entity.Identifier);
      //模组源实体的组件流可长可短(模组自定义组件),外面统一包一层长度,
      //读档端碰到"模组已卸载、查不到类型"才能整块跳过,不再像以前那样读档直接炸。
      bool external = IsModSourceType?.Invoke(entity.GetType()) == true;
      if (hash.HasValue is false)
      {
        //类型不在表里(模组卸了一半之类的悬空实体):按类型名现算哈希并按外部格式存,
        //读档端反正也查不到它,会把这段载荷整块跳过。
        hash = entity.Identifier.GetMsnHashCode();
        external = true;
      }
      writer.Write(hash.Value);
      if (external)
      {
        using MemoryStream buffer = new MemoryStream();
        //leaveOpen 必须给:BinaryWriter 默认 Dispose 时连底下的流一起关,
        //关完再取 buffer.Length 就是 ObjectDisposedException(存档实测踩过)。
        using (BinaryWriter buffered = new BinaryWriter(buffer, Encoding.UTF8, leaveOpen: true))
          entity.SaveStep(buffered);
        writer.Write((int)buffer.Length);
        writer.Write(buffer.ToArray());
      }
      else
      {
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
  public record EcsCreateCommand(
    Ecs Ecs,
    Entity Entity,
    int ID
    );
}