using TextInputEventArgs = MonoGame.IMEHelper.TextInputEventArgs;

namespace Colin.Core.Events
{
  public class TextInputEventResponder : EventResponder
  {
    public TextInputEventResponder(string name) : base(name) { }
    public EventHandler<TextInputEventArgs> TextInput;
  }
}
