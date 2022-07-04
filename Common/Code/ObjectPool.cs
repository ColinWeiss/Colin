namespace Colin.Common.Code
{
    /// <summary>
    /// 表示一个针对 <seealso cref="IPoolObject"/> 的对象池.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : IEngineElement where T : IPoolObject, new()
    {
        /// <summary>
        /// 对象池缓存池.
        /// </summary>
        public T[ ]? Objects { get; }

        /// <summary>
        /// 对象池活跃对象列表.
        /// </summary>
        public List<T>? ActiveList { get; }

        /// <summary>
        /// 指示当前对象池是否启用逻辑刷新的值.
        /// </summary>
        public bool Enable = true;

        /// <summary>
        /// 指示当前对象池是否启用渲染的值.
        /// </summary>
        public bool Visiable = true;

        /// <summary>
        /// 实例化一个对象池.
        /// </summary>
        /// <param name="poolSize">对象池缓存池大小.</param>
        public ObjectPool( int poolSize )
        {
            ActiveList = new List<T>( );
            Objects = new T[poolSize];
            Span<T> ts = Objects;
            T t = new T
            {
                Active = false,
                ActiveIndex = -1,
                PoolIndex = -1
            };
            ts.Fill(t);
        }

        public virtual void DoInitialize( )
        {
            for( int count = 0; count < Objects.Length; count++ )
                Objects[count].DoInitialize( );
        }

        public virtual void DoUpdate( )
        {
            if( !Enable )
                return;
            for( int count = 0; count < ActiveList.Count; count++ )
            {
                ActiveList[count].DoUpdate( );
                if( !ActiveList[count].Active )
                    DormancyObject(ActiveList[count]);
            }
        }

        public virtual void DoRender( )
        {
            if( !Visiable )
                return;
            for( int count = ActiveList.Count - 1; count >= 0; count-- )
                ActiveList[count].DoRender( );
        }

        /// <summary>
        /// 令列表内指定索引的 <seealso cref="IPoolObject"/> 活跃.
        /// </summary>
        /// <param name="index"></param>
        public void ActiveObject( int index )
        {
            Objects[index].Active = true;
            Objects[index].ActiveIndex = ActiveList.Count;
            ActiveList.Add(Objects[index]);
        }

        /// <summary>
        /// 令具有指定引用的 <seealso cref="IPoolObject"/> 休眠.
        /// </summary>
        /// <param name="element">指定的 <seealso cref="IPoolObject"/>.</param>
        public void DormancyObject( T element )
        {
            if( ActiveList.Remove(element) )
            {
                element.ActiveIndex = -1;
                element.Active = false;
            }
        }

        /// <summary>
        /// 令列表内指定索引的 <seealso cref="IPoolObject"/> 休眠.
        /// </summary>
        /// <param name="index"></param>
        public void DormancyObject( int index )
        {
            ActiveList[index].ActiveIndex = -1;
            ActiveList[index].Active = false;
            ActiveList.RemoveAt(index);
        }

    }
}