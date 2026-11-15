using Colin.Core.Resources;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Colin.Core.Preparation
{
  public sealed class Preparator : Scene
  {
    public event Action OnLoadComplete;

    private List<IPreExecution> _preparatoryTasks = new List<IPreExecution>();
    private readonly ConcurrentQueue<IMainThreadPreExecution> _mainThreadTasks = new ConcurrentQueue<IMainThreadPreExecution>();
    private volatile bool _assetLoadComplete;
    public void RegisterPreparatoryTask<T>() where T : IPreExecution, new()
    {
      T t = new T();
      _preparatoryTasks.Add(t);
    }

    public override void SceneInit()
    {
      Task assetLoadTask = null;
      assetLoadTask = Task.Run(
      () =>
      {
        // 运行时资产预加载: 扫描 <exe>/Assets 全目录, 命中已注册加载器的文件全部载入缓存.
        int loaded = Assets.Manager.LoadDirectory();
        Console.Log(ConsoleTextType.Remind, "Assets", string.Concat("运行时资产预加载完成: ", loaded, " 项."));
        IPreExecution theTask;
        for (int count = 0; count < _preparatoryTasks.Count; count++)
        {
          theTask = _preparatoryTasks[count];
          if (theTask is IMainThreadPreExecution mainThreadTask)
            _mainThreadTasks.Enqueue(mainThreadTask);
          else
            theTask.Prepare();
        }
        CodeResources.Load();
        Console.Log(ConsoleTextType.Remind, "Assets", "初始化加载完成.");
        _assetLoadComplete = true;
      });

      if (CoreInfo.Debug)
        Console.Log(ConsoleTextType.Remind, "Assets", "当前正以调试模式启动");
      base.SceneInit();
    }

    public override void UpdatePreset()
    {
      while (_mainThreadTasks.TryDequeue(out IMainThreadPreExecution task))
      {
        try
        {
          task.PrepareMainThread();
        }
        catch (Exception exception)
        {
          Console.Log(ConsoleTextType.Error, "Warmup", string.Concat(task.Name, ": ", exception));
        }
      }

      if (_assetLoadComplete && _mainThreadTasks.IsEmpty)
      {
        _assetLoadComplete = false;
        OnLoadComplete?.Invoke();
      }

      base.UpdatePreset();
    }

    public override void Start()
    {
      base.Start();
    }

    public override void SceneRender()
    {
      CoreInfo.Graphics.GraphicsDevice.Clear(Color.Gray);
      base.SceneRender();
    }
  }
}