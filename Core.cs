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
      // 初始化 Leemo.Assets 运行时资产管线;
      // 调试构建开启热重载, 文件一改即排队, 主线程 Update 里 PumpReloads 落地.
      // 调试构建且检测到仓库布局时, 资产根直接指向源目录 DeltaMachine.Assets/Assets ——
      // 加载与监视的都是你正在编辑的文件, 改完即热重载, 无需构建拷贝; 独立部署回退 <exe>/Assets.
      Assets.Init(GraphicsDevice, rootDir: CoreInfo.Debug ? FindDevAssetsRoot() : null, hotReload: CoreInfo.Debug);
      // 注册 Colin 自定义资产的加载器 (计算着色器 / .fx 特效源码, 均为进程内编译).
      Assets.Manager.RegisterLoader(new ComputeShaderLoader());
      Assets.Manager.RegisterLoader(new EffectSourceLoader());
      // 热重载结果进日志: 成功带出新实例, 失败保留旧资产并报错 (冒烟/调试期间可见).
      Assets.Manager.AssetReloaded += (path, type, _) =>
        Console.Log(ConsoleTextType.Remind, "Core", string.Concat("资产热重载: ", path, " (", type.Name, ")"));
      Assets.Manager.ReloadFailed += (path, ex) =>
        Console.Log(ConsoleTextType.Error, "Core", string.Concat("资产热重载失败 '", path, "': ", ex.Message));
      TargetElapsedTime = new TimeSpan(0, 0, 0, 0, (int)Math.Round(1000f / TargetFrame));
      Components.Add(Singleton.Get<ControllerResponder>());
      Components.Add(Singleton.Get<MouseResponder>());
      Components.Add(Singleton.Get<KeyboardResponder>());
      Components.Add(SpritePool.Instance);
      Components.Add(FileDropProcessor.Instance);
      DoInitialize();
      Console.Log(ConsoleTextType.Normal, "Core", CoreInfo.Tinter.Status);
      base.Initialize();
    }

    public virtual void DoInitialize() { }

    /// <summary>
    /// 向上搜索仓库源资产目录 (DeltaMachine.Assets/Assets, 以 Textures 子目录为存在标记).
    /// 未命中 (独立部署/异构布局) 返回 null, 资产根回退 <exe目录>/Assets.
    /// </summary>
    private static string FindDevAssetsRoot()
    {
      DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
      for (int depth = 0; dir is not null && depth < 8; depth++, dir = dir.Parent)
      {
        string candidate = Path.Combine(dir.FullName, "DeltaMachine.Assets", "Assets");
        if (Directory.Exists(Path.Combine(candidate, "Textures")))
          return candidate;
      }
      return null;
    }

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
      // 帧尖峰黑匣子, 自己用秒表测真实帧耗时, GameTime 在固定步长下是常数测不了尖峰
      HitchRecorder.OnFrame();
      TweenManager.Update();
      DebugProfiler.NextTick();
      Assets.Manager.PumpReloads();
      if (!Started)
      {
        SceneManager.SetScene(Preparator);
        Started = true;
      }
      CoreInfo.GetInformationFromDevice(gameTime);
      using (StageRecorder.Tag("Frame.SceneUpdate"))
        SceneManager.Update(gameTime);
      using (StageRecorder.Tag("Frame.GameUpdate"))
        DoUpdate();
      // 场景本体是 GameComponent, 真正的场景更新跑在 base.Update 里, 必须埋到这
      using (StageRecorder.Tag("Frame.BaseUpdate"))
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
      // 场景本体的渲染同样在 base.Draw 里, 和 base.Update 一起构成主线程两大桶
      using (StageRecorder.Tag("Frame.BaseDraw"))
        base.Draw(gameTime);
      using (StageRecorder.Tag("Frame.SceneRender"))
        DoRender();
      // 后渲染层, ImGui 这类覆盖层在这里画, 此时背板上已经是最终画面
      CoreInfo.PostRender?.Invoke(gameTime);
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