﻿namespace Colin.Common.Code.UI
{
    /// <summary>
    /// 指示当前操作指向的容器.
    /// </summary>
    public class ContainerPointer
    {
        /// <summary>
        /// 指示当前指针指向的容器在 <see cref="Container.ContainerItems"/> 的索引.
        /// </summary>
        public int Count;

        /// <summary>
        /// 指针指向的容器.
        /// </summary>
        public Container Container { get; private set; }

        /// <summary>
        /// 将指针移向上一个容器.
        /// </summary>
        public void LastControl( )
        {
            Count--;
            if( Count < 0 )
                Count = Container.GetActiveContainerElements( ).Count - 1;
            Container = Container.GetActiveContainerElements( )[Count];
            if( !Container.Events.CanGetForPointer )
                LastControl( );
        }

        /// <summary>
        /// 将指针移向下一个容器.
        /// </summary>
        public void NextControl( )
        {
            Count++;
            if( Count > Container.GetActiveContainerElements( ).Count - 1 )
                Count = 0;
            Container = Container.GetActiveContainerElements( )[Count];
            if( !Container.Events.CanGetForPointer )
                NextControl( );
        }

        public ContainerPointer( Container container )
        {
            Count = 0;
            Container = container;
        }

    }
}