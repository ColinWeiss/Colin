using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Scripts
{
  /// <summary>
  /// 按钮行为: 悬停提亮 / 按下压暗并下移文字 / 禁用淡出.
  /// <br>节点形状: 自身绑像素渲染器, 子级 <c>"Text"</c> 挂字体渲染器 (由 <see cref="DivFactory.Button"/> 或编辑器给出).</br>
  /// <br>[!] 状态机以绑定时刻的颜色为基准做乘算, 不吃掉作者设置的颜色; 运行期改色后调 <see cref="RecaptureColors"/>.</br>
  /// </summary>
  public sealed class ButtonScript : DivScript
  {
    /// <summary>文字子节点的约定名字.</summary>
    public const string TextNodeName = "Text";

    /// <summary>指示按钮是否可用; 不可用时不再触发交互事件, 外观整体淡出.</summary>
    public bool Enabled = true;

    /// <summary>悬停时的背景亮度乘算系数.</summary>
    public float HoverBrightness = 1.12f;

    /// <summary>按下时的背景亮度乘算系数.</summary>
    public float PressBrightness = 0.82f;

    /// <summary>禁用时整体 (背景与文字) 的透明度乘算系数.</summary>
    public float DisabledAlpha = 0.45f;

    /// <summary>按下时文字的下移像素.</summary>
    public int PressOffsetY = 1;

    private DivFontRenderer _font;
    private Color _baseColor;
    private Color _baseTextColor;
    private bool _captured;
    private float _tint = 1f;
    private float _fade = 1f;

    protected override void OnBind()
    {
      Div owner = Owner;
      if (owner is null)
        return;
      if (owner.Renderer is null)
        owner.BindRenderer<DivPixelRenderer>();
      owner.Interact.IsSelectable = true;
      Div text = owner.GetChild(TextNodeName);
      _font = text?.Renderer as DivFontRenderer;
      if (text is not null)
      {
        if (_font is null)
          _font = text.BindRenderer<DivFontRenderer>();
        text.LayoutScripted = true;
      }
      RecaptureColors();
    }

    /// <summary>以当前颜色为基准重新捕获状态机的乘算底色.</summary>
    public void RecaptureColors()
    {
      Div owner = Owner;
      if (owner is null)
        return;
      _baseColor = owner.Design.Color;
      Div text = owner.GetChild(TextNodeName);
      _baseTextColor = text is null ? Color.White : text.Design.Color;
      _captured = true;
    }

    public override Color? AuthorColor => _captured ? _baseColor : null;

    public override void ExportValues(List<UIDesignValue> values)
    {
      values.Add(UIDesignValue.Field(nameof(Enabled), Enabled));
      values.Add(UIDesignValue.Field(nameof(HoverBrightness), HoverBrightness));
      values.Add(UIDesignValue.Field(nameof(PressBrightness), PressBrightness));
      values.Add(UIDesignValue.Field(nameof(DisabledAlpha), DisabledAlpha));
      values.Add(UIDesignValue.Field(nameof(PressOffsetY), PressOffsetY));
      base.ExportValues(values);
    }

    protected override void OnUpdate(GameTime time)
    {
      Div owner = Owner;
      if (owner is null)
        return;
      owner.Interact.IsInteractive = Enabled;
      bool pressed = Enabled && owner.Interact.Interaction && MouseResponder.LeftDown;
      float tintTarget = pressed ? PressBrightness : owner.Interact.Interaction ? HoverBrightness : 1f;
      float fadeTarget = Enabled ? 1f : DisabledAlpha;
      float blend = MathF.Min(1f, Time.UnscaledDeltaTime * 18f);
      _tint += (tintTarget - _tint) * blend;
      _fade += (fadeTarget - _fade) * blend;
      owner.Design.Color = new Color(
        (int)Math.Clamp(_baseColor.R * _tint, 0, 255),
        (int)Math.Clamp(_baseColor.G * _tint, 0, 255),
        (int)Math.Clamp(_baseColor.B * _tint, 0, 255),
        (int)Math.Clamp(_baseColor.A * _fade, 0, 255));

      Div text = owner.GetChild(TextNodeName);
      if (text is null)
        return;
      text.Design.Color = new Color(
        _baseTextColor.R,
        _baseTextColor.G,
        _baseTextColor.B,
        (int)Math.Clamp(_baseTextColor.A * _fade, 0, 255));
      text.Layout.Left = owner.Layout.Width / 2 - text.Layout.Width / 2;
      float lineHeight = _font?.Font?.LineHeight ?? 0f;
      text.Layout.Top = owner.Layout.Height / 2 - lineHeight / 2 - 2 + (pressed ? PressOffsetY : 0);
    }
  }
}
