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
    public class SceneUILayer
    {
        Container Container;

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

        public SceneUILayer( Scene scene ) { Scene = scene; }

        public void DoInitialize( )
        {
            Container = new Container( );
            Container.CanSeek = false;
            Container.ContainerElement.SetLayerout( 0, 0, EngineInfo.GameViewWidth, EngineInfo.GameViewHeight );
            Engine.Instance.Window.ClientSizeChanged += ( s, e ) =>
                Container.ContainerElement.SetLayerout( 0, 0, EngineInfo.GameViewWidth, EngineInfo.GameViewHeight );
            Container.DoInitialize( );
        }

        public void DoUpdate( )
        {
            Container.SeekAt( )?.Events.Update( );
            Container.DoUpdate( );
        }

        public void DoDraw( )
        {
            Container.DoDraw( );
        }

        public void Register( Container container ) => Container.Register( container );

    }
}