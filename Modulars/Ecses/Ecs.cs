using Colin.Core.Common.Debugs;
using Colin.Core.Events;
using Colin.Core.Extensions;
using Colin.Core.Graphics.PBR;
using Colin.Core.IO;
using Colin.Core.Mathematical;
using Colin.Core.Modulars.Ecses.Components;
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
  public class Ecs : SceneRenderModule, IOStep, IPbrRenderScene
  {
    private TypeSnapshotTable<EcsSystem> _systems;
    public TypeSnapshotTable<EcsSystem> Systems => _systems;

    public PbrRenderer Renderer { get; private set; }

    public PbrCamera Camera { get; } = new PbrCamera();

    public PbrLighting Lighting { get; set; } = PbrLighting.CreateDefault();

    public bool DrawSkybox { get; set; }

    /// <summary>ECS 3D 的像素滤镜倍率: 3D 渲染到 1/2 分辨率, 再用 Point 采样放大, 形成 2px 像素块.</summary>
    public const int PbrPixelSize = 2;

    /// <summary>普通 2D 渲染所在的 Z 平面. 3D 的 Depth 大于它时在 2D 之上, 小于它时被 2D 遮挡.</summary>
    public const float Scene2DZ = 0f;

    public RenderTarget2D PbrBackRt { get; private set; }

    public RenderTarget2D PbrFrontRt { get; private set; }

    /// <summary>2px 像素滤镜的目标: 3D 先全分辨率渲染/上光, 再缩小到这里, 最后 Point 放大.</summary>
    public RenderTarget2D PbrPixelRt { get; private set; }

    private Texture2D _pbrEnvironment;
    private readonly HashSet<PbrModel> _pbrUpdatedModels = new HashSet<PbrModel>();

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
      if (CoreInfo.Headless is false && CoreInfo.Graphics?.GraphicsDevice is not null)
      {
        Renderer = new PbrRenderer(CoreInfo.Graphics.GraphicsDevice);
        Renderer.GameLighting = true;
        Renderer.DrawSkybox = false;
      }
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
        UpdatePbrModels((float)time.ElapsedGameTime.TotalSeconds);
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

    /// <summary>接入离线烘焙的环境图; 只用于 3D 环境反射, 不作为可见天空盒.</summary>
    public void SetEnvironment(Texture2D source)
    {
      _pbrEnvironment?.Dispose();
      _pbrEnvironment = PbrTextureFactory.CreateMipMapped(CoreInfo.Graphics.GraphicsDevice, source);
      Lighting.EnvironmentTexture = _pbrEnvironment;
    }

    private void UpdatePbrModels(float deltaSeconds)
    {
      if (deltaSeconds <= 0f)
        return;
      _pbrUpdatedModels.Clear();
      for (int i = 0; i < Entities.Length; i++)
      {
        Entity entity = Entities[i];
        EcsComPbrRender render = entity?.GetCom<EcsComPbrRender>();
        if (render?.Model is null || _pbrUpdatedModels.Add(render.Model) is false)
          continue;
        render.Model.Update(deltaSeconds);
      }
    }

    /// <summary>
    /// 渲染一层 ECS 3D. front=false 画在 2D 之下 (Depth &lt;= 0), front=true 画在 2D 之上 (Depth &gt; 0).
    /// <br>由 LightingAdapter 在 2D 合成前/后分别调用, 保证 3D 能按 Z=0 平面和 2D 正确分层.</br>
    /// </summary>
    public void RenderPbrLayer(bool front)
    {
      if (Renderer is null || Scene?.Camera is null)
        return;

      EnsurePbrTargets();
      RenderTarget2D target = front ? PbrFrontRt : PbrBackRt;
      if (target is null)
        return;

      GraphicsDevice device = CoreInfo.Graphics.GraphicsDevice;
      device.SetRenderTarget(target);
      device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1f, 0);

      // 全分辨率渲染, 让 3D 相机和 2D 相机在 z=0 处严格对齐; 2px 滤镜在合成时做.
      Camera.SyncToSceneCamera(Scene.Camera, target.Width, target.Height);
      Renderer.GameLighting = true;
      Renderer.DrawSkybox = false;
      Renderer.Begin(Camera, Lighting);

      for (int i = 0; i < Entities.Length; i++)
      {
        Entity entity = Entities[i];
        EcsComPbrRender render = entity?.GetCom<EcsComPbrRender>();
        if (render?.Model is null || render.Visible is false)
          continue;
        bool isFront = render.Depth > Scene2DZ;
        if (isFront != front)
          continue;

        BoundingBox bounds = render.Model.Bounds;
        Vector3 center = (bounds.Min + bounds.Max) * 0.5f;
        Vector2 world = entity.Transform.Translation + render.Offset;
        Vector3 position = new Vector3(world.X, -world.Y, render.Depth);
        Matrix transform =
            Matrix.CreateTranslation(-center.X, -bounds.Min.Y, -center.Z) *
            Matrix.CreateScale(render.Scale) *
            Matrix.CreateRotationY(render.Yaw) *
            Matrix.CreateTranslation(position);
        Renderer.DrawModel(render.Model, transform);
      }

      Renderer.End();
    }

    private void EnsurePbrTargets()
    {
      int width = CoreInfo.ViewWidth;
      int height = CoreInfo.ViewHeight;
      int pixelWidth = Math.Max(1, width / PbrPixelSize);
      int pixelHeight = Math.Max(1, height / PbrPixelSize);
      if (PbrBackRt is not null && PbrBackRt.Width == width && PbrBackRt.Height == height &&
          PbrPixelRt is not null && PbrPixelRt.Width == pixelWidth && PbrPixelRt.Height == pixelHeight)
        return;

      PbrBackRt?.Dispose();
      PbrFrontRt?.Dispose();
      PbrPixelRt?.Dispose();
      PbrBackRt = RenderTargetExt.CreateWithDepth(width, height);
      PbrFrontRt = RenderTargetExt.CreateWithDepth(width, height);
      PbrPixelRt = RenderTargetExt.CreateDefault(pixelWidth, pixelHeight);
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

    /// <summary>
    /// 实体创建拦截点: 返回 false 表示本次创建被外部接管(不分配槽位, 返回 null).
    /// <br>通用能力: 给"生成权威在别处"的场景用(联机客户端只接受服务器下发, 回放只接受录制流...), 引擎不关心谁在拦.</br>
    /// </summary>
    public Func<Type, bool> CreateInterceptor;

    public bool CanCreate(Type type) => CreateInterceptor is null || CreateInterceptor(type);

    public T MarkCreate<T>() where T : Entity, new()
    {
      if (CanCreate(typeof(T)) is false)
        return null;
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
      if (target is not null && CanCreate(target.GetType()) is false)
        return null;
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
      Renderer?.Dispose();
      Renderer = null;
      PbrBackRt?.Dispose();
      PbrBackRt = null;
      PbrFrontRt?.Dispose();
      PbrFrontRt = null;
      PbrPixelRt?.Dispose();
      PbrPixelRt = null;
      _pbrEnvironment?.Dispose();
      _pbrEnvironment = null;
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