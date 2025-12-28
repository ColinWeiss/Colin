using Microsoft.Xna.Framework.Input;

namespace Colin.Core.Inputs
{
  /// <summary>
  /// 键盘响应器.
  /// </summary>
  public sealed class KeyboardResponder : GameComponent, ISingleton
  {
    public KeyboardResponder() : base(CoreInfo.Core) { }

    public static KeyboardState State = new KeyboardState();

    public static KeyboardState StateLast = new KeyboardState();

    public override void Update(GameTime gameTime)
    {
      StateLast = State;
      State = Keyboard.GetState();
      base.Update(gameTime);
    }

    public static void StaticUpdate()
    {
      StateLast = State;
      State = Keyboard.GetState();
    }

    public static bool Down(Keys keys) => State.IsKeyDown(keys) && Core.Focus;

    public static bool Up(Keys keys) => State.IsKeyUp(keys) && Core.Focus;

    public static bool Clicking(Keys keys) => StateLast.IsKeyUp(keys)&& State.IsKeyDown(keys) && Core.Focus;

    public static bool Clicked(Keys keys) => StateLast.IsKeyDown(keys) && State.IsKeyUp(keys) && Core.Focus;
  }
}