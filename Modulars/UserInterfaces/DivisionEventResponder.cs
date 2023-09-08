using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 划分元素事件响应器.
    /// </summary>
    public class DivisionEventResponder
    {
        public Division Division;

        public DivisionEventResponder( Division division ) => Division = division;

        /// <summary>
        /// 事件: 发生于交互点悬停于该划分元素上持续激活交互状态时.
        /// </summary>
        public event EventHandler<DivisionEvent> HoverOn;

        /// <summary>
        /// 事件: 发生于鼠标与之交互, 左键单击按下时.
        /// </summary>
        public event EventHandler<DivisionEvent> MouseLeftClickBefore;

        /// <summary>
        /// 事件: 发生于鼠标与之交互, 左键长按时.
        /// </summary>
        public event EventHandler<DivisionEvent> MouseLeftDown;

        /// <summary>
        /// 事件: 发生于鼠标与之交互, 左键单击抬起时.
        /// </summary>
        public event EventHandler<DivisionEvent> MouseLeftClickAfter;

        /// <summary>
        /// 事件: 发生于鼠标与之交互, 左键松开时.
        /// </summary>
        public event EventHandler<DivisionEvent> MouseLeftUp;

        /// <summary>
        /// 事件: 发生于鼠标与之交互, 右键单击按下时.
        /// </summary>
        public event EventHandler<DivisionEvent> MouseRightClickBefore;

        /// <summary>
        /// 事件: 发生于鼠标与之交互, 右键长按时.
        /// </summary>
        public event EventHandler<DivisionEvent> MouseRightDown;

        /// <summary>
        /// 事件: 发生于鼠标与之交互, 右键单击抬起时.
        /// </summary>
        public event EventHandler<DivisionEvent> MouseRightClickAfter;

        /// <summary>
        /// 事件: 发生于鼠标与之交互, 右键松开时.
        /// </summary>
        public event EventHandler<DivisionEvent> MouseRightUp;

        /// <summary>
        /// 事件: 发生于鼠标双键松开时.
        /// </summary>
        public event EventHandler<DivisionEvent> MouseRelease;

        /// <summary>
        /// 事件: 发生于划分元素开始拖拽状态时.
        /// </summary>
        public event EventHandler<DivisionEvent> DragStart;

        /// <summary>
        /// 事件: 发生于划分元素拖拽状态时.
        /// </summary>
        public event EventHandler<DivisionEvent> Dragging;

        /// <summary>
        /// 事件: 发生于划分元素结束拖拽状态时.
        /// </summary>
        public event EventHandler<DivisionEvent> DragEnd;

        /// <summary>
        /// 事件: 发生于鼠标滑轮滑动时.
        /// </summary>
        public event EventHandler<DivisionEvent> MousePulleySliding;

        /// <summary>
        /// 事件: 发生于划分元素启用交互状态时.
        /// </summary>
        public event EventHandler<DivisionEvent> ActivationStart;

        /// <summary>
        /// 事件: 发生于划分元素结束交互状态时.
        /// </summary>
        public event EventHandler<DivisionEvent> ActivationEnd;

        /// <summary>
        /// 事件: 发生于划分元素获得焦点时.
        /// </summary>
        public event EventHandler<DivisionEvent> ObtainingFocus;

        /// <summary>
        /// 事件: 发生于划分元素失去焦点时.
        /// </summary>
        public event EventHandler<DivisionEvent> LosesFocus;

        /// <summary>
        /// 指示拖拽状态.
        /// </summary>
        public bool DraggingState = false;

        public bool Invariable = false;

        MouseState _mouseState = new MouseState();

        MouseState _mouseStateLast = new MouseState();

        public void Execute()
        {
            _mouseState = MouseResponder.State;
            _mouseStateLast = MouseResponder.StateLast;
            DivisionEvent divEvent = new DivisionEvent( Division );
            if(Division.Interact.Interaction)
            {
                if(!Division.Interact.InteractionLast)
                {
                    divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Activation_ActivationStart" );
                    ActivationStart?.Invoke( this, divEvent );
                }
                divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_MouseHoverOn" );
                HoverOn?.Invoke( this, divEvent );
                if(MouseResponder.MouseLeftClickBeforeFlag)
                {
                    Invariable = true;
                    UserInterface.Focus = Division;
                    divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_MouseLeftClickBefore" );
                    MouseLeftClickBefore?.Invoke( this, divEvent );
                    if(Division.Interact.IsDraggable && Division.Interact.InteractionLast)
                    {
                        divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_DragStart" );
                        DragStart?.Invoke( this, divEvent );
                        DraggingState = true;
                    }
                }
                if(MouseResponder.MouseLeftDownFlag)
                {
                    divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_MouseLeftDown" );
                    MouseLeftDown?.Invoke( this, divEvent );
                }
                if(MouseResponder.MouseLeftClickAfterFlag && Invariable)
                {
                    Invariable = false;
                    divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_MouseLeftClickAfter" );
                    MouseLeftClickAfter?.Invoke( this, divEvent );
                }
                if(MouseResponder.MouseLeftUpFlag)
                {
                    divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_MouseLeftUp" );
                    MouseLeftUp?.Invoke( this, divEvent );
                    DraggingState = false;
                }
                if(_mouseState.RightButton == ButtonState.Pressed && _mouseStateLast.RightButton == ButtonState.Released)
                {
                    Invariable = true;
                    divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_MouseRightClickBefore" );
                    MouseRightClickBefore?.Invoke( this, divEvent );
                }
                if(_mouseState.RightButton == ButtonState.Pressed && _mouseStateLast.RightButton == ButtonState.Pressed)
                {
                    divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_MouseRightDown" );
                    MouseRightDown?.Invoke( this, divEvent );
                }
                if(_mouseState.RightButton == ButtonState.Released && _mouseStateLast.RightButton == ButtonState.Pressed && Invariable)
                {
                    Invariable = false;
                    divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_MouseRightClickAfter" );
                    MouseRightClickAfter?.Invoke( this, divEvent );
                }
                if(_mouseState.RightButton == ButtonState.Released && _mouseStateLast.RightButton == ButtonState.Released)
                {
                    divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_MouseRightUp" );
                    MouseRightUp?.Invoke( this, divEvent );
                }
                if(_mouseState.ScrollWheelValue != _mouseStateLast.ScrollWheelValue)
                {
                    divEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_MousePulleySliding" );
                    MousePulleySliding?.Invoke( this, divEvent );
                }
            }
        }

        public void Independent()
        {
            _mouseStateLast = _mouseState;
            _mouseState = Mouse.GetState();
            DivisionEvent divisionEvent = new DivisionEvent( Division );
            Division.Interact.FocusLast = Division.Interact.Focus;
            if(UserInterface.Focus == Division)
                Division.Interact.Focus = true;
            else
                Division.Interact.Focus = false;

            if(Division.Interact.Focus && !Division.Interact.FocusLast)
            {
                divisionEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Activation_ObtainingFocus" );
                ObtainingFocus?.Invoke( this, divisionEvent );
            }
            if(!Division.Interact.Focus && Division.Interact.FocusLast)
            {
                divisionEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Activation_LosesFocus" );
                LosesFocus?.Invoke( this, divisionEvent );
            }
            if(Division.Interact.InteractionLast)
            {
                if(!Division.Interact.Interaction)
                {
                    divisionEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Activation_ActivationEnd" );
                    ActivationEnd?.Invoke( this, divisionEvent );
                }
            }
            if(DraggingState && Division.Interact.IsDraggable)
            {
                divisionEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_Dragging" );
                Dragging?.Invoke( this, divisionEvent );
            }
            if(MouseResponder.MouseLeftUpFlag && Division.Interact.IsDraggable)
            {
                DraggingState = false;
                divisionEvent.Name = string.Concat( "Event_Division_", Division.Name, "_Mouse_DragEnd" );
                DragEnd?.Invoke( this, divisionEvent );
            }
        }
    }
}