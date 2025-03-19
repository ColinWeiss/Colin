using Colin.Core.Common.Debugs;
using Colin.Core.IO;

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
    /// 指示场景是否可以被回收.
    /// <br>[!] 默认为 <see langword="true"/>, 若需要控制 Dispose 时序, 则需要先设为 <see langword="false"/>, 再按需操作为 <see langword="true"/>.</br>
    /// </summary>
    public bool CanDispose { get; set; }

    /// <summary>
    /// 指示场景在切换时是否执行初始化的值.
    /// </summary>
    public bool InitializeOnSwitch = true;

    /// <summary>
    /// 场景本身的 RenderTarget.
    /// </summary>
    public RenderTarget2D SceneRenderTarget;

    public ScreenReprocess ScreenReprocess = new ScreenReprocess();

    public SceneEvents Events;

    public Scene() : base(CoreInfo.Core)
    {
      CanDispose = true;
      Events = new SceneEvents();
      // 仅此一处管理Game.Window事件, 其他地方都用Scene.Event统一进行管理, 不需要单独删除
    }

    public override void Initialize()
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
        CoreInfo.IMEHandler.TextInput += Events.OnTextInput;
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
      using (DebugProfiler.Tag("Update-" + GetType().Name))
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
    public override sealed void Draw(GameTime gameTime)
    {
      using (DebugProfiler.Tag("Draw-" + GetType().Name))
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
        CoreInfo.IMEHandler.TextInput -= Events.OnTextInput;
      }
      base.Dispose(disposing);
    }

    public void LoadStep(BinaryReader reader)
    {
      ISceneModule module;
      for (int count = 0; count < Modules.Count; count++)
      {
        module = Modules.ElementAt(count).Value;
        if (module is IOStep io)
        {
          io.LoadStep(reader);
        }
      }
    }

    public void SaveStep(BinaryWriter writer)
    {
      ISceneModule module;
      for (int count = 0; count < Modules.Count; count++)
      {
        module = Modules.ElementAt(count).Value;
        if (module is IOStep io)
        {
          io.SaveStep(writer);
        }
      }
    }
  }
}