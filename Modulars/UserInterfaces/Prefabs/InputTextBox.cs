using System.Globalization;
using TextInputEventArgs = MonoGame.IMEHelper.TextInputEventArgs;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
  /// <summary>
  /// 单行文本输入框.
  /// <br>点击文本框拿到焦点后才能输入;焦点进出会跟着开关输入法组合窗口,中文输入法能正常上屏。</br>
  /// <br>方向键移动光标,退格删除,回车=收工(交出焦点);<see cref="Limit"/> 限制最大字符数。</br>
  /// </summary>
  public class InputTextBox : Div
  {
    public InputTextBox(string name, int limit = 16) : base(name)
    {
      Limit = limit;
      Label = new Label("Text");
    }

    public Label Label;

    public string Text;

    public string DisplayText;

    /// <summary>
    /// 获取该文本框限制的字符数量.
    /// </summary>
    public readonly int Limit;

    public bool Editing = false;

    private bool _editingLast = false;

    public Rectangle InputRect;

    public int CursorPosition;

    /// <summary>
    /// 允许开头的空格.
    /// </summary>
    public bool AllowStartedSpace = false;

    /// <summary>
    /// 允许空格输入.
    /// </summary>
    public bool AllowSpace = false;

    /// <summary>
    /// 允许换行.
    /// </summary>
    public bool AllowLineFeed = false;

    public event EventHandler<TextInputEventArgs> TextInput;

    public override void DivInit()
    {
      Text = "";
      Register(Label);
      //点击文本框就明确把焦点拿到手:不指望外层的点击分发顺路带上,自己管自己最稳。
      Events.LeftClicked += (sender, args) =>
      {
        args.IsCapture = true;
        Module.Focus = this;
      };
      Module.Scene.Events.TextInput += IMEHandler_TextInput;
      base.DivInit();
    }

    private void IMEHandler_TextInput(object sender, TextInputEventArgs e)
    {
      if (Editing is false)
        return;
      TextInput?.Invoke(sender, e);
      if (e.Key == Keys.Back && CursorPosition > 0)
      {
        Text = Text.Remove(CursorPosition - 1, 1);
        CursorPosition--;
      }
      else if (e.Key == Keys.Enter)
      {
        //回车=收工:交出焦点。以前这里直接把 Text 清空,输入半天的东西一敲回车就没了。
        if (AllowLineFeed)
        {
          Text += "\n";
          CursorPosition++;
        }
        else
          Module.Focus = null;
      }
      else
      {
        if (e.Key == Keys.Space && CursorPosition <= 0 && !AllowStartedSpace)
          return;
        string result = e.Character.ToString();
        //字数上限在这里兜住,以前 Limit 只是摆设。
        if (Text.Length + result.Length > Limit)
          return;
        if (Input.LegalInput(result))
        {
          Text = Text.Insert(CursorPosition, Convert.ToString(e.Character, CultureInfo.InvariantCulture));
          CursorPosition += result.Length;
        }
      }
    }

    public override void OnUpdate(GameTime time)
    {
      Editing = Module.Focus == this;
      //IME 组合窗口跟焦点走:拿到焦点才打开输入法,丢了就收掉。
      //这两句以前被注释成死代码,输入法组合窗口从来没开过,中文一个字都上不了屏。
      if (Editing && _editingLast is false)
      {
        InputRect = Layout.RenderTargetBounds;
        InputRect.X += 16;
        InputRect.Y += 16;
        CoreInfo.IMEHandler.StartTextComposition();
        CoreInfo.IMEHandler.SetTextInputRect(ref InputRect);
      }
      else if (Editing is false && _editingLast)
      {
        CoreInfo.IMEHandler.StopTextComposition();
      }
      _editingLast = Editing;

      if (Text != string.Empty && Editing)
      {
        if (KeyboardResponder.Clicked(Keys.Left))
          CursorPosition = Math.Clamp(CursorPosition - 1, 0, Text.Length);
        if (KeyboardResponder.Clicked(Keys.Right))
          CursorPosition = Math.Clamp(CursorPosition + 1, 0, Text.Length);

        if (KeyboardResponder.Clicked(Keys.PageDown))
          CursorPosition = Text.Length;
        if (KeyboardResponder.Clicked(Keys.PageUp))
          CursorPosition = 0;
      }
      else
        CursorPosition = Text.Length;

      DisplayText = Text.Insert(CursorPosition, "|");
      if (Editing)
        Label.SetText(DisplayText);
      else
        Label.SetText(Text);

      base.OnUpdate(time);
    }
  }
}
