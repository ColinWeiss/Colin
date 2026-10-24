using Colin.Core.Events;
using Colin.Core.Modulars.UserInterfaces.Controllers;
using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
  /// <summary>
  /// 滚动视口: 线性列表 + 内建滚轮 + 平滑滚动 + 视口外项剔除 (虚拟化) + 可选滚动条.
  /// <br>滚动方向由 <see cref="Direction"/> 指定; 滚轮语义与 Colin 命名坑对齐: 物理下滚触发 ScrollUp, 物理上滚触发 ScrollDown.</br>
  /// <br>视口外的内容项会置 <see cref="Div.IsCulled"/> 跳过更新/渲染/事件; 想往里塞项用 <see cref="Register"/> (转发给内容容器).</br>
  /// <br>[!] 内容项勿自行启用 <see cref="DivLayoutStyle"/> 的滚动轴向锚定, 位置由 <see cref="LinearMenuController"/> 逐帧重排.</br>
  /// </summary>
  public class ScrollView : Div
  {
    public ScrollView(string name) : base(name)
    {
      Content = new Div("Content");
    }

    /// <summary>
    /// 内容容器; 项注册到它上面, 由线性菜单控制器逐帧排列.
    /// </summary>
    public Div Content;

    /// <summary>
    /// 滚动条; 于 <see cref="DivInit"/> 自动创建, 跟随滚动状态双向同步.
    /// </summary>
    public Slider Slider;

    /// <summary>指示是否显示滚动条; 内容不溢出时滚动条无论如何都隐藏.</summary>
    public bool ShowSlider = true;

    /// <summary>滚动方向.</summary>
    public Direction Direction = Direction.Vertical;

    /// <summary>项间隔.</summary>
    public int ItemInterval = 4;

    /// <summary>滚动条厚度.</summary>
    public float SliderThickness = 6f;

    /// <summary>剔除判定时视口边缘的外扩像素, 兼容项进出时的位置滞后.</summary>
    public float CullMargin = 48f;

    /// <summary>当前滚动偏移 (0 ~ <see cref="MaxScroll"/>).</summary>
    public float Scroll => _scroll;

    /// <summary>最大可滚动偏移; 内容不溢出时为 0.</summary>
    public float MaxScroll { get; private set; }

    /// <summary>
    /// 视口在滚动轴向上的净长度 (扣除内边距与滚动条).
    /// </summary>
    public float ViewportLength { get; private set; }

    private LinearMenuController _linear;
    private float _scroll;
    private float _target;

    public override void DivInit()
    {
      Layout.ScissorEnable = true;

      _linear = Content.BindController<LinearMenuController>();
      _linear.AutoSetSize = true;
      _linear.DivInterval = ItemInterval;
      _linear.Direction = Direction;
      _linear.Toward = Direction == Direction.Horizontal ? Direction.Right : Direction.Down;
      _linear.Alignment = Direction == Direction.Horizontal ? Direction.Up : Direction.Left;

      Slider = new Slider("Slider");
      Slider.Direction = Direction;
      Slider.Layout.Width = SliderThickness;
      Slider.BindRenderer<DivPixelRenderer>();
      Slider.Block.Layout.Width = SliderThickness;

      Events.ScrollDown += OnWheelUp;
      Events.ScrollUp += OnWheelDown;
      Register(Content);
      Register(Slider);
      Content.LayoutScripted = true;
      Slider.LayoutScripted = true;
      Slider.Block.LayoutScripted = true;
      base.DivInit();
    }

    private void OnWheelUp(object sender, MouseArgs args)
    {
      if (MaxScroll <= 0f)
        return;
      args.IsCapture = true;
      ScrollTo(_target - UITheme.ScrollWheelStep);
    }

    private void OnWheelDown(object sender, MouseArgs args)
    {
      if (MaxScroll <= 0f)
        return;
      args.IsCapture = true;
      ScrollTo(_target + UITheme.ScrollWheelStep);
    }

    /// <summary>
    /// 注册内容项; 实际挂到 <see cref="Content"/> 下.
    /// </summary>
    public override bool Register(Div div, bool doInit = false)
      => Content.Register(div, doInit);

    /// <summary>
    /// 滚动到指定偏移 (自动钳制); 滚轮与程序化滚动共用同一平滑逼近.
    /// </summary>
    public void ScrollTo(float position)
      => _target = position;

    /// <summary>
    /// 滚动视口, 使内容坐标区间 [begin, end] 完整可见.
    /// </summary>
    public void EnsureVisible(float begin, float end)
    {
      if (end > _scroll + ViewportLength)
        ScrollTo(end - ViewportLength);
      else if (begin < _scroll)
        ScrollTo(begin);
    }

    public override void LayoutCalculate(ref DivLayout layout)
    {
      Layout.ScissorWidth = (int)Layout.Width;
      Layout.ScissorHeight = (int)Layout.Height;

      bool vertical = Direction == Direction.Vertical;
      float paddingLeft = Layout.PaddingLeft + (vertical && Slider is not null && Slider.IsVisible ? SliderThickness : 0f);
      float paddingTop = Layout.PaddingTop + (vertical ? 0f : SliderThickness);
      float viewportX = Layout.Width - paddingLeft - Layout.PaddingRight;
      float viewportY = Layout.Height - paddingTop - Layout.PaddingBottom;
      ViewportLength = vertical ? viewportY : viewportX;

      //内容总长与最大偏移; 控制器上一帧刚排完, 取值滞后一帧, 对滚动无感.
      float contentLength = vertical ? _linear.TotalSize.Y : _linear.TotalSize.X;
      MaxScroll = MathF.Max(0f, contentLength - ViewportLength);

      //滚动条拖拽优先回读; 其余时刻由滚动状态反推滑块位置.
      if (Slider is not null && Slider.Block is not null)
      {
        if (Slider.Block.Events.DraggingState && MaxScroll > 0f)
          _target = (vertical ? Slider.Precent.Y : Slider.Precent.X) * MaxScroll;
        float percent = MaxScroll > 0f ? _scroll / MaxScroll : 0f;
        if (vertical)
        {
          float track = Layout.Height - Layout.PaddingTop - Layout.PaddingBottom - Slider.Block.Layout.Height;
          Slider.Block.Layout.Top = Layout.PaddingTop + percent * MathF.Max(0f, track);
        }
        else
        {
          float track = Layout.Width - Layout.PaddingLeft - Layout.PaddingRight - Slider.Block.Layout.Width;
          Slider.Block.Layout.Left = Layout.PaddingLeft + percent * MathF.Max(0f, track);
        }
        Slider.IsVisible = ShowSlider && MaxScroll > 0.5f;
      }

      //平滑逼近目标偏移.
      float blend = MathF.Min(1f, Time.UnscaledDeltaTime * UITheme.ScrollSmooth);
      _scroll += (_target - _scroll) * blend;
      if (MathF.Abs(_target - _scroll) < 0.5f)
        _scroll = _target;
      _scroll = Math.Clamp(_scroll, 0f, MaxScroll);
      _target = Math.Clamp(_target, 0f, MaxScroll);

      if (vertical)
        Content.Layout.SetLocation(paddingLeft, paddingTop - _scroll);
      else
        Content.Layout.SetLocation(paddingLeft - _scroll, paddingTop);

      //视口外项剔除: 依据项在内容容器内的相对位置判定.
      List<Div> items = Content.Children;
      for (int index = 0; index < items.Count; index++)
      {
        Div item = items[index];
        float begin = vertical ? item.Layout.Top : item.Layout.Left;
        float length = vertical ? item.Layout.Height : item.Layout.Width;
        item.IsCulled = begin + length < _scroll - CullMargin || begin > _scroll + ViewportLength + CullMargin;
      }
      base.LayoutCalculate(ref layout);
    }
  }
}