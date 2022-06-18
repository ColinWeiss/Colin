using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Colin.Common.Code.UI
{
    /// <summary>
    /// 指示顶层容器的事件.
    /// </summary>
    public class ContainerEvents
    {
        /// <summary>
        /// 指示该容器是否启用拖动功能.
        /// </summary>
        public bool Drop = false;

        /// <summary>
        /// 若启用拖动功能, 记录当前容器是否正处于拖动状态.
        /// </summary>
        public bool Droping = false;

        /// <summary>
        /// 若启用拖动功能, 在第一帧按下时记录光标在容器内的位置.
        /// </summary>
        public Vector2 SelectPoint;

        /// <summary>
        /// 指示该容器是否会被父容器的指针寻找到.
        /// </summary>
        public bool CanGetForPointer = false;

        public Container Container { get; private set; }

        public ContainerEvents( Container container )
        {
            Container = container;
            OnMouseLeftClick += MouseLeftClickEvent;
            OnMouseLeftDown += MouseLeftDownEvent;
            OnMouseLeftUp += MouseLeftUpEvent;
            OnMouseRightClick += MouseRightClickEvent;
            OnMouseRightDown += MouseRightDownEvent;
            OnMouseRightUp += MouseRightUpEvent;
            OnInterview += InterviewEvent;
            OnInterviewStateChange += InterviewStateChangeEvent;
        }

        /// <summary>
        /// 获取当前容器是否处于可交互状态的值.
        /// </summary>
        public bool Interview { get; private set; } = false;

        /// <summary>
        /// 发生容器的可交互状态发生改变时.
        /// </summary>
        public event Action OnInterviewStateChange;
        /// <summary>
        /// 由执行器调用: 执行自定义的操作于当前容器可交互状态更改时.
        /// </summary>
        public void DoInterviewStateChangeEvent( ) => OnInterviewStateChange.Invoke( );
        /// <summary>
        /// 在容器的可交互状态发生改变时执行.
        /// </summary>
        protected virtual void InterviewStateChangeEvent( )
        {
        }

        /// <summary>
        /// 发生在容器于可交互状态下, 光标左键单击时.
        /// </summary>
        public event Action OnMouseLeftClick;
        /// <summary>
        /// 由执行器调用: 执行自定义的操作于当前容器处于可交互状态下时, 光标左键单击时.
        /// </summary>
        public void DoMouseLeftClickEvent( )
        {
            if( Drop )
            {
                Droping = true;
                SelectPoint = new Vector2(Mouse.GetState( ).X,Mouse.GetState( ).Y) - Container.Location;
            }
            OnMouseLeftClick.Invoke( );
        }
        /// <summary>
        /// 在容器于可交互状态下, 光标左键单击时执行.
        /// </summary>
        protected virtual void MouseLeftClickEvent( )
        {
        }

        /// <summary>
        /// 发生在容器于可交互状态下, 光标左键长按时.
        /// </summary>
        public event Action OnMouseLeftDown;
        /// <summary>
        /// 由执行器调用: 执行自定义的操作于当前容器处于可交互状态下时, 光标左键长按时.
        /// </summary>
        public void DoMouseLeftDownEvent( )
        {
            OnMouseLeftDown.Invoke( );
        }
        /// <summary>
        /// 在容器于可交互状态下, 光标左键长按时执行.
        /// </summary>
        protected virtual void MouseLeftDownEvent( )
        {
        }

        /// <summary>
        /// 发生在容器于可交互状态下, 光标左键抬起时.
        /// </summary>
        public event Action OnMouseLeftUp;
        /// <summary>
        /// 由执行器调用: 执行自定义的操作于当前容器处于可交互状态下时, 光标左键抬起时.
        /// </summary>
        public void DoMouseLeftUpEvent( )
        {
            OnMouseLeftUp.Invoke( );
        }
        /// <summary>
        /// 在容器于可交互状态下, 光标左键抬起时执行.
        /// </summary>
        protected virtual void MouseLeftUpEvent( )
        {
        }

        /// <summary>
        /// 发生在容器处于可交互状态下时.
        /// </summary>
        public event Action OnInterview;
        /// <summary>
        /// 由执行器调用: 执行自定义的操作于当前容器处于可交互状态下时.
        /// </summary>
        public void DoInterviewEvent( ) => OnInterview.Invoke( );
        /// <summary>
        /// 在容器处于可交互状态下时执行.
        /// </summary>
        protected virtual void InterviewEvent( )
        {
        }

        /// <summary>
        /// 发生在容器于可交互状态下, 光标右键单击时.
        /// </summary>
        public event Action OnMouseRightClick;
        /// <summary>
        /// 由执行器调用: 执行自定义的操作于当前容器处于可交互状态下时, 光标右键单击时.
        /// </summary>
        public void DoMouseRightClickEvent( )
        {
            OnMouseRightClick.Invoke( );
        }
        /// <summary>
        /// 在容器于可交互状态下, 光标右键单击时执行.
        /// </summary>
        protected virtual void MouseRightClickEvent( )
        {
        }

        /// <summary>
        /// 发生在容器于可交互状态下, 光标右键长按时.
        /// </summary>
        public event Action OnMouseRightDown;
        /// <summary>
        /// 由执行器调用: 执行自定义的操作于当前容器处于可交互状态下时, 光标右键长按时.
        /// </summary>
        public void DoMouseRightDownEvent( )
        {
            OnMouseRightDown.Invoke( );
        }
        /// <summary>
        /// 在容器于可交互状态下, 光标右键长按时执行.
        /// </summary>
        protected virtual void MouseRightDownEvent( )
        {
        }

        /// <summary>
        /// 发生在容器于可交互状态下, 光标右键抬起时.
        /// </summary>
        public event Action OnMouseRightUp;
        /// <summary>
        /// 由执行器调用: 执行自定义的操作于当前容器处于可交互状态下时, 光标右键抬起时.
        /// </summary>
        public void DoMouseRightUpEvent( )
        {
            OnMouseRightUp.Invoke( );
        }
        /// <summary>
        /// 在容器于可交互状态下, 光标右键抬起时执行.
        /// </summary>
        protected virtual void MouseRightUpEvent( )
        {
        }

        public virtual void Update( )
        {
            if( Input.MouseReleased && Drop && Droping )
                Droping = false;
            Interview = Container.GetInterviewState( );
            if( Interview )
                DoInterviewEvent( );
            if( Input.MouseLeftClick )
                DoMouseLeftClickEvent( );
            else if( Input.MouseLeftDown && Container._scuiLayer.LeftClickContainer.Equals(Container) )
                DoMouseLeftDownEvent( );
            else if( Input.MouseLeftUp && Container._scuiLayer.LeftClickContainer.Equals(Container) )
                DoMouseLeftUpEvent( );
            if( Input.MouseRightClick )
                DoMouseRightClickEvent( );
            else if( Input.MouseRightDown && Container._scuiLayer.RightClickContainer.Equals(Container) )
                DoMouseRightDownEvent( );
            else if( Input.MouseRightUp && Container._scuiLayer.RightClickContainer.Equals(Container) )
                DoMouseRightUpEvent( );

        }
    }
}