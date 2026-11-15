using Colin.Core.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace Colin.Core.Preparation
{
  /// <summary>
  /// 启动预加载场景: 第一个正式场景 (<c>Driver.Start</c>) 之前, 把全量运行期资产与各预任务跑完.
  /// <br>[!] 本场景只负责"干活 + 报进度": 加载界面由常驻的 <c>Loader</c> 场景叠在上面显示, 每帧读
  /// <see cref="Running"/> / <see cref="Prompt"/> / <see cref="Progress"/> 刷新, 启动阶段不再拿灰屏顶着.</br>
  /// <br>进度状态是静态的: 写方只有本场景的后台任务, 读方是主线程的加载界面, 两个都是单实例语义.</br>
  /// </summary>
  public sealed class Preparator : Scene
  {
    public event Action OnLoadComplete;

    /// <summary>资产全目录扫描在总进度里的权重; 它是启动阶段最长的一段, 其余任务与代码资产表均分剩下的.</summary>
    private const float AssetScanWeight = 0.65f;

    /// <summary>启动预加载是否仍在进行 (主线程读: 加载界面据此决定是否显示自己).</summary>
    public static bool Running => _running;
    private static volatile bool _running;

    /// <summary>当前阶段的提示文本 (主线程读; 后台任务写).</summary>
    public static string Prompt => _prompt;
    private static volatile string _prompt = string.Empty;

    /// <summary>启动预加载总进度, 0 到 1 (主线程读; 后台任务写).</summary>
    public static float Progress => _progress;
    private static volatile float _progress;

    /// <summary>
    /// 加载界面尚未接上时的兜底底色.
    /// <br>游戏侧会在启动时把它换成加载界面的底色, 这样即使界面晚一帧出现, 闪过去的也不是原来的灰.</br>
    /// </summary>
    public Color ClearColor { get; set; } = Color.Gray;

    private List<IPreExecution> _preparatoryTasks = new List<IPreExecution>();
    public void RegisterPreparatoryTask<T>() where T : IPreExecution, new()
    {
      T t = new T();
      _preparatoryTasks.Add(t);
    }

    /// <summary>待主线程代跑的预任务; 主线程取走执行后自行清空, 再放闸.</summary>
    private static volatile IPreExecution _mainThreadTask;
    /// <summary>主线程任务的交接闸: 后台任务发布任务前落闸, 主线程跑完后放行.</summary>
    private static ManualResetEventSlim _mainThreadGate = new ManualResetEventSlim(false);
    /// <summary>主线程任务抛出的异常: 交回后台任务按启动失败处理.</summary>
    private static volatile Exception _mainThreadError;

    public override void SceneInit()
    {
      _running = true;
      _prompt = "预加载资产";
      _progress = 0f;
      _mainThreadError = null;
      Task.Run(PrepareAll);
      if (CoreInfo.Debug)
        Console.Log(ConsoleTextType.Remind, "Assets", "当前正以调试模式启动");
      base.SceneInit();
    }

    public override void Start()
    {
      base.Start();
    }

    /// <summary>
    /// 主线程代跑 <see cref="IPreExecution.OnMainThread"/> 任务.
    /// <br>在 Update 段执行, 此刻设备未绑定任何 RenderTarget, 读到的视口即背屏真值;
    /// 执行期间加载界面本帧不再重绘, 任务耗时就是这一帧的卡顿时长.</br>
    /// </summary>
    public override void SceneUpdate()
    {
      IPreExecution task = _mainThreadTask;
      if (task is null)
        return;
      try
      {
        task.Prepare();
      }
      catch (Exception exception)
      {
        _mainThreadError = exception;
      }
      finally
      {
        // 先清交接位再放闸: 放行后台后它才可能发布下一个任务, 本任务不会被重复执行
        _mainThreadTask = null;
        _mainThreadGate.Set();
      }
    }

    /// <summary>发布一个主线程任务并等待主线程代跑完; 失败时把异常带回后台线程.</summary>
    private static void RunOnMainThread(IPreExecution task)
    {
      _mainThreadGate.Reset();
      _mainThreadTask = task;
      _mainThreadGate.Wait();
      Exception error = _mainThreadError;
      if (error is not null)
      {
        _mainThreadError = null;
        throw new InvalidOperationException(string.Concat("主线程预任务 [", task.Name, "] 失败."), error);
      }
    }

    /// <summary>
    /// 后台预加载主体: 扫描资产 -> 逐个预任务 -> 建立代码资产表.
    /// <br>每一步都先写 <see cref="Prompt"/>, 所以加载界面上看到的永远是当前真正在跑的阶段.</br>
    /// </summary>
    private void PrepareAll()
    {
      try
      {
        int loaded = Assets.Manager.LoadDirectory(progress: OnAssetScanned);
        _progress = AssetScanWeight;
        Console.Log(ConsoleTextType.Remind, "Assets", string.Concat("运行时资产预加载完成: ", loaded, " 项."));

        // 尾段进度: 每个预任务 + 代码资产表各占一格, 不细分任务内部 (任务自己也不报细分进度).
        float stepWeight = (1f - AssetScanWeight) / Math.Max(1, _preparatoryTasks.Count + 1);
        float step = AssetScanWeight;
        for (int count = 0; count < _preparatoryTasks.Count; count++)
        {
          IPreExecution task = _preparatoryTasks[count];
          _prompt = string.IsNullOrWhiteSpace(task.Name) ? task.GetType().Name : task.Name;
          _progress = step;
          if (task.OnMainThread)
            RunOnMainThread(task);
          else
            task.Prepare();
          step = Math.Min(step + stepWeight, 1f);
          _progress = step;
        }

        _prompt = "建立代码资产表";
        _progress = step;
        CodeResources.Load();
        _progress = 1f;
        Console.Log(ConsoleTextType.Remind, "Assets", "初始化加载完成.");
      }
      catch (Exception exception)
      {
        // 启动失败不再退回无信息的灰屏: 把原因停在加载界面上, 日志与控制台错误流各留一份.
        _prompt = string.Concat("启动预加载失败: ", exception.Message);
        Console.Log(ConsoleTextType.Error, "Assets", string.Concat("启动预加载失败: ", exception));
        System.Console.Error.WriteLine(exception);
        return;
      }

      // 先通知游戏侧 (切场景 + 让加载界面再盖若干帧), 再把 Running 拉下:
      // 顺序反过来的话, 主线程可能恰好在这一帧看到"预加载已结束且没有遮盖", 界面会闪一下.
      try
      {
        OnLoadComplete?.Invoke();
      }
      catch (Exception exception)
      {
        Console.Log(ConsoleTextType.Error, "Assets", string.Concat("启动收尾失败: ", exception));
        System.Console.Error.WriteLine(exception);
      }
      finally
      {
        _running = false;
      }
    }

    /// <summary>资产扫描的进度探针: 把"已扫字节/总字节"折算到总进度的扫描段里.</summary>
    private static void OnAssetScanned(long done, long total)
      => _progress = total > 0 ? AssetScanWeight * done / total : 0f;

    public override void SceneRender()
    {
      CoreInfo.Graphics.GraphicsDevice.Clear(ClearColor);
      base.SceneRender();
    }
  }
}
