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
    /// <summary>
    /// 百分比定位: 锚定于父级内容区内按 <see cref="DivLayoutStyle.PercentPoint"/> 指定的比例点 (0 即起始边, 1 即结束边).
    /// </summary>
    Percent,
    /// <summary>
    /// 排于参照区间之前: 本元素的结束边贴住参照起始边 (左/上侧外贴), <see cref="DivLayoutStyle.OffsetEnd"/> 为与参照的间隙.
    /// </summary>
    Before,
    /// <summary>
    /// 排于参照区间之后: 本元素的起始边贴住参照结束边 (右/下侧外贴), <see cref="DivLayoutStyle.OffsetBegin"/> 为与参照的间隙.
    /// </summary>
    After,
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
    /// 自适应: 尺寸取自 <see cref="Div.MeasureDesired"/> 的内容测量结果, 并附加 <see cref="DivLayoutStyle.AutoExtra"/>.
    /// </summary>
    Auto,
    /// <summary>
    /// 匹配: 尺寸取自锚点目标 (见 <see cref="DivLayoutStyle.AnchorTarget"/>) 的同轴尺寸, 并附加 <see cref="DivLayoutStyle.MatchOffset"/>.
    /// <br>用于跨元素跟随 (如血条宽度跟随快捷栏); 目标失效时保持自身当前尺寸.</br>
    /// </summary>
    Match,
  }

  /// <summary>
  /// 划分元素的布局样式: 声明式锚定 / 停靠 / 百分比定位 / 百分比尺寸 / 自适应尺寸 / 跨元素匹配.
  /// <br>水平轴与垂直轴相互独立; 全部为默认值时布局系统不做任何干预, 与手动布局完全兼容.</br>
  /// <br>布局样式的解析发生于每帧控制器布局之后、 <see cref="Div.LayoutCalculate"/> 之前, 解析结果经布局脏标记机制进入变换计算.</br>
  /// <br>[!] 同一轴向上, 布局样式与逐帧重排子级的组控制器 (如 <c>LinearMenuController</c>) 不应同时使用.</br>
  /// <br>[!] <see cref="DivSizeMode.Match"/> 引用其他元素, 相互引用不会死锁 (各读对方上一帧结果), 但可能振荡; 请保持引用为单向.</br>
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
    /// 锚点目标元素 (通常为兄弟级, 如血条跟随快捷栏).
    /// <br>尺寸模式为 <see cref="DivSizeMode.Match"/> 时取其同轴尺寸; 轴向锚定 (Begin/Center/End/Percent/Stretch)
    /// 设有目标时位置改以目标矩形为参照 (贴着目标排), 未设时以父级内容区为参照. 为空或已释放时全部按父级解析.</br>
    /// </summary>
    public Div AnchorTarget;

    /// <summary>
    /// 匹配尺寸的附加偏移.
    /// <br>仅于尺寸模式为 <see cref="DivSizeMode.Match"/> 时生效; X 附加于宽度, Y 附加于高度.</br>
    /// </summary>
    public Vector2 MatchOffset;

    /// <summary>
    /// 自适应尺寸在内容测量结果之外的附加量 (如四周镶边; 前导留白由子级自身位置体现).
    /// <br>仅于尺寸模式为 <see cref="DivSizeMode.Auto"/> 时生效; X 附加于宽度, Y 附加于高度.</br>
    /// </summary>
    public Vector2 AutoExtra;

    /// <summary>
    /// 百分比定位点, 相对父级内容区, 0 即起始边, 1 即结束边.
    /// <br>仅于 <see cref="Horizontal"/> / <see cref="Vertical"/> 为 <see cref="DivAxisAnchor.Percent"/> 时生效; 水平轴取 X, 垂直轴取 Y.</br>
    /// </summary>
    public Vector2 PercentPoint;

    /// <summary>
    /// 文档还原用: 锚点目标的元素名 (见 <see cref="Documents.UIDocument"/>).
    /// <br>代码直接设 <see cref="AnchorTarget"/>; 该暂存名仅在文档读入时填写, 还原后由文档解析为元素引用.</br>
    /// </summary>
    public string AnchorTargetName;

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

      //解析尺寸; 结果供 Center / End / Percent 位置解析使用.
      float width;
      switch (WidthMode)
      {
        case DivSizeMode.Percent:
          width = MathF.Max(0f, PercentWidth * contentWidth);
          div.Layout.Width = width;
          break;
        case DivSizeMode.Auto:
          width = MathF.Max(0f, desired.X + AutoExtra.X);
          div.Layout.Width = width;
          break;
        case DivSizeMode.Match:
          if (AnchorTarget is Div target && target.Disposed is false && ReferenceEquals(target, div) is false)
          {
            width = MathF.Max(0f, target.Layout.Width + MatchOffset.X);
            div.Layout.Width = width;
          }
          else
            width = div.Layout.Width;
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
          height = MathF.Max(0f, desired.Y + AutoExtra.Y);
          div.Layout.Height = height;
          break;
        case DivSizeMode.Match:
          if (AnchorTarget is Div targetY && targetY.Disposed is false && ReferenceEquals(targetY, div) is false)
          {
            height = MathF.Max(0f, targetY.Layout.Height + MatchOffset.Y);
            div.Layout.Height = height;
          }
          else
            height = div.Layout.Height;
          break;
        default:
          height = div.Layout.Height;
          break;
      }

      //轴锚参照矩形: 设有锚点目标时取目标的矩形 (同父用相对坐标, 跨父经屏幕坐标差换算, 均为上一帧结果),
      //未设时取父级内容区 —— 位置锚定从此可"贴着另一个元素排".
      float refBeginX = contentBeginX, refSpanX = contentWidth, refEndX = contentEndX;
      float refBeginY = contentBeginY, refSpanY = contentHeight, refEndY = contentEndY;
      if (AnchorTarget is Div anchorRef && anchorRef.Disposed is false && ReferenceEquals(anchorRef, div) is false)
      {
        float targetLeft, targetTop;
        if (anchorRef.Parent == parent)
        {
          targetLeft = anchorRef.Layout.Left;
          targetTop = anchorRef.Layout.Top;
        }
        else
        {
          targetLeft = anchorRef.Layout.ScreenLocation.X - parent.Layout.ScreenLocation.X;
          targetTop = anchorRef.Layout.ScreenLocation.Y - parent.Layout.ScreenLocation.Y;
        }
        refBeginX = targetLeft;
        refSpanX = anchorRef.Layout.Width;
        refEndX = targetLeft + refSpanX;
        refBeginY = targetTop;
        refSpanY = anchorRef.Layout.Height;
        refEndY = targetTop + refSpanY;
      }

      //解析位置; Stretch 模式的尺寸由参照矩形与两侧留白推导, 覆盖此前解析的尺寸.
      switch (Horizontal)
      {
        case DivAxisAnchor.Begin:
          div.Layout.Left = refBeginX + OffsetBegin.X;
          break;
        case DivAxisAnchor.Center:
          div.Layout.Left = refBeginX + (refSpanX - width) / 2 + OffsetBegin.X;
          break;
        case DivAxisAnchor.End:
          div.Layout.Left = refEndX - width - OffsetEnd.X;
          break;
        case DivAxisAnchor.Percent:
          div.Layout.Left = refBeginX + refSpanX * PercentPoint.X + OffsetBegin.X;
          break;
        case DivAxisAnchor.Before:
          div.Layout.Left = refBeginX - width - OffsetEnd.X;
          break;
        case DivAxisAnchor.After:
          div.Layout.Left = refEndX + OffsetBegin.X;
          break;
        case DivAxisAnchor.Stretch:
          div.Layout.Left = refBeginX + OffsetBegin.X;
          div.Layout.Width = MathF.Max(0f, refSpanX - OffsetBegin.X - OffsetEnd.X);
          break;
      }
      switch (Vertical)
      {
        case DivAxisAnchor.Begin:
          div.Layout.Top = refBeginY + OffsetBegin.Y;
          break;
        case DivAxisAnchor.Center:
          div.Layout.Top = refBeginY + (refSpanY - height) / 2 + OffsetBegin.Y;
          break;
        case DivAxisAnchor.End:
          div.Layout.Top = refEndY - height - OffsetEnd.Y;
          break;
        case DivAxisAnchor.Percent:
          div.Layout.Top = refBeginY + refSpanY * PercentPoint.Y + OffsetBegin.Y;
          break;
        case DivAxisAnchor.Before:
          div.Layout.Top = refBeginY - height - OffsetEnd.Y;
          break;
        case DivAxisAnchor.After:
          div.Layout.Top = refEndY + OffsetBegin.Y;
          break;
        case DivAxisAnchor.Stretch:
          div.Layout.Top = refBeginY + OffsetBegin.Y;
          div.Layout.Height = MathF.Max(0f, refSpanY - OffsetBegin.Y - OffsetEnd.Y);
          break;
      }
    }
  }
}
