namespace Colin.Common.Code.Fecs
{
    /// <summary>
    /// 实体组件; 提供一组基础的数据以供该组件所绑定的实体使用.
    /// <para>[!] 与传统的ECS设计模式中的组件不同, 本框架中的组件允许拥有自己的逻辑/绘制方法.</para>
    /// <para>[!] 将 <seealso cref="Enable"/> 的值设为 <see href="true"/> 并重载 <seealso cref="Update( )"/> 以启用组件的逻辑计算.</para>
    /// <para>[!] 将 <seealso cref="Enable"/> 的值设为 <see href="true"/> 并重载 <seealso cref="Update( )"/> 以启用组件的逻辑计算.</para>
    /// </summary>
    public abstract class EntityComponent
    {
        internal string? _typeFullName;

        /// <summary>
        /// 表示该组件在实体组件列表中的索引.
        /// </summary>
        public int Index = -1;

        /// <summary>
        /// 指示该组件是否可以被实体重复添加.
        /// </summary>
        public virtual bool RepeatableAddition { get; } = false;

        public Entity? Entity { get; internal set; }

        /// <summary>
        /// 表示该组件的名称.
        /// </summary>
        public virtual string? Name { get; }

        /// <summary>
        /// 指示该组件是否执行逻辑计算.
        /// <para>[!] 出于性能考虑, 该属性在组件实例化时的值为 <see href="false"/>.</para>
        /// <para>[!] 如若需要启用组件的逻辑计算, 请将该值设置为 <see href="true"/>.</para>
        /// </summary>
        public virtual bool Enable { get; } = false;

        /// <summary>
        /// 组件的逻辑计算; 重载该函数编写自定义操作, 并将 <seealso cref="Enable"/> 的值设为 <see href="true"/> 即可启用组件的逻辑计算.
        /// </summary>
        public virtual void Update( )
        {

        }

        /// <summary>
        /// 指示该组件是否执行绘制方法.
        /// <para>[!] 出于性能考虑, 该属性在组件实例化时的值为 <see href="false"/>.</para>
        /// <para>[!] 如若需要启用组件的绘制方法, 请将该值设置为 <see href="true"/>.</para>
        /// </summary>
        public virtual bool Visable { get; } = false;

        /// <summary>
        /// 组件的绘制方法; 重载该函数编写自定义操作, 并将 <seealso cref="Visable"/> 的值设为 <see href="true"/> 即可启用组件的逻辑计算.
        /// </summary>
        public virtual void Draw( )
        {

        }

        public EntityComponent( Entity entity )
        {
            _typeFullName = GetType( ).FullName;
            Entity = entity;
        }
    }
}