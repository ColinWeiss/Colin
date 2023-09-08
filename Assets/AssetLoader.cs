using Colin.Core.Common;
using Colin.Core.Events;
using System.Reflection;
using System.Threading.Tasks;

namespace Colin.Core.Assets
{
    /// <summary>
    /// 游戏资产加载器.
    /// </summary>
    public sealed class AssetLoader : Scene
    {
        public EventHandler<BasicEvent> OnLoadComplete = ( s, e ) => { };

        public override void SceneInit()
        {
            LoadGameAssets();
            base.SceneInit();
        }

        private async void LoadGameAssets()
        {
            await Task.Run( () =>
            {
                IGameResource asset;
                foreach(Type item in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if(item.GetInterfaces().Contains( typeof( IGameResource ) ) && !item.IsAbstract)
                    {
                        asset = (IGameResource)Activator.CreateInstance( item );
                        asset.LoadResource();
                        EngineConsole.WriteLine( ConsoleTextType.Remind, string.Concat( "正在加载 ", asset.Name ) );
                    }
                }
                EngineConsole.WriteLine( ConsoleTextType.Remind, "资源加载完成." );
                BasicEvent onResourceLoadComplete = new BasicEvent();
                onResourceLoadComplete.name = "Event_GameResources_LoadComplete";
                OnLoadComplete?.Invoke( this, onResourceLoadComplete );
            } );
        }

        public override void SceneRender()
        {
            EngineInfo.Graphics.GraphicsDevice.Clear( Color.Gray );
            base.SceneRender();
        }
    }
}