namespace Colin.Core.Modulars.Ecses.Components
{
  /// <summary>
  /// 关于实体绘制信息的逻辑刷新方法.
  /// </summary>
  public delegate void EntityRenderUpdateFunction();

  /// <summary>
  /// 实体绘制方法; 返回绘制方法名称.
  /// </summary>
  public delegate void EntityRenderFunction(GraphicsDevice device, SpriteBatch batch);

  public record EntityRenderUpdateFunctionInfo(string Name, EntityRenderUpdateFunction Function);

  public record EntityAdvancedRenderFunctionInfo(string Name, EntityRenderFunction Function);

  public record EntityDeferredRenderFunctionInfo(string Name, EntityRenderFunction Function);

  /// <summary>
  /// 渲染数据组件.
  /// <br>包含Sprite、可见性等信息.</br>
  /// <br>需要自行订阅渲染方法.</br>
  /// </summary>
  public class EcsComRenderData : IEntityCom
  {
    public List<Sprite> Sprites = new();
    public SpriteEffects SpriteEffects;
    public Sprite Sprite
    {
      get => Sprites.Count > 0 ? Sprites[0] : null;
      set
      {
        if (Sprites.Count <= 0)
        {
          Sprites.Add(value);
        }
        Sprites[0] = value;
      }
    }
    public bool Visible = true;

    /// <summary>
    /// 渲染数据所需要进行的逻辑刷新.
    /// <br>[!] 禁止存放与绘制无关的逻辑!!!</br>
    /// </summary>
    public List<EntityRenderUpdateFunctionInfo> Updates;

    /// <summary>
    /// 高级渲染方法信息, 原则上允许在其中操作Batch.
    /// <br>通过名称注册、删除、查询、修改.</br>
    /// </summary>
    public List<EntityAdvancedRenderFunctionInfo> Advanceds;

    /// <summary>
    /// 合批渲染方法信息, 原则上禁止在其中操作Batch.
    /// <br>通过名称注册、删除、查询、修改.</br>
    /// </summary>
    public List<EntityDeferredRenderFunctionInfo> Deferreds;

    public EcsComRenderData()
    {
      Updates = new List<EntityRenderUpdateFunctionInfo>();
      Advanceds = new List<EntityAdvancedRenderFunctionInfo>();
      Deferreds = new List<EntityDeferredRenderFunctionInfo>();
    }

    public void DoInitialize() { }

    public void RegisterRenderUpdateFunction(string name, EntityRenderUpdateFunction function)
      => Updates.Add(new EntityRenderUpdateFunctionInfo(name, function));

    /// <summary>
    /// 注册具有指定名称的高级渲染方法.
    /// </summary>
    public void RegisterAdvancedRenderFunction(string name, EntityRenderFunction function)
      => Advanceds.Add(new EntityAdvancedRenderFunctionInfo(name, function));

    /// <summary>
    /// 注册具有指定名称的合批渲染方法.
    /// </summary>
    public void RegisterDeferredRenderFunction(string name, EntityRenderFunction function)
      => Deferreds.Add(new EntityDeferredRenderFunctionInfo(name, function));

    /// <summary>
    /// 删除具有对应名称的高级渲染方法.
    /// </summary>
    public void RemoveAdvancedRenderFunction(string name)
      => Advanceds.RemoveAt(Advanceds.FindIndex(x => x.Name == name));

    /// <summary>
    /// 删除具有对应名称的合批渲染方法.
    /// </summary>
    public void RemoveDeferredRenderFunction(string name)
      => Deferreds.RemoveAt(Deferreds.FindIndex(x => x.Name == name));

    /// <summary>
    /// 寻找具有对应名称的高级渲染方法.
    /// </summary>
    public EntityRenderFunction FindAdvancedRenderFunction(string name)
      => Advanceds.Find(x => x.Name == name)?.Function;

    /// <summary>
    /// 寻找具有对应名称的合批渲染方法.
    /// </summary>
    public EntityRenderFunction FindDeferredRenderFunction(string name)
      => Deferreds.Find(x => x.Name == name)?.Function;

    /// <summary>
    /// 用于自定义绘制场景中, 但无法使用合批功能, 需要自行操作Batch; 会先绘制 Deferreds 中方法, 然后再绘制 Advanceds 中方法.
    /// </summary>
    public void DoRender(GraphicsDevice device, SpriteBatch batch)
    {
      foreach (var deferred in Deferreds)
      {
        deferred.Function.Invoke(device, batch);
      }
      foreach (var advanced in Advanceds)
      {
        advanced.Function.Invoke(device, batch);
      }
    }
  }
}