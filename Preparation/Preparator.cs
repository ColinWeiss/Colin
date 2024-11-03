using Colin.Core.Resources;
using System.Diagnostics;
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
      CanDispose = false;
      if (CoreInfo.Debug)
        Console.WriteLine("Remind", "当前正以调试模式启动");
      Asset.LoadAssets();
      Task assetLoadTask = null;
      assetLoadTask = Task.Run(
      (Action)(() =>
      {
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
        CanDispose = true;
      }));
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
    public override void SceneRender()
    {
      CoreInfo.Graphics.GraphicsDevice.Clear(Color.Gray);
      base.SceneRender();
    }
  }
}