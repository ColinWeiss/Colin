using Colin.Core.Common.Debugs;
using Colin.Core.IO;
using Colin.Core.Modulars;

namespace Colin.Core.Common
{
  /// <summary>
  /// 场景.
  /// </summary>
  public class Scene : DrawableGameComponent, IScene, IOStep
  {
    public string Name { get; set; }

    public SceneCamera Camera;

    private SceneModuleList _components;
    /// <summary>
    /// 获取场景模块列表.
    /// </summary>
    public SceneModuleList Modules => _components;

    /// <summary>
    /// 若为场景添加了 <see cref="Business"/> 模块, 则可通过该属性获取该模块引用.
    /// </summary>
    public Business Business => Modules.GetModule<Business>();

    /// <summary>
    /// 指示场景在切换时是否执行初始化的值.
    /// </summary>
    public bool InitializeOnSwitch = true;

    /// <summary>
    /// 场景本身的 RenderTarget.
    /// </summary>
    public RenderTarget2D SceneRenderTarget;

    public ModulePostProcessor ModulePostProcessor = new ModulePostProcessor();

    public SceneEvents Events;

    public Scene() : base(CoreInfo.Core)
    {
      Events = new SceneEvents();
      // 仅此一处管理Game.Window事件, 其他地方都用Scene.Event统一进行管理, 不需要单独删除
    }

    public override sealed void Initialize()
    {
      Started = false;
      if (InitializeOnSwitch)
      {
        InitRenderTarget(this, new EventArgs());
        Events.OrientationChanged += InitRenderTarget;
        Events.ClientSizeChanged += InitRenderTarget;
        _components = new SceneModuleList(this);
        _components.Add(Events);
        _components.Add(Camera = new SceneCamera());
        SceneInit();
        CoreInfo.Core.Window.ClientSizeChanged += Events.InvokeSizeChange;
        CoreInfo.Core.Window.OrientationChanged += Events.InvokeSizeChange;
        CoreInfo.IMEHandler?.TextInput += Events.OnTextInput;
      }
      base.Initialize();
    }

    internal void InitRenderTarget(object s, EventArgs e)
    {
      SceneRenderTarget?.Dispose();
      SceneRenderTarget = RenderTargetExt.CreateDefault();
    }

    /// <summary>
    /// 执行场景初始化.
    /// </summary>
    public virtual void SceneInit() { }

    internal bool Started = false;
    public override sealed void Update(GameTime gameTime)
    {
      {
        if (!Started)
        {
          Start();
          Modules.DoStart();
          Started = true;
        }
        UpdatePreset();
        Modules.DoUpdate(gameTime);
        SceneUpdate();
        base.Update(gameTime);
      }
    }
    public virtual void Start() { }
    public virtual void UpdatePreset() { }
    public virtual void SceneUpdate() { }

    private bool _skipRender = true;
    private bool _renderStarted = false;
    private int _sizeMismatchFrames;
    private (int rtW, int rtH, int vw, int vh) _sizeMismatchState;
    public override sealed void Draw(GameTime gameTime)
    {
      {
        if (_skipRender is true)
        {
          _skipRender = false;
          return;
        }
        else if (_renderStarted is false)
        {
          RenderStart();
          _renderStarted = true;
        }
        SceneRenderPreset();
        Modules.DoRender(CoreInfo.Batch);
        SceneRender();
        CoreInfo.Graphics.GraphicsDevice.SetRenderTarget(null);
        // 自愈: 场景若曾带错位尺寸的 RT (如历史版本在后台线程构建场景时读到 Draw 段绑定的 RT 尺寸),
        // 渲染全程会被拉伸糊掉且无人纠正. 此刻已解绑 RT, Viewport 必为背屏真值.
        // 错位要连续多帧且形态不变才广播重建: 拖拽缩放期间事件与背屏更新交错, 瞬时错位形态逐帧不同会自行收敛,
        // 只有"冻结的错位"才需要走 InvokeSizeChange 全场景重建; 重建当帧跳过呈现, 下一帧起用新 RT.
        int viewWidth = CoreInfo.ViewWidth;
        int viewHeight = CoreInfo.ViewHeight;
        if (SceneRenderTarget is null || SceneRenderTarget.IsDisposed
          || SceneRenderTarget.Width != viewWidth || SceneRenderTarget.Height != viewHeight)
        {
          (int, int, int, int) mismatch = (SceneRenderTarget?.Width ?? 0, SceneRenderTarget?.Height ?? 0, viewWidth, viewHeight);
          if (mismatch == _sizeMismatchState)
            _sizeMismatchFrames++;
          else
          {
            _sizeMismatchState = mismatch;
            _sizeMismatchFrames = 1;
          }
          if (_sizeMismatchFrames >= 5 && viewWidth > 0 && viewHeight > 0)
          {
            _sizeMismatchFrames = 0;
            Events.InvokeSizeChange(this, EventArgs.Empty);
            // 广播链里的重建回调可能绑定过 RT (如巨图携带), 呈现前必须解绑
            CoreInfo.Graphics.GraphicsDevice.SetRenderTarget(null);
            base.Draw(gameTime);
            return;
          }
        }
        else
        {
          _sizeMismatchFrames = 0;
        }
        CoreInfo.Batch.Begin();
        CoreInfo.Batch.Draw(SceneRenderTarget, new Rectangle(0, 0, CoreInfo.ViewWidth, CoreInfo.ViewHeight), Color.White);
        CoreInfo.Batch.End();
        base.Draw(gameTime);
      }
    }
    public virtual void RenderStart() { }
    public virtual void SceneRenderPreset() { }
    public virtual void SceneRender() { }

