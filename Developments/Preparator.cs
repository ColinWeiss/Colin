using Colin.Core.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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
            foreach(Type item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if(!item.IsAbstract && item.GetInterfaces().Contains( typeof( IProgramChecker ) ))
                {
                    checker = (IProgramChecker)Activator.CreateInstance( item );
                    checker.Check();
                }
            }
        }

        public override void SceneInit()
        {
            DoCheck();
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
                OnLoadComplete?.Invoke( );
            } );
        }

        public override void SceneRender()
        {
            EngineInfo.Graphics.GraphicsDevice.Clear( Color.Gray );
            base.SceneRender();
        }
    }
}