using Colin.Core.Events;
using Microsoft.Xna.Framework.Input;
namespace Colin.Core.Modulars.UserInterfaces
{
    /// <summary>
    /// 划分元素事件响应器.
    /// </summary>
    public class DivisionEventResponder
    {
        public Division Div;

        public DivisionEventResponder( Division division ) => Div = division;

        public MouseEventResponder Mouse = new MouseEventResponder( "MouseEvents" );

        public KeysEventResponder Keys = new KeysEventResponder( "KeysEvents" );

        public event Action HoverStart;
        public event Action Hover;
        public event Action HoverOver;

        public event Action LeftClickBefore;
        public event Action LeftDown;
        public event Action LeftClickAfter;
        public event Action LeftUp;

        public event Action RightClickBefore;
        public event Action RightDown;
        public event Action RightClickAfter;
        public event Action RightUp;

        public event Action DragStart;
        public event Action Dragging;
        public event Action DragOver;

        public event Action GetFocus;
        public event Action LoseFocus;

        public EventHandler<KeysEventArgs> KeyClickBefore;

        public EventHandler<KeysEventArgs> KeyDown;

        public EventHandler<KeysEventArgs> KeyClickAfter;

        /// <summary>
        /// 指示拖拽状态.
        /// </summary>
        public bool DraggingState = false;

        public void DoInitialize()
        {
            Mouse.Hover += ( s, e ) =>
                {
                    if(Div.Interact.Interaction && !Div.Interact.InteractionLast)
                        HoverStart?.Invoke();
                    Invoke( e, Hover );
                    if(!Div.Interact.Interaction && Div.Interact.InteractionLast)
                        HoverOver?.Invoke();
                };
            Mouse.LeftClickBefore += ( s, e ) =>
            {
                Invoke( e, LeftClickBefore );
                Invoke( e, () =>
                {
                    Div.Interface.LastFocus = Div.Interface.Focus;
                    Div.Interface.Focus = Div;
                    if(!Div.Interact.IsDraggable)
                        return;
                    DragStart?.Invoke();
                    DraggingState = true;
                    if(Div.Parent != null)
                    {
                        Point mouseForParentLocation = MouseResponder.State.Position - Div.Parent.Layout.Location;
                        _cachePos = mouseForParentLocation - Div.Layout.Location;
                    }
                    else
                    {
                        _cachePos = MouseResponder.State.Position - Div.Layout.Location;
                    }
                } );
            };
            Mouse.LeftDown += ( s, e ) => Invoke( e, LeftDown );
            Mouse.LeftClickAfter += ( s, e ) =>
            {
                DragOver?.Invoke();
                Invoke( e, LeftClickAfter );
                if(!Div.Interact.IsDraggable)
                    return;
                DraggingState = false;
                _cachePos = new Point( -1, -1 );
            };
            Mouse.LeftUp += ( s, e ) => Invoke( e, LeftUp );
            Keys.ClickBefore += KeyClickBefore;
            Keys.Down += KeyDown;
            Keys.ClickAfter += KeyClickAfter;
        }
        private void Invoke( MouseEventArgs e, Action action )
        {
            if(Div.IsVisible && Div.ContainsPoint( MouseResponder.State.Position ) && Div.Interact.IsInteractive)
            {
                e.Captured = true;
                action?.Invoke();
            }
        }
        private Point _cachePos = new Point( -1, -1 );
        public void DoUpdate()
        {
            Div.Interact.InteractionLast = Div.Interact.Interaction;
            if(Div.ContainsPoint( MouseResponder.State.Position ) && Div.Interact.IsInteractive)
                Div.Interact.Interaction = true;
            else
                Div.Interact.Interaction = false;
            if(DraggingState && Div.Interact.IsDraggable)
            {
                if(!Div.Interact.IsDraggable)
                    return;
                DraggingState = true;
                if(Div.Parent != null)
                {
                    Point _resultLocation = MouseResponder.State.Position - Div.Parent.Layout.Location - _cachePos;
                    Div.Layout.Left = _resultLocation.X;
                    Div.Layout.Top = _resultLocation.Y;
                }
                else
                {
                    Point _resultLocation = MouseResponder.State.Position - _cachePos;
                    Div.Layout.Left = _resultLocation.X;
                    Div.Layout.Top = _resultLocation.Y;
                }
                if(Div.Interact.IsDraggable && Div.Interact.DragLimit != Rectangle.Empty)
                {
                    Div.Layout.Left = Math.Clamp( Div.Layout.Left, 0, Div.Interact.DragLimit.Width - Div.Layout.Width );
                    Div.Layout.Top = Math.Clamp( Div.Layout.Top, 0, Div.Interact.DragLimit.Height - Div.Layout.Height );
                }
                Dragging?.Invoke();
            }
            if(Div.Interface.Focus == Div && Div.Interface.LastFocus != Div)
                GetFocus?.Invoke();
            if(Div.Interface.Focus != Div && Div.Interface.LastFocus == Div)
                LoseFocus?.Invoke();
        }
    }
}