namespace Colin.Core.Common
{
    /// <summary>
    /// <see cref="SceneComponentList"/> 中的事件使用的参数.
    /// </summary>
    public sealed class SceneComponentListEventArgs : EventArgs
    {
        private ISceneComponent _sceneMode;

        /// <summary>
        /// 受事件影响的场景模块.
        /// </summary>
        public ISceneComponent Mode => _sceneMode;

        /// <summary>
        /// 新建 <see cref="SceneComponentListEventArgs"/> 实例.
        /// </summary>
        /// <param name="sceneMode">受事件影响的场景模块.</param>
        public SceneComponentListEventArgs( ISceneComponent sceneMode )
        {
            _sceneMode = sceneMode;
        }

    }
}