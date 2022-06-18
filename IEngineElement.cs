namespace Colin
{
    /// <summary>
    /// 表示一个引擎中可执行初始化、逻辑刷新、绘制相关操作的对象.
    /// </summary>
    public interface IEngineElement
    {
        /// <summary>
        /// 执行对象初始化相关操作.
        /// </summary>
        public void DoInitialize( );

        /// <summary>
        /// 执行对象逻辑刷新相关操作.
        /// </summary>
        public void DoUpdate( );

        /// <summary>
        /// 执行对象绘制相关操作.
        /// </summary>
        public void DoRender( );
    }
}