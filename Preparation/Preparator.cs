using Colin.Core.Resources;
using System.Reflection;
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
        Asset.LoadAssets();
        LoadGameAssets();
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

    private void LoadGameAssets()
    {
      try
      {
        IGameAsset asset;
        foreach (Type item in Assembly.GetExecutingAssembly().GetTypes())
        {
          if (item.GetInterfaces().Contains(typeof(IGameAsset)) && !item.IsAbstract)
          {
            asset = (IGameAsset)Activator.CreateInstance(item);
            asset.LoadResource();
            Console.WriteLine(string.Concat("正在加载", asset.Name));
          }
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
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