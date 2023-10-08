using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Inputs
{
    /// <summary>
    /// 键盘响应器.
    /// </summary>
    public sealed class KeyboardResponder : GameComponent, ISingleton
    {
        public KeyboardResponder() : base( EngineInfo.Engine ) { }

        public static KeyboardState State = new KeyboardState();

        public static KeyboardState StateLast = new KeyboardState();

        public override void Update( GameTime gameTime )
        {
            StateLast = State;
            State = Keyboard.GetState();
            base.Update( gameTime );
        }

        public static bool IsKeyDown( Keys keys ) => State.IsKeyDown( keys );

        public static bool IsKeyUp( Keys keys ) => State.IsKeyUp( keys );

        public static bool IsKeyClickBefore( Keys keys ) => StateLast.IsKeyUp( keys ) && State.IsKeyDown( keys );

        public static bool IsKeyClickAfter( Keys keys ) => StateLast.IsKeyDown( keys ) && State.IsKeyUp( keys );

    }
}