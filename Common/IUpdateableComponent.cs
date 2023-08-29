namespace Colin.Core.Common
{
    /// <summary>
    /// 表示一个可执行逻辑计算的组件.
    /// </summary>
    public interface IUpdateableComponent
    {
        bool Enable { get; set; }
        int UpdateOrder { get; }
        public virtual void DoUpdate( GameTime gameTime ) { }
    }
}