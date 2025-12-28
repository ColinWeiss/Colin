using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Inputs
{
  /// <summary>
  /// 鼠标响应器.
  /// <br>[!] 已继承 <see cref="ISingleton"/>.</br>
  /// </summary>
  public sealed class MouseResponder : GameComponent, ISingleton
  {
    public MouseResponder() : base(CoreInfo.Core) { }

    public static bool LeftClicking =>
      State.LeftButton == ButtonState.Pressed &&
      StateLast.LeftButton == ButtonState.Released && Core.Focus;

    public static bool LeftDown =>
      State.LeftButton == ButtonState.Pressed &&
      StateLast.LeftButton == ButtonState.Pressed && Core.Focus;

    public static bool LeftUp =>
      State.LeftButton == ButtonState.Released &&
      StateLast.LeftButton == ButtonState.Released && Core.Focus;

    public static bool LeftClicked =>
      State.LeftButton == ButtonState.Released &&
      StateLast.LeftButton == ButtonState.Pressed && Core.Focus;

    public static bool RightClicking =>
      State.RightButton == ButtonState.Pressed &&
      StateLast.RightButton == ButtonState.Released && Core.Focus;

    public static bool RightDown =>
      State.RightButton == ButtonState.Pressed &&
      StateLast.RightButton == ButtonState.Pressed && Core.Focus;

    public static bool RightUp =>
      State.RightButton == ButtonState.Released &&
      StateLast.RightButton == ButtonState.Released && Core.Focus;

    public static bool RightClicked =>
      State.RightButton == ButtonState.Released &&
      StateLast.RightButton == ButtonState.Pressed && Core.Focus;

    public static bool ScrollUp =>
      State.ScrollWheelValue < StateLast.ScrollWheelValue && Core.Focus;

    public static bool ScrollDown =>
      State.ScrollWheelValue > StateLast.ScrollWheelValue && Core.Focus;

    public static int ScrollValue
    {
      get
      {
        if (MouseResponder.ScrollDown)
          return 1;
        else if (MouseResponder.ScrollUp)
          return -1;
        else
          return 0;
      }
    }

    public static bool ScrollClickAfter =>
      State.MiddleButton == ButtonState.Released &&
      StateLast.MiddleButton == ButtonState.Pressed && Core.Focus;

    public static bool ScrollClickBefore =>
      State.MiddleButton == ButtonState.Pressed &&
      StateLast.MiddleButton == ButtonState.Released && Core.Focus;

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

    public override void Update(GameTime gameTime)
    {
      StateLast = State;
      State = Mouse.GetState();
      base.Update(gameTime);
    }

    public static void StaticUpdate()
    {
      StateLast = State;
      State = Mouse.GetState();
    }
  }
}