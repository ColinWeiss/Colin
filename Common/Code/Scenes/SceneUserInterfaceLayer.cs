using Colin.Common.Code.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Common.Code.Scenes
{
    /// <summary>
    /// 场景内容层: 场景用户交互界面内容层.
    /// </summary>
    public class SceneUserInterfaceLayer
    {
        internal ContainerPage ContainerPage { get; private set; }

        /// <summary>
        /// 获取该用户交互界面层所绑定的场景.
        /// </summary>
        public Scene? Scene { get; internal set; }

        /// <summary>
        /// 启用该用户交互界面的逻辑刷新相关操作.
        /// </summary>
        public bool UpdateEnable { get; set; } = true;

        /// <summary>
        /// 启用该用户交互界面的绘制相关操作.
        /// </summary>
        public bool DrawEnable { get; set; } = true;

        /// <summary>
        /// 获取最后一次响应左键单击的容器实例.
        /// </summary>
        public Container LeftClickContainer { get; private set; }

        /// <summary>
        /// 获取最后一次响应右键单击的容器实例.
        /// </summary>
        public Container RightClickContainer { get; private set; }

        public SceneUserInterfaceLayer( Scene scene ) { Scene = scene; }

        public void DoInitialize( )
        {
            ContainerPage = new ContainerPage( );
            ContainerPage.CanSeek = false;
            ContainerPage.ContainerElement.SetLayerout( 0, 0, EngineInfo.GameViewWidth, EngineInfo.GameViewHeight );
            Engine.Instance.Window.ClientSizeChanged += ( s, e ) =>
                ContainerPage.ContainerElement.SetLayerout( 0, 0, EngineInfo.GameViewWidth, EngineInfo.GameViewHeight );
            ContainerPage.DoInitialize( );
        }

        public void DoUpdate( )
        {
            ContainerPage.DoReset( );
            if ( Input.MouseLeftClick && ContainerPage.SeekAt( ) != null )
                LeftClickContainer = ContainerPage.SeekAt( );
            if ( Input.MouseRightClick && ContainerPage.SeekAt( ) != null )
                RightClickContainer = ContainerPage.SeekAt( );
            ContainerPage.SeekAt( )?.Events.Update( );
            ContainerPage.CanSeek = false;
            ContainerPage.DoUpdate( );
        }

        public void DoDraw( )
        {
            ContainerPage.DoDraw( );
        }

        public void Register( Container container )
        {
            container._scuiLayer = this;
            ContainerPage.Register( container );
        }

    }
}