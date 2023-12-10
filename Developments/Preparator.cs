using Colin.Core.Resources;
using System.Reflection;
using System.Threading.Tasks;

namespace Colin.Core.Developments
{
    /// <summary>
    /// 程序预备器.
    /// </summary>
    public class Preparator : Scene
    {
        public event Action OnLoadComplete;

        /// <summary>
        /// 执行程序检查流程.
        /// </summary>
        public static void DoCheck()
        {
            IProgramChecker checker;
            foreach (Type item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!item.IsAbstract && item.GetInterfaces().Contains( typeof( IProgramChecker ) ))
                {
                    checker = (IProgramChecker)Activator.CreateInstance( item );
                    checker.Check();
                }
            }
        }

        public override async void SceneInit()
        {
            DoCheck();
            await Task.Run( LoadGameAssets );
            await Task.Run( LoadCodeResource );

            EngineConsole.WriteLine( ConsoleTextType.Remind, "初始化加载完成." );
            OnLoadComplete?.Invoke();
            base.SceneInit();
        }

        private void LoadGameAssets()
        {
            IGameAsset asset;
            foreach (Type item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (item.GetInterfaces().Contains( typeof( IGameAsset ) ) && !item.IsAbstract)
                {
                    asset = (IGameAsset)Activator.CreateInstance( item );
                    asset.LoadResource();
                    EngineConsole.WriteLine( ConsoleTextType.Remind, string.Concat( "正在加载 ", asset.Name ) );
                }
            }
        }

        private static List<Type> CodeResourceTypes = new List<Type>();
        public void RegisterCodeResourceType<T>()
        {
            if (typeof( T ).IsNotPublic || typeof( T ).IsGenericType || typeof( T ).IsEnum || typeof( T ).IsValueType)
                EngineConsole.WriteLine( ConsoleTextType.Error, "为代码资产列表注册类型失败: " + typeof( T ).Name );
            else
                CodeResourceTypes.Add( typeof( T ) );
        }

        private void LoadCodeResource()
        {
            foreach (Type item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!item.IsAbstract && CodeResourceTypes.Contains( item ) && item.GetInterfaces().Contains( typeof( ICodeResource ) ))
                {
                    Type resources = typeof( CodeResources<> );
                    Type resource = resources.MakeGenericType( item );
                    resource.GetMethod( "Load" ).Invoke( Activator.CreateInstance( resource ), null );
                }
            }
        }

        public override void SceneRender()
        {
            EngineInfo.Graphics.GraphicsDevice.Clear( Color.Gray );
            base.SceneRender();
        }
    }
}