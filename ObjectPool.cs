using Microsoft.Xna.Framework;

namespace Colin
{
    /// <summary>
    /// 表示一个针对 <seealso cref="IPoolObject"/> 的对象池.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : IEmptyState where T : IPoolObject, IEmptyState, new()
    {
        public T[ ]? Objects { get; }

        public List<T>? ActiveList { get; }

        public bool Empty { get; set; } = false;

        public ObjectPool( int poolSize )
        {
            ActiveList = new List<T>( );
            Objects = new T[ poolSize ];
            Span<T> ts = Objects;
            T t = new T
            {
                Empty = true,
                ActiveIndex = -1,
                PoolIndex = -1
            };
            ts.Fill( t );
        }

        public virtual void Initialize( )
        {
            for ( int count = 0; count < Objects.Length; count++ )
                Objects[ count ].Initialize( );
        }

        public virtual void Update( GameTime gameTime )
        {
            if ( Empty )
                return;
            for ( int count = 0; count < ActiveList.Count; count++ )
            {
                ActiveList[ count ].Update( gameTime );
                if ( ActiveList[ count ].Empty )
                    DormancyObject( ActiveList[ count ] );
            }
        }

        public virtual void Draw( GameTime gameTime )
        {
            if ( Empty )
                return;
            for ( int count = 0; count < ActiveList.Count; count++ )
                ActiveList[ count ].Draw( gameTime );
        }

        /// <summary>
        /// 令指定 <seealso cref="IPoolObject"/> 活跃.
        /// </summary>
        /// <param name="index"></param>
        public void ActiveObject( int index )
        {
            Objects[ index ].Empty = false;
            Objects[ index ].ActiveIndex = ActiveList.Count;
            ActiveList.Add( Objects[ index ] );
        }

        /// <summary>
        /// 令指定 <seealso cref="IPoolObject"/> 休眠.
        /// </summary>
        /// <param name="element">指定的 <seealso cref="IPoolObject"/>.</param>
        public void DormancyObject( T element )
        {
            if ( ActiveList.Remove( element ) )
            {
                element.ActiveIndex = -1;
                element.Empty = true;
            }
        }
    }
}