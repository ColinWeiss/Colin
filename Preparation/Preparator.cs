using Colin.Core.Resources;
using System.Reflection;
using System.Threading;
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
      if (CoreInfo.DebugEnable)
        Console.WriteLine("Remind", "当前正以调试模式启动");
      Asset.LoadAssets();
      Task.Run(
        () =>
        {
          LoadGameAssets();
          IPreExecution theTask;
          for (int count = 0; count < _preparatoryTasks.Count; count++)
          {
            theTask = _preparatoryTasks[count];
            theTask.Prepare();
          }
          CodeResourceManager.LoadCodeResource();
          Console.WriteLine("Remind", "初始化加载完成.");
          OnLoadComplete?.Invoke();
        });
      base.SceneInit();
    }

    private void LoadGameAssets()
    {
      IGameAsset asset;
      foreach (Type item in Assembly.GetExecutingAssembly().GetTypes())
      {
        if (item.GetInterfaces().Contains(typeof(IGameAsset)) && !item.IsAbstract)
        {
          asset = (IGameAsset)Activator.CreateInstance(item);
          asset.LoadResource();
          Console.WriteLine(string.Concat("正在加载 ", asset.Name));
        }
      }
    }
    public override void SceneRender()
    {
      CoreInfo.Graphics.GraphicsDevice.Clear(Color.Gray);
      base.SceneRender();
    }
  }
}