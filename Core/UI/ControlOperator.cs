using Microsoft.Xna.Framework.Graphics;

namespace Colin.Core.UI
{
    /// <summary>
    /// 表示一个控件执行器
    /// </summary>
    public class ControlOperator : EngineElement
    {
        public new float Width => HardwareInfo.GameViewWidth;

        public new float Height => HardwareInfo.GameViewHeight;

        /// <summary>
        /// 该执行器内的元素列表
        /// </summary>
        public List<Control> Controls = new List<Control>( );

        /// <summary>
        /// 获得上一帧鼠标所悬浮的、在界面最顶层的UI元素
        /// </summary>
        /// <returns></returns>
        public Control? OldAtControl { get; private set; }

        /// <summary>
        /// 获得目前可交互的、在该运行器内最顶层的控件
        /// </summary>
        /// <returns></returns>
        public virtual Control ControlSeekAt( )
        {
            Control? target = null;
            for ( int element = Controls.Count - 1; element >= 0; element-- )
            {
                if ( Controls[ element ].SeekAt( ) != null )
                {
                    target = Controls[ element ].SeekAt( );
                    break;
                }
            }
            return target;
        }

        public ControlPointer? Pointer { get; private set; }

        /// <summary>
        /// 将一个具有指定引用的元素注册至该状态管理器
        /// </summary>
        /// <param name="control"></param>
        public void Register( Control control )
        {
            control.Operator = this;
            Controls.Add( control );
        }

        protected override void Initialization( )
        {
            Pointer = new ControlPointer( this );
            for ( int Count = 0; Count < Controls.Count; Count++ )
                Controls[ Count ].Initialize( );
        }

        private Control? _seekControl;
        protected override void Update( )
        {
            base.Update( );
            OldAtControl = _seekControl;
            _seekControl = ControlSeekAt( );
            for ( int Count = 0; Count < Controls.Count; Count++ )
                Controls[ Count ].Update( HardwareInfo.GameTimeCache );
            _seekControl = ControlSeekAt( );
            if ( _seekControl != null && _seekControl.Enable )
            {
                if ( _seekControl.Interactive && Input.MouseLeftClick )
                    _seekControl.MouseLeftClickEvent( );
                else if ( _seekControl.Interactive && Input.MouseLeftPressed )
                    _seekControl.MouseLeftPressedEvent( );
                else if ( _seekControl.Interactive && Input.MouseLeftUp )
                    _seekControl.MouseLeftUpEvent( );
                if ( _seekControl.Interactive && Input.MouseRightUp )
                    _seekControl.MouseRightUpEvent( );
                else if ( _seekControl.Interactive && Input.MouseRightPressed )
                    _seekControl.MouseRightPressedEvent( );
                else if ( _seekControl.Interactive && Input.MouseRightUp )
                    _seekControl.MouseRightUpEvent( );
                if ( _seekControl.Interactive )
                    _seekControl.MouseHoverEvent( );
                if ( _seekControl.Interactive && OldAtControl != _seekControl )
                    _seekControl.MouseIntoEvent( );
            }
            if ( _seekControl != OldAtControl && OldAtControl != null && !OldAtControl.Interactive )
                OldAtControl.MouseLeaveEvent( );
        }

        protected override void Draw( SpriteBatch spriteBatch )
        {
            base.Draw( spriteBatch );
            for ( int Count = 0; Count < Controls.Count; Count++ )
                Controls[ Count ].Draw( HardwareInfo.GameTimeCache );
        }
        /// <summary>
        /// 获取该执行器内的所有控件及控件的子控件.
        /// </summary>
        /// <returns></returns>
        public List<Control> GetControls( )
        {
            List<Control> result = new List<Control>( );
            for ( int count = 0; count < Controls.Count; count++ )
            {
                for ( int sub = 0; sub < Controls[ count ].GetControls( ).Count; sub++ )
                {
                    result.Add( Controls[ count ].GetControls( )[ sub ] );
                }
            }
            return result;
        }

        /// <summary>
        /// 获取该执行器内的所有已启用控件及已启用控件的子控件.
        /// </summary>
        /// <returns></returns>
        public List<Control> GetActiveControls( )
        {
            List<Control> result = new List<Control>( );
            for ( int count = 0; count < Controls.Count; count++ )
            {
                if ( Controls[ count ].Enable && Controls[ count ].Rectangle.Intersects( HardwareInfo.GameViewSize ) )
                {
                    for ( int sub = 0; sub < Controls[ count ].GetControls( ).Count; sub++ )
                    {
                        if ( Controls[ count ].GetControls( )[ sub ].Enable )
                            result.Add( Controls[ count ].GetControls( )[ sub ] );
                    }
                }
            }
            return result;
        }
    }
}