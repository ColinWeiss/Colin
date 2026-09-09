using Colin.Core.Common.Debugs;
using Colin.Core.Graphics.Bridge;
using Colin.Core.Graphics.Tinters;
using Colin.Core.Graphics.Tweens;
using Colin.Core.IO;
using Colin.Core.Preparation;
using System.Reflection;

namespace Colin.Core
{
  public class Core : Game
  {
    [System.Runtime.InteropServices.DllImport("nvapi64.dll", EntryPoint = "fake")]
    private static extern int LoadNvApi64();

    [System.Runtime.InteropServices.DllImport("nvapi.dll", EntryPoint = "fake")]
    private static extern int LoadNvApi32();

    private void TryForceHighPerformanceGpu()
    {
      try
      {
        if (System.Environment.Is64BitProcess)
          LoadNvApi64();
        else
          LoadNvApi32();
      }
      catch { } // this will always be triggered, so just catch it and do nothing :P
    }
    public CoreInfo Info;

    public bool Enable { get; set; } = true;

    public bool Visiable { get; set; } = true;

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
      TryForceHighPerformanceGpu();
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
      IsMouseVisible = false;
      IsFixedTimeStep = true;
    }

    protected override sealed void Initialize()
    {
      CoreInfo.Tinter = new TinterBridge(GraphicsDevice);
      CoreInfo.Batch = new SpriteBatch(CoreInfo.Graphics.GraphicsDevice);
      CoreInfo.Config = new Config();
      CoreInfo.Config.Load();
      // 初始化 Leemo.Assets 运行时资产管线 (rootDir 缺省为 <exe目录>/Assets, 与宿主工程的资源拷贝约定一致);
      // 调试构建开启热重载, 文件一改即排队, 主线程 Update 里 PumpReloads 落地.
      Assets.Init(GraphicsDevice, hotReload: CoreInfo.Debug);
      // 注册 Colin 自定义资产的加载器 (计算着色器 / .fx 特效源码, 均为进程内编译).
      Assets.Manager.RegisterLoader(new ComputeShaderLoader());
      Assets.Manager.RegisterLoader(new EffectSourceLoader());
      // 热重载结果进日志: 成功带出新实例, 失败保留旧资产并报错 (冒烟/调试期间可见).
      Assets.Manager.AssetReloaded += (path, type, _) =>
        Console.WriteLine("Remind", string.Concat("资产热重载: ", path, " (", type.Name, ")"));
      Assets.Manager.ReloadFailed += (path, ex) =>
        Console.WriteLine("Error", string.Concat("资产热重载失败 '", path, "': ", ex.Message));
      TargetElapsedTime = new TimeSpan(0, 0, 0, 0, (int)Math.Round(1000f / TargetFrame));
      Components.Add(Singleton.Get<ControllerResponder>());
      Components.Add(Singleton.Get<MouseResponder>());
      Components.Add(Singleton.Get<KeyboardResponder>());
      Components.Add(SpritePool.Instance);
      Components.Add(FileDropProcessor.Instance);
      DoInitialize();
      Console.WriteLine(CoreInfo.Tinter.Status);
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
      TweenManager.Update();
      DebugProfiler.NextTick();
      Assets.Manager.PumpReloads();
      if (!Started)
      {
        SceneManager.SetScene(Preparator);
        Started = true;
      }
      CoreInfo.GetInformationFromDevice(gameTime);
      SceneManager.Update(gameTime);
      DoUpdate();
      base.Update(gameTime);
    }
    public virtual void DoUpdate() { }

    protected override sealed void Draw(GameTime gameTime)
    {
      if (!Visiable)
        return;
      //GraphicsDevice.Clear(Color.Transparent);

      /*  CoreModule module;
        for (int count = 0; count < Modules.Count; count++)
        {
          module = Modules[count];
          module.DoRender(GraphicsDevice, CoreInfo.Batch);
        }*/
      CoreInfo.Tinter.BeginFrame();
      base.Draw(gameTime);
      DoRender();
    }
    public virtual void DoRender() { }

    /// <summary>
    /// 请求退出程序.
    /// <br>嵌入宿主模式下不会调用 MonoGame 的 <see cref="Game.Exit"/> (它会销毁引擎的独立窗口,
    /// 导致宿主渲染循环崩溃), 而是保存设置后通知宿主关闭宿主窗口.</br>
    /// </summary>
    public void RequestExit()
    {
      // 原生模式下 Exit 也会经 OnExiting 再存一次, 重复保存无害.
      CoreInfo.Config?.Save();
      if (CoreInfo.IsEmbedded)
      {
        CoreInfo.RaiseEmbeddedExitRequested();
        return;
      }
      Exit();
    }

    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
      CoreInfo.Config.Save();
      Assets.Shutdown();
      args.Cancel = false;
      base.OnExiting(sender, args);
    }

    public static bool Focus = true;

    protected override void OnActivated(object sender, EventArgs args)
    {
      Focus = true;
      base.OnActivated(sender, args);
    }
    protected override void OnDeactivated(object sender, EventArgs args)
    {
      Focus = false;
      base.OnDeactivated(sender, args);
    }
  }
}