using Colin.Core.Events;
using Colin.Core.Modulars.UserInterfaces.Events;
using TextInputEventArgs = MonoGame.IMEHelper.TextInputEventArgs;

namespace Colin.Core.Common
{
  public class SceneEvents : ISceneModule
  {
    public Scene Scene { get; set; }
    public bool Enable { get; set; }

    public MouseEventNode Mouse;

    public KeysEventNode Keys;

    public event EventHandler ClientSizeChanged;

    public event EventHandler OrientationChanged;

    public event EventHandler<TextInputEventArgs> TextInput;

    public void DoInitialize()
    {
      Mouse = new MouseEventNode();
      Keys = new KeysEventNode();
    }
    public void Start()
    {
    }
    public void DoUpdate(GameTime time)
    {
      if (MouseResponder.LeftClicked)
        Mouse.LeftClicked?.TriggerCapture(GetMouseArgs<LeftClickedArgs>());
      if (MouseResponder.LeftClicking)
        Mouse.LeftClicking?.TriggerCapture(GetMouseArgs<LeftClickingArgs>());
      if (MouseResponder.LeftDown)
        Mouse.LeftDown?.TriggerCapture(GetMouseArgs<LeftDownArgs>());
      if (MouseResponder.LeftUp)
        Mouse.LeftUp?.TriggerCapture(GetMouseArgs<LeftUpArgs>());
      if (MouseResponder.RightClicked)
        Mouse.RightClicked?.TriggerCapture(GetMouseArgs<RightClickedArgs>());
      if (MouseResponder.RightClicking)
        Mouse.RightClicking?.TriggerCapture(GetMouseArgs<RightClickingArgs>());
      if (MouseResponder.RightDown)
        Mouse.RightDown?.TriggerCapture(GetMouseArgs<RightDownArgs>());
      if (MouseResponder.RightUp)
        Mouse.RightUp?.TriggerCapture(GetMouseArgs<RightUpArgs>());
      if (MouseResponder.ScrollDown)
        Mouse.ScrollDown?.TriggerCapture(GetMouseArgs<ScrollDownArgs>());
      if (MouseResponder.ScrollUp)
        Mouse.ScrollUp?.TriggerCapture(GetMouseArgs<ScrollUpArgs>());

      Keys[] lasts = KeyboardResponder.StateLast.GetPressedKeys();
      Keys[] current = KeyboardResponder.State.GetPressedKeys();
      if (CoreInfo.IMEHandler.Enabled is false)
      {
        foreach (var key in lasts)
          if (!current.Contains(key))
            Keys.KeysClicked?.TriggerCapture(GetKeysArgs<KeysClickedArgs>(key));
        foreach (var key in current)
        {
          if (!lasts.Contains(key))
            Keys.KeysClicking?.TriggerCapture(GetKeysArgs<KeysClickingArgs>(key));
          if (lasts.Contains(key))
            Keys.KeysDown?.TriggerCapture(GetKeysArgs<KeysDownArgs>(key));
        }
      }
    }
    public void Dispose()
    {
    }

    public void InvokeSizeChange(object sender, EventArgs e)
    {
      ClientSizeChanged?.Invoke(this, e);
      OrientationChanged?.Invoke(this, e);
    }
    public void OnTextInput(object sender, TextInputEventArgs e)
    {
      TextInput?.Invoke(sender, e);
    }

    private T GetMouseArgs<T>() where T : MouseArgs, new()
    {
      T result = new T();
      result.Sender = "User";
      result.IsCapture = false;
      result.MousePos = MouseResponder.Position;
      result.Wheel = MouseResponder.ScrollValue;
      return result;
    }
    private T GetKeysArgs<T>(Keys keys) where T : KeysArgs, new()
    {
      T result = new T();
      result.Sender = "User";
      result.IsCapture = false;
      result.StopBubbling = false;
      result.Keys = keys;
      return result;
    }
  }
}