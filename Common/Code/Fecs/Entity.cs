using Microsoft.Xna.Framework.Graphics;

namespace Colin.Common.Code.Fecs
{
    /// <summary>
    /// 实体.
    /// </summary>
    public abstract class Entity : IEngineElement
    {
        #region ECS 组件系统部分

        /// <summary>
        /// 该实体目前拥有的组件.
        /// </summary>
        internal List<EntityComponent> Components { get; set; } = new List<EntityComponent>( );

        public int ComponentCount => Components.Count;

        /// <summary>
        /// 根据特定的 <seealso cref="EntityComponent"/> 类型, 从该 <seealso cref="Entity"/> 中判断是否可以获取到指定类型的 <seealso cref="EntityComponent"/>.
        /// </summary>
        /// <typeparam name="T">作为判断依据的 <seealso cref="EntityComponent"/> 类型.</typeparam>
        /// <returns>如若获取到了指定类型的 <seealso cref="EntityComponent"/>, 返回 <see href="true"/>, 否则返回 <see href="false"/>.</returns>
        public bool HasComponent<T>( ) where T : EntityComponent
        {
            return Components.Find( a => a._typeFullName == typeof( T ).FullName ) != null;
        }

        /// <summary>
        /// 根据具有指定引用的 <seealso cref="EntityComponent"/> 对象, 判断该 <seealso cref="Entity"/> 是否拥有该对象.
        /// </summary>
        /// <param name="component">作为判断依据的 <seealso cref="EntityComponent"/>.</param>
        /// <returns>如若该 <seealso cref="Entity"/> 拥有指定引用的 <seealso cref="EntityComponent"/>, 返回 <see href="true"/>, 否则返回 <see href="false"/>.</returns>
        public bool HasComponent( EntityComponent component )
        {
            return Components.Contains( component );
        }

        /// <summary>
        /// 从该 <seealso cref="Entity"/> 中获取指定类型的 <seealso cref="EntityComponent"/>.
        /// </summary>
        /// <typeparam name="T">要获取的特定类型的 <seealso cref="EntityComponent"/>.</typeparam>
        /// <returns></returns>
        public T GetComponent<T>( ) where T : EntityComponent
        {
            return (T)Components.Find( a => a._typeFullName == typeof( T ).FullName );
        }

        /// <summary>
        /// 根据索引从该 <seealso cref="Entity"/> 中获取指定 <seealso cref="EntityComponent"/>.
        /// </summary>
        /// <param name="index">索引.</param>
        /// <returns></returns>
        public EntityComponent GetComponent( int index )
        {
            return Components[ index ];
        }

        /// <summary>
        /// 向该 <seealso cref="Entity"/> 添加具有指定引用的 <seealso cref="EntityComponent"/>.
        /// </summary>
        /// <param name="component">要添加的具有指定引用的 <seealso cref="EntityComponent"/>.</param>
        public void AddComponent( EntityComponent component )
        {
            if ( !HasComponent( component ) || component.RepeatableAddition )
            {
                component.Entity = this;
                Components.Add( component );
            }
            else if ( HasComponent( component ) )
                throw new Exception( "名为: " + component.Name + " 的组件已经被添加过, 请检查某组件是否被重复地添加至不同的实体." );
        }

        /// <summary>
        ///  从该 <seealso cref="Entity"/> 删除具有指定引用的 <seealso cref="EntityComponent"/>.
        /// </summary>
        /// <param name="component">要删除的 <seealso cref="EntityComponent"/>.</param>
        /// <returns>若删除成功, 则返回 true, 否则返回 false.</returns>
        public bool RemoveComponent( EntityComponent component )
        {
            if ( !Components.Remove( component ) )
            {
                component.Entity = null;
                return true;
            }
            else
                return false;
        }

        public bool RemoveComponent<T>( ) where T : EntityComponent
        {
            for ( int count = 0; count < Components.Count; count++ )
            {
                if ( Components[ count ].GetType( ) == typeof( T ) )
                {
                    if ( !Components.Remove( Components[ count ] ) )
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 清空组件列表.
        /// </summary>
        public void ClearComponent( )
        {
            Components.Clear( );
        }

        #endregion

        public void DoInitialize( )
        {

        }

        public void DoUpdate( )
        {
            for ( int count = 0; count < Components.Count; count++ )
            {
                Components[ count ].Index = count;
                if ( Components[ count ].Enable )
                {
                    Components[ count ].Update( );
                }
            }
        }

        public void DoDraw( )
        {
            for ( int count = 0; count < Components.Count; count++ )
            {
                if ( Components[ count ].Visable )
                    Components[ count ].Draw( );
            }
        }

    }
}