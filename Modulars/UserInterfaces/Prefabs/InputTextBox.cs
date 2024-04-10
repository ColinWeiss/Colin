﻿using System.Globalization;
using TextInputEventArgs = MonoGame.IMEHelper.TextInputEventArgs;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
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
      Label.Interact.IsInteractive = false;
      Register(Label);

      Events.GetFocus += () =>
      {
        EngineInfo.IMEHandler.StartTextComposition();
        EngineInfo.IMEHandler.SetTextInputRect(ref InputRect);
      };
      Events.LoseFocus += () =>
      {
        EngineInfo.IMEHandler.StopTextComposition();
        Label.SetText(Text);
      };
      UserInterface.Scene.Events.TextInput += IMEHandler_TextInput;
      base.DivInit();
    }

    private void IMEHandler_TextInput(object sender, MonoGame.IMEHelper.TextInputEventArgs e)
    {
      if (Editing)
      {
        TextInput?.Invoke(sender, e);
        if (e.Key == Keys.Back && CursorPosition > 0)
        {
          Text = Text.Remove(CursorPosition - 1, 1);
          CursorPosition--;
        }
        else if (e.Key == Keys.Enter)
        {
          if (AllowLineFeed)
          {
            Text += "\n";
            CursorPosition++;
          }
        }
        else
        {
          if (e.Key == Keys.Space && CursorPosition <= 0 && !AllowStartedSpace)
            return;
          string result = e.Character.ToString();
          if (Input.LegalInput(result))
          {
            Text = Text.Insert(CursorPosition, Convert.ToString(e.Character, CultureInfo.InvariantCulture));
            CursorPosition += e.Character.ToString().Length;
          }
        }
      }
    }

    public override void OnUpdate(GameTime time)
    {
      Editing = UserInterface.Focus == this;
      InputRect = Layout.RenderTargetBounds;
      InputRect.Y += 16;
      InputRect.X += 16;
      if (Text != string.Empty && Editing)
      {
        if (KeyboardResponder.IsKeyClickBefore(Keys.Left))
          CursorPosition = Math.Clamp(CursorPosition - 1, 0, Text.Length);
        if (KeyboardResponder.IsKeyClickBefore(Keys.Right))
          CursorPosition = Math.Clamp(CursorPosition + 1, 0, Text.Length);
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