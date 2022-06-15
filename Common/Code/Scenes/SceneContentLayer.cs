using Colin.Common.Code.Fecs;
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

        /// <summary>
        /// 初始化一个场景内容层.
        /// </summary>
        /// <param name="scene">该内容层所属的场景.</param>
        public SceneContentLayer( Scene scene ) { Scene = scene; }

        /// <summary>
        /// 启用该用户交互界面的逻辑刷新相关操作.
        /// </summary>
        public bool UpdateEnable { get; set; } = true;

        /// <summary>
        /// 启用该用户交互界面的绘制相关操作.
        /// </summary>
        public bool RenderEnable { get; set; } = true;

        /// <summary>
        /// 该场景所有实体.
        /// </summary>
        public ObjectPool<Entity> Entities { get; protected set; } = new ObjectPool<Entity>( 1024 );

        public virtual void DoInitialize( )
        {

        }

        public virtual void DoUpdate( )
        {

        }

        public virtual void DoRender(  )
        {

        }

    }
}