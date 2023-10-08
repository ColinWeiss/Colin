using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Inputs
{
    /// <summary>
    /// 鼠标响应器.
    /// <br>[!] 已继承 <see cref="ISingleton"/>.</br>
    /// </summary>
    public sealed class MouseResponder : GameComponent, ISingleton
    {
        public MouseResponder() : base( EngineInfo.Engine ) { }

        public static bool LeftClickBefore =>
            State.LeftButton == ButtonState.Pressed &&
            StateLast.LeftButton == ButtonState.Released;

        public static bool LeftDown =>
            State.LeftButton == ButtonState.Pressed &&
            StateLast.LeftButton == ButtonState.Pressed;

        public static bool LeftUp =>
            State.LeftButton == ButtonState.Released &&
            StateLast.LeftButton == ButtonState.Released;

        public static bool LeftClickAfter =>
           State.LeftButton == ButtonState.Released &&
            StateLast.LeftButton == ButtonState.Pressed;

        public static bool RightClickBefore =>
            State.RightButton == ButtonState.Pressed &&
            StateLast.RightButton == ButtonState.Released;

        public static bool RightDown =>
            State.RightButton == ButtonState.Pressed &&
            StateLast.RightButton == ButtonState.Pressed;

        public static bool RightUp =>
             State.RightButton == ButtonState.Released &&
             StateLast.RightButton == ButtonState.Released;

        public static bool RightClickAfter =>
            State.RightButton == ButtonState.Released &&
            StateLast.RightButton == ButtonState.Pressed;

        public static bool ScrollUp =>
            State.ScrollWheelValue < StateLast.ScrollWheelValue;

        public static bool ScrollDown =>
            State.ScrollWheelValue > StateLast.ScrollWheelValue;

        public bool Enable { get; set; }

        /// <summary>
        /// 当前鼠标状态.
        /// </summary>
        public static MouseState State = new MouseState();

        /// <summary>
        /// 上一帧鼠标状态.
        /// </summary>
        public static MouseState StateLast = new MouseState();

        /// <summary>
        /// 鼠标位置.
        /// </summary>
        public static Vector2 Position => State.Position.ToVector2();

        public override void Update( GameTime gameTime )
        {
            StateLast = State;
            State = Mouse.GetState();
            base.Update( gameTime );
        }
    }
}