using Microsoft.Xna.Framework;

namespace Colin.Core
{
    /// <summary>
    /// 表示一个可被对象池加入的对象.
    /// </summary>
    public interface IPoolObject
    {
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
        /// 初始化对象.
        /// </summary>
        void Initialize( );
        /// <summary>
        /// 逻辑刷新.
        /// </summary>
        /// <param name="gameTime"></param>
        void Update( GameTime gameTime );
        /// <summary>
        /// 纹理绘制.
        /// </summary>
        /// <param name="gameTime"></param>
        void Draw( GameTime gameTime );
    }
}
