using Colin.Core.Resources;
using System.Threading.Tasks;

namespace Colin.Core.Preparation
{
  public sealed class Preparator : Scene
  {
    public event Action OnLoadComplete;

    private List<IPreExecution> _preparatoryTasks = new List<IPreExecution>();
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
        Console.WriteLine("Remind", string.Concat("运行时资产预加载完成: ", loaded, " 项."));
        IPreExecution theTask;
        for (int count = 0; count < _preparatoryTasks.Count; count++)
        {
          theTask = _preparatoryTasks[count];
          theTask.Prepare();
        }
        CodeResources.Load();
        Console.WriteLine("Remind", "初始化加载完成.");
        OnLoadComplete?.Invoke();
      });

      if (CoreInfo.Debug)
        Console.WriteLine("Remind", "当前正以调试模式启动");
      base.SceneInit();
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