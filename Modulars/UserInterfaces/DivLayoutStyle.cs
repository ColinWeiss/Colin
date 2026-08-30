namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 指示划分元素于单一轴向上的锚定方式.
  /// <br>同一枚举适用于水平轴与垂直轴: Begin 对应左/上, End 对应右/下.</br>
  /// </summary>
  public enum DivAxisAnchor
  {
    /// <summary>
    /// 不参与布局样式; 该轴向保留手动布局 (直接设定 <see cref="DivLayout.Left"/> / <see cref="DivLayout.Top"/>).
    /// </summary>
    None,
    /// <summary>
    /// 锚定于父级内容区的起始边 (左/上), 以 <see cref="DivLayoutStyle.OffsetBegin"/> 指定边距.
    /// </summary>
    Begin,
    /// <summary>
    /// 居中于父级内容区, 以 <see cref="DivLayoutStyle.OffsetBegin"/> 作居中位置上的微调偏移.
    /// </summary>
    Center,
    /// <summary>
    /// 锚定于父级内容区的结束边 (右/下), 以 <see cref="DivLayoutStyle.OffsetEnd"/> 指定边距 (向内为正).
    /// </summary>
    End,
    /// <summary>
    /// 停靠拉伸: 于父级内容区内, 由 <see cref="DivLayoutStyle.OffsetBegin"/> 与 <see cref="DivLayoutStyle.OffsetEnd"/> 指定两侧留白, 尺寸由内容区与留白推导.
    /// <br>两侧留白为零即为停靠填满; 该轴向的尺寸模式被忽略.</br>
    /// </summary>
    Stretch,
  }

  /// <summary>
  /// 指示划分元素单一轴向尺寸的解析方式.
  /// </summary>
  public enum DivSizeMode
  {
    /// <summary>
    /// 定值: 直接使用 <see cref="DivLayout.Width"/> / <see cref="DivLayout.Height"/>.
    /// </summary>
    Fixed,
    /// <summary>
    /// 百分比: 尺寸为父级内容区尺寸乘以 <see cref="DivLayoutStyle.PercentWidth"/> / <see cref="DivLayoutStyle.PercentHeight"/> (1 即 100%).
    /// </summary>
    Percent,
    /// <summary>
    /// 自适应: 尺寸取自 <see cref="Div.MeasureDesired"/> 的内容测量结果.
    /// </summary>
    Auto,
  }

  /// <summary>
  /// 划分元素的布局样式: 声明式锚定 / 停靠 / 百分比尺寸 / 自适应尺寸.
  /// <br>水平轴与垂直轴相互独立; 全部为默认值时布局系统不做任何干预, 与手动布局完全兼容.</br>
  /// <br>布局样式的解析发生于每帧控制器布局之后、 <see cref="Div.LayoutCalculate"/> 之前, 解析结果经布局脏标记机制进入变换计算.</br>
  /// <br>[!] 同一轴向上, 布局样式与逐帧重排子级的组控制器 (如 <c>LinearMenuController</c>) 不应同时使用.</br>
  /// </summary>
  public struct DivLayoutStyle
  {
    /// <summary>
    /// 指示划分元素于父级内容区内的水平锚定方式.
    /// </summary>
    public DivAxisAnchor Horizontal;

    /// <summary>
    /// 指示划分元素于父级内容区内的垂直锚定方式.
    /// </summary>
    public DivAxisAnchor Vertical;

    /// <summary>
    /// 起始侧偏移.
    /// <br>Begin 模式: 距父级内容区左/上边缘的距离.</br>
    /// <br>Center 模式: 于居中位置基础上的微调偏移.</br>
    /// <br>Stretch 模式: 内容区起始边到划分元素的留白.</br>
    /// </summary>
    public Vector2 OffsetBegin;

    /// <summary>
    /// 结束侧偏移.
    /// <br>End 模式: 距父级内容区右/下边缘的距离 (向内为正).</br>
    /// <br>Stretch 模式: 内容区结束边到划分元素的留白 (向内为正).</br>
    /// </summary>
    public Vector2 OffsetEnd;

    /// <summary>
    /// 指示宽度解析方式.
    /// </summary>
    public DivSizeMode WidthMode;

    /// <summary>
    /// 指示高度解析方式.
    /// </summary>
    public DivSizeMode HeightMode;

    /// <summary>
    /// 宽度百分比, 相对父级内容区宽度, 1 即 100%.
    /// <br>仅于 <see cref="WidthMode"/> 为 <see cref="DivSizeMode.Percent"/> 时生效.</br>
    /// </summary>
    public float PercentWidth;

    /// <summary>
    /// 高度百分比, 相对父级内容区高度, 1 即 100%.
    /// <br>仅于 <see cref="HeightMode"/> 为 <see cref="DivSizeMode.Percent"/> 时生效.</br>
    /// </summary>
    public float PercentHeight;

    /// <summary>
    /// 创建停靠填满父级内容区的布局样式.
    /// </summary>
    /// <param name="margin">四周留白.</param>
    public static DivLayoutStyle Fill(float margin = 0f)
      => new DivLayoutStyle()
      {
        Horizontal = DivAxisAnchor.Stretch,
        Vertical = DivAxisAnchor.Stretch,
        OffsetBegin = new Vector2(margin),
        OffsetEnd = new Vector2(margin),
      };

    /// <summary>
    /// 创建于父级内容区内居中的布局样式.
    /// </summary>
    public static DivLayoutStyle Centered()
      => new DivLayoutStyle()
      {
        Horizontal = DivAxisAnchor.Center,
        Vertical = DivAxisAnchor.Center,
      };

    /// <summary>
    /// 依据布局样式解析划分元素于父级内容区中的位置与尺寸.
    /// <br>样式为默认值或划分元素没有父级时, 不做任何处理.</br>
    /// </summary>
    /// <param name="div">要应用布局样式的划分元素.</param>
    /// <param name="parent">划分元素的父级, 作为内容区参照.</param>
    public void Apply(Div div, Div parent)
    {
      if (parent is null)
        return;
      if (Horizontal == DivAxisAnchor.None && Vertical == DivAxisAnchor.None &&
          WidthMode == DivSizeMode.Fixed && HeightMode == DivSizeMode.Fixed)
        return;

      float contentBeginX = parent.Layout.PaddingLeft;
      float contentEndX = parent.Layout.Width - parent.Layout.PaddingRight;
      float contentBeginY = parent.Layout.PaddingTop;
      float contentEndY = parent.Layout.Height - parent.Layout.PaddingBottom;
      float contentWidth = contentEndX - contentBeginX;
      float contentHeight = contentEndY - contentBeginY;

      Vector2 desired = Vector2.Zero;
      if (WidthMode == DivSizeMode.Auto || HeightMode == DivSizeMode.Auto)
        desired = div.MeasureDesired();

      //解析尺寸; 结果供 Center / End 位置解析使用.
      float width;
      switch (WidthMode)
      {
        case DivSizeMode.Percent:
          width = MathF.Max(0f, PercentWidth * contentWidth);
          div.Layout.Width = width;
          break;
        case DivSizeMode.Auto:
          width = MathF.Max(0f, desired.X);
          div.Layout.Width = width;
          break;
        default:
          width = div.Layout.Width;
          break;
      }
      float height;
      switch (HeightMode)
      {
        case DivSizeMode.Percent:
          height = MathF.Max(0f, PercentHeight * contentHeight);
          div.Layout.Height = height;
          break;
        case DivSizeMode.Auto:
          height = MathF.Max(0f, desired.Y);
          div.Layout.Height = height;
          break;
        default:
          height = div.Layout.Height;
          break;
      }

      //解析位置; Stretch 模式的尺寸由内容区与两侧留白推导, 覆盖此前解析的尺寸.
      switch (Horizontal)
      {
        case DivAxisAnchor.Begin:
          div.Layout.Left = contentBeginX + OffsetBegin.X;
          break;
        case DivAxisAnchor.Center:
          div.Layout.Left = contentBeginX + (contentWidth - width) / 2 + OffsetBegin.X;
          break;
        case DivAxisAnchor.End:
          div.Layout.Left = contentEndX - width - OffsetEnd.X;
          break;
        case DivAxisAnchor.Stretch:
          div.Layout.Left = contentBeginX + OffsetBegin.X;
          div.Layout.Width = MathF.Max(0f, contentWidth - OffsetBegin.X - OffsetEnd.X);
          break;
      }
      switch (Vertical)
      {
        case DivAxisAnchor.Begin:
          div.Layout.Top = contentBeginY + OffsetBegin.Y;
          break;
        case DivAxisAnchor.Center:
          div.Layout.Top = contentBeginY + (contentHeight - height) / 2 + OffsetBegin.Y;
          break;
        case DivAxisAnchor.End:
          div.Layout.Top = contentEndY - height - OffsetEnd.Y;
          break;
        case DivAxisAnchor.Stretch:
          div.Layout.Top = contentBeginY + OffsetBegin.Y;
          div.Layout.Height = MathF.Max(0f, contentHeight - OffsetBegin.Y - OffsetEnd.Y);
          break;
      }
    }
  }
}
