using System.Globalization;
using Colin.Core.Modulars.UserInterfaces.Renderers;
using TextInputEventArgs = MonoGame.IMEHelper.TextInputEventArgs;

namespace Colin.Core.Modulars.UserInterfaces.Scripts
{
  /// <summary>
  /// 单行文本输入框行为: 点击拿焦点 → 输入法上屏 / 方向键移动光标 / 退格删除 / 回车收工, <see cref="Limit"/> 限长.
  /// <br>节点形状: 子级 <c>"Text"</c> 挂字体渲染器.</br>
  /// </summary>
  public sealed class InputTextBoxScript : DivScript
  {
    /// <summary>显示文本的子节点约定名字.</summary>
    public const string TextNodeName = "Text";

    public string Text;

    public string DisplayText;

    /// <summary>最大字符数.</summary>
    public int Limit = 16;

    public bool Editing;

    public Rectangle InputRect;

    public int CursorPosition;

    /// <summary>允许开头的空格.</summary>
    public bool AllowStartedSpace;

    /// <summary>允许空格输入.</summary>
    public bool AllowSpace;

    /// <summary>允许换行.</summary>
    public bool AllowLineFeed;

    public event EventHandler<TextInputEventArgs> TextInput;

    private bool _editingLast;
    private DivFontRenderer _font;

    protected override void OnBind()
    {
      Div owner = Owner;
      if (owner is null)
        return;
      owner.Interact.IsSelectable = true;
      Text = string.Empty;
      Div text = owner.GetChild(TextNodeName);
      if (text is not null)
      {
        _font = text.Renderer as DivFontRenderer;
        if (_font is null)
          _font = text.BindRenderer<DivFontRenderer>();
        text.LayoutScripted = true;
      }
      OnClick(owner, (node, args) => node.Module.Focus = node);
      if (owner.Module?.Scene?.Events is { } sceneEvents)
      {
        sceneEvents.TextInput += IMEHandler_TextInput;
        Unhook(() => sceneEvents.TextInput -= IMEHandler_TextInput);
      }
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
        //回车=收工: 交出焦点 (不再清空已输入内容).
        if (AllowLineFeed)
        {
          Text += "\n";
          CursorPosition++;
        }
        else if (Owner?.Module is not null)
          Owner.Module.Focus = null;
      }
      else
      {
        if (e.Key == Keys.Space && CursorPosition <= 0 && AllowStartedSpace is false)
          return;
        string result = e.Character.ToString();
        if (Text.Length + result.Length > Limit)
          return;
        if (Input.LegalInput(result))
        {
          Text = Text.Insert(CursorPosition, Convert.ToString(e.Character, CultureInfo.InvariantCulture));
          CursorPosition += result.Length;
        }
      }
    }

    protected override void OnUpdate(GameTime time)
    {
      Div owner = Owner;
      if (owner is null)
        return;
      Editing = owner.Module is not null && owner.Module.Focus == owner;
      //IME 组合窗口跟焦点走: 拿到焦点才打开输入法, 丢了就收掉.
      if (Editing && _editingLast is false)
      {
        InputRect = owner.Layout.RenderTargetBounds;
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

      //光标指示符仅在编辑态拼入, 避免未编辑时每帧字符串分配.
      if (_font is not null)
      {
        if (Editing)
          _font.Text = DisplayText = Text.Insert(CursorPosition, "|");
        else
          _font.Text = Text;
      }
    }

    public override void ExportValues(List<UIDesignValue> values)
    {
      values.Add(UIDesignValue.Field(nameof(Limit), Limit));
      values.Add(UIDesignValue.Field(nameof(AllowSpace), AllowSpace));
      values.Add(UIDesignValue.Field(nameof(AllowStartedSpace), AllowStartedSpace));
      values.Add(UIDesignValue.Field(nameof(AllowLineFeed), AllowLineFeed));
      base.ExportValues(values);
    }
  }
}
