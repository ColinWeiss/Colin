﻿using Colin.Core.Common;
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
        public EventHandler<BasicEventArgs> OnLoadComplete = ( s, e ) => { };

        public override void SceneInit()
        {
            LoadGameAssets();
            base.SceneInit();
        }

        private async void LoadGameAssets()
        {
            await Task.Run( () =>
            {
                IGameAsset asset;
                foreach(Type item in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if(item.GetInterfaces().Contains( typeof( IGameAsset ) ) && !item.IsAbstract)
                    {
                        asset = (IGameAsset)Activator.CreateInstance( item );
                        asset.LoadResource();
                        EngineConsole.WriteLine( ConsoleTextType.Remind, string.Concat( "正在加载 ", asset.Name ) );
                    }
                }
                EngineConsole.WriteLine( ConsoleTextType.Remind, "资源加载完成." );
                BasicEventArgs onResourceLoadComplete = new BasicEventArgs( "Event.GameResources.LoadComplete" );
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