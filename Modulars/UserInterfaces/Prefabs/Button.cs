using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
  /// <summary>
  /// 按钮.
  /// <br>内置标准交互状态机: 悬停提亮 / 按下压暗并下移内容 / 禁用淡出, 反馈幅度可调.</br>
  /// <br>默认外观取自 <see cref="UITheme"/>; 状态机以 <see cref="DivInit"/> 完成时刻的颜色为基准做乘算, 不吃掉子类自定义色 —— 运行期改色后调 <see cref="RecaptureColors"/>.</br>
  /// </summary>
  public class Button : Div
  {
    public Label Label;

    /// <summary>
    /// 指示按钮是否可用; 不可用时不再触发交互事件, 外观整体淡出.
    /// </summary>
    public bool Enabled = true;

    /// <summary>悬停时的背景亮度乘算系数.</summary>
    public float HoverBrightness = 1.12f;

    /// <summary>按下时的背景亮度乘算系数.</summary>
    public float PressBrightness = 0.82f;

    /// <summary>禁用时整体 (背景与文字) 的透明度乘算系数.</summary>
    public float DisabledAlpha = 0.45f;

    /// <summary>按下时文字的下移像素.</summary>
    public int PressOffsetY = 1;

    private Color _baseColor;
    private Color _baseTextColor;
    private float _tint = 1f;
    private float _fade = 1f;

    public Button(string name, string text = "") : base(name)
    {
      Label = new Label("ButtonText");
      Label.FontRenderer = Label.BindRenderer<DivFontRenderer>();
      Label.FontRenderer.Font = Assets.Font("Fonts/GlowSansMedium.ttf").GetFont((int)UITheme.ButtonFontSize);
      Label.Design.Color = UITheme.ButtonText;
      Label.SetText(text);
      Design.Color = UITheme.ButtonNormal;
    }

    public override void DivInit()
    {
      Interact.IsSelectable = true;
      if (Renderer is null)
        BindRenderer<DivPixelRenderer>();
      Register(Label);
      Label.LayoutScripted = true;
      base.DivInit();
      RecaptureColors();
    }

    /// <summary>
    /// 以当前颜色为基准重新捕获状态机的乘算底色.
    /// </summary>
    public void RecaptureColors()
    {
      _baseColor = Design.Color;
      _baseTextColor = Label.Design.Color;
    }

    public override void OnUpdate(GameTime time)
    {
      Interact.IsInteractive = Enabled;
      bool pressed = Enabled && Interact.Interaction && MouseResponder.LeftDown;
      float tintTarget = pressed ? PressBrightness : Interact.Interaction ? HoverBrightness : 1f;
      float fadeTarget = Enabled ? 1f : DisabledAlpha;
      float blend = MathF.Min(1f, Time.UnscaledDeltaTime * 18f);
      _tint += (tintTarget - _tint) * blend;
      _fade += (fadeTarget - _fade) * blend;
      Design.Color = new Color(
        (int)Math.Clamp(_baseColor.R * _tint, 0, 255),
        (int)Math.Clamp(_baseColor.G * _tint, 0, 255),
        (int)Math.Clamp(_baseColor.B * _tint, 0, 255),
        (int)Math.Clamp(_baseColor.A * _fade, 0, 255));
      Label.Design.Color = new Color(
        _baseTextColor.R,
        _baseTextColor.G,
        _baseTextColor.B,
        (int)Math.Clamp(_baseTextColor.A * _fade, 0, 255));
      Label.Layout.Left = Layout.Width / 2 - Label.Layout.Width / 2;
      Label.Layout.Top = Layout.Height / 2 - Label.FontRenderer.Font.LineHeight / 2 - 2 + (pressed ? PressOffsetY : 0);
      base.OnUpdate(time);
    }
  }
}