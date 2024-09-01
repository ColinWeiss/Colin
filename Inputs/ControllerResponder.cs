using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Inputs
{
  /// <summary>
  /// [!] 已继承 <see cref="ISingleton"/>.
  /// </summary>
  public class ControllerResponder : GameComponent, ISingleton
  {
    public ControllerResponder() : base(CoreInfo.Core) { }

    public static GamePadState state;

    public static GamePadState stateLast;

    public static Vector2 cursorPosition;

    #region Buttons
    public static bool Button_A_ClickBefore => state.Buttons.A == ButtonState.Pressed && stateLast.Buttons.A == ButtonState.Released;
    public static bool Button_A_ClickAfter => state.Buttons.A == ButtonState.Released && stateLast.Buttons.A == ButtonState.Pressed;
    public static bool Button_A_Pressed => state.Buttons.A == ButtonState.Pressed && stateLast.Buttons.A == ButtonState.Pressed;
    public static bool Button_A_Released => state.Buttons.A == ButtonState.Released && stateLast.Buttons.A == ButtonState.Released;
    public static bool Button_B_ClickBefore => state.Buttons.B == ButtonState.Pressed && stateLast.Buttons.B == ButtonState.Released;
    public static bool Button_B_ClickAfter => state.Buttons.B == ButtonState.Released && stateLast.Buttons.B == ButtonState.Pressed;
    public static bool Button_B_Pressed => state.Buttons.B == ButtonState.Pressed && stateLast.Buttons.B == ButtonState.Pressed;
    public static bool Button_B_Released => state.Buttons.B == ButtonState.Released && stateLast.Buttons.B == ButtonState.Released;
    public static bool Button_X_ClickBefore => state.Buttons.X == ButtonState.Pressed && stateLast.Buttons.X == ButtonState.Released;
    public static bool Button_X_ClickAfter => state.Buttons.X == ButtonState.Released && stateLast.Buttons.X == ButtonState.Pressed;
    public static bool Button_X_Pressed => state.Buttons.X == ButtonState.Pressed && stateLast.Buttons.X == ButtonState.Pressed;
    public static bool Button_X_Released => state.Buttons.X == ButtonState.Released && stateLast.Buttons.X == ButtonState.Released;
    public static bool Button_Y_ClickBefore => state.Buttons.Y == ButtonState.Pressed && stateLast.Buttons.Y == ButtonState.Released;
    public static bool Button_Y_ClickAfter => state.Buttons.Y == ButtonState.Released && stateLast.Buttons.Y == ButtonState.Pressed;
    public static bool Button_Y_Pressed => state.Buttons.Y == ButtonState.Pressed && stateLast.Buttons.Y == ButtonState.Pressed;
    public static bool Button_Y_Released => state.Buttons.Y == ButtonState.Released && stateLast.Buttons.Y == ButtonState.Released;
    #endregion

    #region DPad
    public static bool DPad_Up_ClickBefore => state.DPad.Up == ButtonState.Pressed && stateLast.DPad.Up == ButtonState.Released;
    public static bool DPad_Up_ClickAfter => state.DPad.Up == ButtonState.Pressed && stateLast.DPad.Up == ButtonState.Released;
    public static bool DPad_Up_Pressed => state.DPad.Up == ButtonState.Pressed && stateLast.DPad.Up == ButtonState.Released;
    public static bool DPad_Up_Released => state.DPad.Up == ButtonState.Pressed && stateLast.DPad.Up == ButtonState.Released;
    public static bool DPad_Left_ClickBefore => state.DPad.Left == ButtonState.Pressed && stateLast.DPad.Left == ButtonState.Released;
    public static bool DPad_Left_ClickAfter => state.DPad.Left == ButtonState.Pressed && stateLast.DPad.Left == ButtonState.Released;
    public static bool DPad_Left_Pressed => state.DPad.Left == ButtonState.Pressed && stateLast.DPad.Left == ButtonState.Released;
    public static bool DPad_Left_Released => state.DPad.Left == ButtonState.Pressed && stateLast.DPad.Left == ButtonState.Released;
    public static bool DPad_Right_ClickBefore => state.DPad.Right == ButtonState.Pressed && stateLast.DPad.Right == ButtonState.Released;
    public static bool DPad_Right_ClickAfter => state.DPad.Right == ButtonState.Pressed && stateLast.DPad.Right == ButtonState.Released;
    public static bool DPad_Right_Pressed => state.DPad.Right == ButtonState.Pressed && stateLast.DPad.Right == ButtonState.Released;
    public static bool DPad_Right_Released => state.DPad.Right == ButtonState.Pressed && stateLast.DPad.Right == ButtonState.Released;
    public static bool DPad_Down_ClickBefore => state.DPad.Down == ButtonState.Pressed && stateLast.DPad.Down == ButtonState.Released;
    public static bool DPad_Down_ClickAfter => state.DPad.Down == ButtonState.Pressed && stateLast.DPad.Down == ButtonState.Released;
    public static bool DPad_Down_Pressed => state.DPad.Down == ButtonState.Pressed && stateLast.DPad.Down == ButtonState.Released;
    public static bool DPad_Down_Released => state.DPad.Down == ButtonState.Pressed && stateLast.DPad.Down == ButtonState.Released;
    #endregion

    public override void Update(GameTime gameTime)
    {
      stateLast = state;
      state = GamePad.GetState(PlayerIndex.One);
      if (state.IsConnected)
      {
        //       EngineInfo.Engine.IsMouseVisible = false;
        if (state.ThumbSticks.Right.X > 0)
          cursorPosition.X += 3;
        if (state.ThumbSticks.Right.X < 0)
          cursorPosition.X -= 3;
        if (state.ThumbSticks.Right.Y > 0)
          cursorPosition.Y -= 3;
        if (state.ThumbSticks.Right.Y < 0)
          cursorPosition.Y += 3;
        //          Console.WriteLine( EntityInfo.ThumbSticks.Right );
      }
      //      else
      //         EngineInfo.Engine.IsMouseVisible = true;

      base.Update(gameTime);
    }
  }

}