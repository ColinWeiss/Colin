using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Inputs
{
    /// <summary>
    /// 键盘响应器.
    /// </summary>
    public sealed class KeyboardResponder : GameComponent, ISingleton
    {
        public KeyboardResponder( ) : base( EngineInfo.Engine ) { }

        /// <summary>
        /// 事件: 发生于键盘上任何键单击按下时.
        /// </summary>
        public static EventHandler<KeyboardEventArgs> KeyClickBefore;

        /// <summary>
        /// 事件: 发生于键盘上任何键单击松开时.
        /// </summary>
        public static EventHandler<KeyboardEventArgs> KeyClickAfter;

        public static KeyboardState State = new KeyboardState( );

        public static KeyboardState StateLast = new KeyboardState( );

        /// <summary>
        /// 当前键盘事件.
        /// </summary>
        public KeyboardEventArgs KeyboardEvent;

        public override void Update( GameTime gameTime )
        {
            StateLast = State;
            State = Keyboard.GetState( );
            KeyboardEvent = new KeyboardEventArgs( State );
            Keys[ ] pressedKeysLast = StateLast.GetPressedKeys( );
            foreach( Keys key in State.GetPressedKeys( ) )
            {
                if( !pressedKeysLast.Contains( key ) )
                {
                    KeyboardEvent.name = "Event.Keyboard.KeyClickBefore";
                    KeyboardEvent.Keys = key;
                    KeyClickBefore?.Invoke( this, KeyboardEvent );
                }
            }
            foreach( Keys keyLast in StateLast.GetPressedKeys( ) )
            {
                if( State.IsKeyUp( keyLast ) && StateLast.IsKeyDown( keyLast ) )
                {
                    KeyboardEvent.name = "Event.Keyboard.KeyClickAfter";
                    KeyboardEvent.Keys = keyLast;
                    KeyClickAfter?.Invoke( this, KeyboardEvent );
                }
            }
            base.Update( gameTime );
        }

        public static bool IsKeyDown( Keys keys ) => State.IsKeyDown( keys );

        public static bool IsKeyUp( Keys keys ) => State.IsKeyUp( keys );

        public static bool IsKeyClickBefore( Keys keys ) => StateLast.IsKeyUp( keys ) && State.IsKeyDown( keys );

        public static bool IsKeyClickAfter( Keys keys ) => StateLast.IsKeyDown( keys ) && State.IsKeyUp( keys );

    }
}