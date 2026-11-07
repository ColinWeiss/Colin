using Colin.Core.Modulars.UserInterfaces.Renderers;
using Colin.Core.Modulars.UserInterfaces.Scripts;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
  // ============================================================================
  //  过渡壳: 这些类只为"游戏侧还没迁到 Div + 脚本"的调用点而存在.
  //  新代码不要用它们 —— 节点一律 new Div(...) + DivFactory/扩展方法 + BindScript.
  //  每迁完一个游戏文件, 就删掉这里对应的壳.
  // ============================================================================

  /// <summary>[过渡] 文本节点: 改用 <c>parent.Label(name, text)</c>.</summary>
  [Obsolete("改用 Div + BindRenderer<DivFontRenderer>() / DivFactory.Label(name, text)")]
  public class Label : Div
  {
    public Label(string name) : base(name) { }

    public DivFontRenderer FontRenderer;

    public DivFontRenderer EnsureFontRenderer()
    {
      if (Renderer is DivFontRenderer existing)
      {
        FontRenderer = existing;
        return existing;
      }
      FontRenderer = BindRenderer<DivFontRenderer>();
      return FontRenderer;
    }

    public override void DivInit()
    {
      EnsureFontRenderer();
      base.DivInit();
    }

    public void SetText(string text)
    {
      EnsureFontRenderer();
      FontRenderer.Text = text;
    }

    public string Text => FontRenderer?.Text ?? string.Empty;
  }

  /// <summary>[过渡] 按钮: 改用 <c>parent.Button(name, text)</c> + <see cref="ButtonScript"/>.</summary>
  [Obsolete("改用 Div + BindScript(new ButtonScript()) / DivFactory.Button(name, text)")]
  public class Button : Div
  {
    public const string TextNodeName = ButtonScript.TextNodeName;

    public Button(string name, string text = "") : base(name)
    {
      Label = new Label(TextNodeName);
      Label.FontRenderer = Label.BindRenderer<DivFontRenderer>();
      Label.FontRenderer.Font = Assets.Font("Fonts/GlowSansMedium.ttf").GetFont((int)UITheme.ButtonFontSize);
      Label.Design.Color = UITheme.ButtonText;
      Design.Color = UITheme.ButtonNormal;
      Attach(Label);
      Label.SetText(text);
      BindScript(new ButtonScript());
    }

    public Label Label;

    private ButtonScript Script => this.GetScript<ButtonScript>();

    public bool Enabled
    {
      get => Script?.Enabled ?? true;
      set { if (Script is not null) Script.Enabled = value; }
    }

    public override void DivInit()
    {
      Label.LayoutScripted = true;
      Interact.IsSelectable = true;
      if (Renderer is null)
        BindRenderer<DivPixelRenderer>();
      base.DivInit();
    }

    /// <summary>以当前颜色为基准重新捕获状态机的乘算底色.</summary>
    public void RecaptureColors() => Script?.RecaptureColors();
  }

  /// <summary>[过渡] 滑动条: 改用 <c>parent.Slider(name)</c> + <see cref="SliderScript"/>.</summary>
  [Obsolete("改用 Div + BindScript(new SliderScript()) / DivFactory.Slider(name)")]
  public class Slider : Div
  {
    public Slider(string name) : base(name)
    {
      Block = new Div(SliderScript.BlockNodeName);
      Attach(Block);
      BindScript(new SliderScript());
    }

    public Div Block;

    private SliderScript Script => this.GetScript<SliderScript>();

    public Direction Direction
    {
      get => Script?.Direction ?? Direction.Vertical;
      set { if (Script is not null) Script.Direction = value; }
    }

    public Vector2 Precent => Script?.Precent ?? Vector2.Zero;

    public override void DivInit()
    {
      base.DivInit();
    }

    public void Bind(Div list, Div view) => Script?.Bind(list, view);
  }

  /// <summary>[过渡] 滚动视口: 改用 <c>parent.ScrollView(name)</c> + <see cref="ScrollViewScript"/>.</summary>
  [Obsolete("改用 Div + BindScript(new ScrollViewScript()) / DivFactory.ScrollView(name)")]
  public class ScrollView : Div
  {
    public ScrollView(string name) : base(name)
    {
      Content = new Div(ScrollViewScript.ContentNodeName);
      Slider = new Slider(ScrollViewScript.SliderNodeName);
      Attach(Content);
      Attach(Slider);
      BindScript(new ScrollViewScript());
    }

    public Div Content;

    public Slider Slider;

    private ScrollViewScript Script => this.GetScript<ScrollViewScript>();

    public bool ShowSlider
    {
      get => Script?.ShowSlider ?? true;
      set { if (Script is not null) Script.ShowSlider = value; }
    }

    public Direction Direction
    {
      get => Script?.Direction ?? Direction.Vertical;
      set { if (Script is not null) Script.Direction = value; }
    }

    public float MaxScroll => Script?.MaxScroll ?? 0f;

    public float ViewportLength => Script?.ViewportLength ?? 0f;

    public override void DivInit()
    {
      base.DivInit();
    }

    public override bool Register(Div div, bool doInit = false) => Content.Register(div, doInit);

    public void ScrollTo(float position) => Script?.ScrollTo(position);

    public void EnsureVisible(float begin, float end) => Script?.EnsureVisible(begin, end);
  }

  /// <summary>[过渡] 列表容器: 改用 <c>parent.DivList(name)</c> + <see cref="DivListScript"/>.</summary>
  [Obsolete("改用 Div + BindScript(new DivListScript()) / DivFactory.DivList(name)")]
  public class DivList : Div
  {
    public DivList(string name, bool isCanvas = false) : base(name, isCanvas)
    {
      List = new Div(DivListScript.ListNodeName);
      Attach(List);
      BindScript(new DivListScript());
    }

    public Div List;

    public Slider Slider;

    private DivListScript Script => this.GetScript<DivListScript>();

    public Controllers.LinearMenuController Linear => Script?.Linear;

    public override void DivInit()
    {
      base.DivInit();
    }

    public override bool Register(Div div, bool doInit = false) => List.Register(div, doInit);

    public void BindSlider(Slider slider)
    {
      Slider = slider;
      Script?.BindSlider(slider);
    }
  }

  /// <summary>[过渡] 单行输入框: 改用 <c>parent.InputBox(name)</c> + <see cref="InputTextBoxScript"/>.</summary>
  [Obsolete("改用 Div + BindScript(new InputTextBoxScript()) / DivFactory.InputBox(name)")]
  public class InputTextBox : Div
  {
    public InputTextBox(string name, int limit = 16) : base(name)
    {
      Label = new Label(InputTextBoxScript.TextNodeName);
      Attach(Label);
      BindScript(new InputTextBoxScript { Limit = limit });
    }

    public Label Label;

    private InputTextBoxScript Script => this.GetScript<InputTextBoxScript>();

    public string Text
    {
      get => Script?.Text ?? string.Empty;
      set { if (Script is not null) Script.Text = value; }
    }

    public int Limit
    {
      get => Script?.Limit ?? 0;
      set { if (Script is not null) Script.Limit = value; }
    }

    public bool Editing => Script?.Editing ?? false;

    public override void DivInit()
    {
      base.DivInit();
    }
  }

  /// <summary>[过渡] 进度条: 改用 <c>parent.ProgressBar(name)</c> + <see cref="ProgressBarScript"/>.</summary>
  [Obsolete("改用 Div + BindScript(new ProgressBarScript()) / DivFactory.ProgressBar(name)")]
  public class ProgressBar : Div
  {
    public ProgressBar(string name) : base(name)
    {
      Fill = new Div(ProgressBarScript.FillNodeName);
      Attach(Fill);
      BindScript(new ProgressBarScript());
    }

    public Div Fill;

    private ProgressBarScript Script => this.GetScript<ProgressBarScript>();

    public float Percentage
    {
      get => Script?.Percentage ?? 0f;
      set { if (Script is not null) Script.Percentage = value; }
    }

    public Direction Direction
    {
      get => Script?.Direction ?? Direction.Horizontal;
      set { if (Script is not null) Script.Direction = value; }
    }

    public Direction Toward
    {
      get => Script?.Toward ?? Direction.Right;
      set { if (Script is not null) Script.Toward = value; }
    }

    public Point FillOffset
    {
      get => Script?.FillOffset ?? Point.Zero;
      set { if (Script is not null) Script.FillOffset = value; }
    }

    public override void DivInit()
    {
      base.DivInit();
    }
  }
}
