using Colin.Core.IO;
using Colin.Core.Preparation;
using System.Reflection;

namespace Colin.Core
{
  public class Core : Game
  {
    public CoreInfo Info;

    public bool Enable { get; set; } = true;

    public bool Visiable { get; set; } = true;

    /// <summary>
    /// 指示当前活跃场景.
    /// </summary>
    public Scene CurrentScene { get; internal set; }

    public Preparator Preparator { get; private set; }

    private int _targetFrame = 60;
    /// <summary>
    /// 指示程序目标帧率.
    /// </summary>
    public int TargetFrame
    {
      get => _targetFrame;
      set => SetTargetFrame(value);
    }
    private void SetTargetFrame(int frame)
    {
      _targetFrame = frame;
      TargetElapsedTime = new TimeSpan(0, 0, 0, 0, (int)Math.Round(1000f / frame));
    }

    public Core()
    {
      //执行程序检查程序.
      IProgramChecker checker;
      foreach (Type item in Assembly.GetExecutingAssembly().GetTypes())
      {
        if (!item.IsAbstract && item.GetInterfaces().Contains(typeof(IProgramChecker)))
        {
          checker = (IProgramChecker)Activator.CreateInstance(item);
          checker.Check();
        }
      }
      CoreInfo.Init(this);
      if (CoreInfo.Graphics == null)
      {
        CoreInfo.Graphics = new GraphicsDeviceManager(this)
        {
          PreferHalfPixelOffset = false,
          HardwareModeSwitch = false,
          SynchronizeWithVerticalRetrace = true,
          PreferMultiSampling = true,
          GraphicsProfile = GraphicsProfile.HiDef
        };
      }
      Content.RootDirectory = "Content";
      IsMouseVisible = false;
      IsFixedTimeStep = true;
    }

    /// <summary>
    /// 切换场景.
    /// </summary>
    /// <param name="scene">要切换到的场景对象.</param>
    public void SetScene(Scene scene)
    {
      if (scene is null)
      {
        if (CurrentScene.InitializeOnSwitch)
        {
          Window.ClientSizeChanged -= CurrentScene.InitRenderTarget;
          Window.OrientationChanged -= CurrentScene.InitRenderTarget;
        }
        Components.Remove(CurrentScene);
        CurrentScene = null;
        Components.Clear();
        Components.Add(Singleton.Get<ControllerResponder>());
        Components.Add(Singleton.Get<MouseResponder>());
        Components.Add(Singleton.Get<KeyboardResponder>());
        Components.Add(SpritePool.Instance);
        GC.Collect();
        return;
      }
      if (CurrentScene != null)
      {
        if (CurrentScene.InitializeOnSwitch)
        {
          Window.ClientSizeChanged -= CurrentScene.InitRenderTarget;
          Window.OrientationChanged -= CurrentScene.InitRenderTarget;
        }
        Components.Remove(CurrentScene);
      }
      CurrentScene?.Dispose();
      Components.Clear();
      Components.Add(Singleton.Get<ControllerResponder>());
      Components.Add(Singleton.Get<MouseResponder>());
      Components.Add(Singleton.Get<KeyboardResponder>());
      Components.Add(SpritePool.Instance);
      Components.Add(scene);
      CurrentScene = scene;
      GC.Collect();
    }

    protected override sealed void Initialize()
    {
      CoreInfo.Batch = new SpriteBatch(CoreInfo.Graphics.GraphicsDevice);
      CoreInfo.DrawBatcherAlt = new(CoreInfo.Graphics.GraphicsDevice);
      CoreInfo.Config = new Config();
      CoreInfo.Config.Load();
      TargetElapsedTime = new TimeSpan(0, 0, 0, 0, (int)Math.Round(1000f / TargetFrame));
      Components.Add(FileDropProcessor.Instance);
      DoInitialize();
      base.Initialize();
    }
    public virtual void DoInitialize() { }

    protected override sealed void LoadContent()
    {
      Preparator = new Preparator();
      Load();
      Preparator.OnLoadComplete += Start;
      base.LoadContent();
    }
    public virtual void Load() { }

    /// <summary>
    /// 在程序开始运行时执行.
    /// </summary>
    public virtual void Start() { }

    private bool Started = false;
    protected override sealed void Update(GameTime gameTime)
    {
      if (!Enable)
        return;
      Time.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
      if (!Started)
      {
        SetScene(Preparator);
        Started = true;
      }
      CoreInfo.GetInformationFromDevice(gameTime);
      DoUpdate();
      base.Update(gameTime);
    }
    public virtual void DoUpdate() { }

    protected override sealed void Draw(GameTime gameTime)
    {
      if (!Visiable)
        return;
      GraphicsDevice.Clear(Color.Black);
      base.Draw(gameTime);
      DoRender();
    }
    public virtual void DoRender() { }

    protected override void OnExiting(object sender, EventArgs args)
    {
      CoreInfo.Config.Save();
      base.OnExiting(sender, args);
    }

    public static bool OnActive = true;

    protected override void OnActivated(object sender, EventArgs args)
    {
      OnActive = true;
      base.OnActivated(sender, args);
    }
    protected override void OnDeactivated(object sender, EventArgs args)
    {
      OnActive = false;
      base.OnDeactivated(sender, args);
    }
  }
}