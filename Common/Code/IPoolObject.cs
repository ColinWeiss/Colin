using Microsoft.Xna.Framework;

namespace Colin.Common.Code
{
    /// <summary>
    /// 表示一个可被对象池加入的对象.
    /// </summary>
    public interface IPoolObject
    {
        /// <summary>
        /// 指示该对象在活跃池中的活跃状态.
        /// </summary>
        bool Active { get; set; }

        /// <summary>
        /// 指示该对象在活跃池中的索引.
        /// </summary>
        int ActiveIndex { get; set; }

        /// <summary>
        /// 指示该对象在对象池中的索引.
        /// </summary>
        int PoolIndex { get; set; }

        /// <summary>
        /// 在令对象活跃时执行.
        /// </summary>
        void OnActive( );

        /// <summary>
        /// 在令对象休眠时执行.
        /// </summary>
        void OnDormancy( );

        /// <summary>
        /// 执行初始化相关操作.
        /// </summary>
        void DoInitialize( );

        /// <summary>
        /// 执行逻辑刷新相关操作.
        /// </summary>
        void DoUpdate( );

        /// <summary>
        /// 执行纹理绘制相关操作.
        /// </summary>
        void DoDraw( );
    }
}