    /// <summary>
    /// 根据指定类型获取场景模块.
    /// </summary>
    /// <typeparam name="T">指定的 <see cref="ISceneModule"/> 类型.</typeparam>
    /// <returns>如果成功获取, 那么返回指定对象, 否则返回 <see langword="null"/>.</returns>
    public T GetModule<T>() where T : ISceneModule => Modules.GetModule<T>();

    /// <summary>
    /// 根据指定类型获取场景渲染模块.
    /// </summary>
    /// <typeparam name="T">指定的 <see cref="IRenderableISceneModule"/> 类型.</typeparam>
    /// <returns>如果成功获取, 那么返回指定对象, 否则返回 <see langword="null"/>.</returns>
    public T GetRenderModule<T>() where T : IRenderableISceneModule => Modules.GetRenderModule<T>();

    /// <summary>
    /// 根据指定类型删除场景模块.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>如果成功删除, 那么返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
    public bool RemoveModule<T>() where T : ISceneModule => Modules.RemoveModule<T>();

    /// <summary>
    /// 根据指定对象删除场景模块.
    /// </summary>
    /// <returns>如果成功删除, 那么返回 <see langword="true"/>, 否则返回 <see langword="false"/>.</returns>
    public bool RemoveModule(ISceneModule module) => Modules.Remove(module);

    protected override void Dispose(bool disposing)
    {
      Modules.Dispose();
      SceneRenderTarget?.Dispose();
      if (CoreInfo.Core.Window is not null)
      {
        CoreInfo.Core.Window.ClientSizeChanged -= Events.InvokeSizeChange;
        CoreInfo.Core.Window.OrientationChanged -= Events.InvokeSizeChange;
        CoreInfo.IMEHandler?.TextInput -= Events.OnTextInput;
      }
      base.Dispose(disposing);
    }

    public void LoadStep(TagCompound data)
    {
      LoadScene(data.GetCompound("Scene"));
      ISceneModule module;
      for (int count = 0; count < Modules.Count; count++)
      {
        module = Modules.ElementAt(count).Value;
        if (module is IOStep io)
        {
          //模块各占一个键(键名是模块类型全名); 档里还没有这个键(新模块碰上老存档)就保持默认
          TagCompound moduleData = data.GetCompound(module.GetType().FullName);
          if (moduleData is null)
            continue;
          io.LoadStep(moduleData);
        }
      }
      LoadModulesPost(data.GetCompound("Post"));
    }
    public virtual void LoadScene(TagCompound data) { }
    public virtual void LoadModulesPost(TagCompound data) { }

    public void SaveStep(TagCompound data)
    {
      TagCompound sceneData = new TagCompound();
      SaveScene(sceneData);
      data["Scene"] = sceneData;
      TagCompound moduleData;
      ISceneModule module;
      for (int count = 0; count < Modules.Count; count++)
      {
        module = Modules.ElementAt(count).Value;
        if (module is IOStep io)
        {
          moduleData = new TagCompound();
          io.SaveStep(moduleData);
          data[module.GetType().FullName] = moduleData;
        }
      }
      TagCompound postData = new TagCompound();
      SaveModulesPost(postData);
      data["Post"] = postData;
    }
    public virtual void SaveScene(TagCompound data) { }
    public virtual void SaveModulesPost(TagCompound data) { }
  }
}