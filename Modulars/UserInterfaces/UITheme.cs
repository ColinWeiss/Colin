namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 界面主题令牌: 界面视觉常数 (颜色 / 字号 / 间距) 的单一来源.
  /// <br>预制件的默认外观统一读这里; ColinEditor 的「界面调试」窗口可实时调参.</br>
  /// <br>令牌经 <see cref="ColorTokens"/> / <see cref="ValueTokens"/> 登记, 编辑器按登记表自动枚举, 新增令牌只需一行.</br>
  /// </summary>
  public static class UITheme
  {
    // —— 颜色令牌 ——
    public static Color ButtonNormal = Color.White;
    public static Color ButtonText = Color.Black;
    public static Color SliderBlock = new Color(255, 223, 135);
    public static Color ScrollViewTrack = new Color(38, 36, 33);
    public static Color FormView = new Color(20, 20, 25);
    public static Color FormTitleText = new Color(255, 227, 137);

    // —— 数值令牌 ——
    public static float ButtonFontSize = 20f;
    public static float CornerRadius = 4f;
    public static float ScrollWheelStep = 32f;
    public static float ScrollSmooth = 14f;

    /// <summary>
    /// 颜色令牌的编辑器登记表.
    /// </summary>
    public static readonly List<ColorToken> ColorTokens = new List<ColorToken>
    {
      new ColorToken("按钮-常态", () => ButtonNormal, value => ButtonNormal = value),
      new ColorToken("按钮-文字", () => ButtonText, value => ButtonText = value),
      new ColorToken("滑块", () => SliderBlock, value => SliderBlock = value),
      new ColorToken("滚动区-轨道", () => ScrollViewTrack, value => ScrollViewTrack = value),
      new ColorToken("窗体-视图", () => FormView, value => FormView = value),
      new ColorToken("窗体-标题文字", () => FormTitleText, value => FormTitleText = value),
    };

    /// <summary>
    /// 数值令牌的编辑器登记表.
    /// </summary>
    public static readonly List<ValueToken> ValueTokens = new List<ValueToken>
    {
      new ValueToken("按钮字号", () => ButtonFontSize, value => ButtonFontSize = value, 8f, 48f),
      new ValueToken("圆角半径", () => CornerRadius, value => CornerRadius = value, 0f, 32f),
      new ValueToken("滚轮步长", () => ScrollWheelStep, value => ScrollWheelStep = value, 8f, 128f),
      new ValueToken("滚动平滑", () => ScrollSmooth, value => ScrollSmooth = value, 4f, 32f),
    };

    public readonly struct ColorToken
    {
      public readonly string Name;
      public readonly Func<Color> Get;
      public readonly Action<Color> Set;
      public ColorToken(string name, Func<Color> get, Action<Color> set)
      {
        Name = name;
        Get = get;
        Set = set;
      }
    }

    public readonly struct ValueToken
    {
      public readonly string Name;
      public readonly Func<float> Get;
      public readonly Action<float> Set;
      public readonly float Min;
      public readonly float Max;
      public ValueToken(string name, Func<float> get, Action<float> set, float min, float max)
      {
        Name = name;
        Get = get;
        Set = set;
        Min = min;
        Max = max;
      }
    }
  }
}