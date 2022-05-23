using Microsoft.Xna.Framework;

namespace Colin.Common.Code.Scenes
{
    /// <summary>
    /// 表示一个场景中的内容层.
    /// </summary>
    public class SceneContentLayer
    {
        /// <summary>
        /// 获取该内容层所绑定的场景.
        /// </summary>
        public Scene? Scene { get; internal set; }

        public SceneContentLayer( Scene scene ) { Scene = scene; }

        /// <summary>
        /// 启用该用户交互界面的逻辑刷新相关操作.
        /// </summary>
        public bool UpdateEnable { get; set; } = true;

        /// <summary>
        /// 启用该用户交互界面的绘制相关操作.
        /// </summary>
        public bool DrawEnable { get; set; } = true;

        public virtual void DoInitialize( )
        {

        }

        public virtual void DoUpdate( )
        {

        }

        public virtual void DoDraw(  )
        {

        }

    }
}