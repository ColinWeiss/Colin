using Colin.Core.Resources;
using System.Reflection;
using System.Threading.Tasks;

namespace Colin.Core.Common
{
    /// <summary>
    /// 程序预备器.
    /// </summary>
    public sealed class Preparator : Scene
    {
        public event Action OnLoadComplete;

        private List<Preliminary> _preparatoryTasks = new List<Preliminary>();
        public void RegisterPreparatoryTask<T>() where T : Preliminary, new()
        {
            T t = new T();
            _preparatoryTasks.Add(t);
        }

        public override async void SceneInit()
        {
            await Task.Run(LoadGameAssets);
            Preliminary theTask;
            for (int count = 0; count < _preparatoryTasks.Count; count++)
            {
                theTask = _preparatoryTasks[count];
                await Task.Run(theTask.Prepare);
            }
            await Task.Run(CodeResourceManager.LoadCodeResource);
            EngineConsole.WriteLine(ConsoleTextType.Remind, "初始化加载完成.");
            OnLoadComplete?.Invoke();
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
                    EngineConsole.WriteLine(ConsoleTextType.Remind, string.Concat("正在加载 ", asset.Name));
                }
            }
        }

        public override void SceneRender()
        {
            EngineInfo.Graphics.GraphicsDevice.Clear(Color.Gray);
            base.SceneRender();
        }
    }
}