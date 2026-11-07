using Colin.Core.Events;
using Colin.Core.Modulars.UserInterfaces.Controllers;
using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Scripts
{
  /// <summary>
  /// 滚动视口行为: 线性列表 + 滚轮 + 平滑滚动 + 视口外剔除 (虚拟化) + 可选滚动条.
  /// <br>节点形状: 子级 <c>"Content"</c> (内容容器, 挂线性排列控制器) 与 <c>"Slider"</c> (滚动条, 挂 <see cref="SliderScript"/>).</br>
  /// <br>[!] 滚轮语义与 Colin 命名坑对齐: 物理下滚触发 ScrollUp, 物理上滚触发 ScrollDown.</br>
  /// <br>[!] 内容项请直接 Attach 到 <see cref="Content"/>; 视口外的项会被置 <see cref="Div.IsCulled"/> 跳过更新/渲染/事件.</br>
  /// </summary>
  public sealed class ScrollViewScript : DivScript
  {
    /// <summary>内容容器子节点的约定名字.</summary>
    public const string ContentNodeName = "Content";

    /// <summary>滚动条子节点的约定名字.</summary>
    public const string SliderNodeName = "Slider";

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

    private Div _content;
    private Div _slider;
    private SliderScript _sliderScript;
    private LinearMenuController _linear;
    private float _scroll;
    private float _target;

    /// <summary>内容容器 (项挂到它下面).</summary>
    public Div Content => _content ??= Owner?.GetChild(ContentNodeName);

    /// <summary>滚动条节点.</summary>
    public Div Slider => _slider ??= Owner?.GetChild(SliderNodeName);

    /// <summary>当前滚动偏移 (0 ~ <see cref="MaxScroll"/>).</summary>
    public float Scroll => _scroll;

    /// <summary>最大可滚动偏移; 内容不溢出时为 0.</summary>
    public float MaxScroll { get; private set; }

    /// <summary>视口在滚动轴向上的净长度 (扣除内边距与滚动条).</summary>
    public float ViewportLength { get; private set; }

    protected override void OnBind()
    {
      Div owner = Owner;
      if (owner is null)
        return;
      owner.Layout.ScissorEnable = true;

      _content = owner.GetChild(ContentNodeName);
      if (_content is null)
      {
        _content = new Div(ContentNodeName);
        owner.Attach(_content);
      }
      _slider = owner.GetChild(SliderNodeName);
      if (_slider is null)
      {
        _slider = new Div(SliderNodeName);
        owner.Attach(_slider);
      }
      _sliderScript = _slider.GetScript<SliderScript>();
      if (_sliderScript is null)
        _sliderScript = _slider.BindScript(new SliderScript());
      _sliderScript.Direction = Direction;
      _slider.Layout.Width = SliderThickness;

      _linear = _content.Controller as LinearMenuController;
      if (_linear is null)
        _linear = _content.BindController<LinearMenuController>();
      _linear.AutoSetSize = true;
      _linear.DivInterval = ItemInterval;
      _linear.Direction = Direction;
      _linear.Toward = Direction == Direction.Horizontal ? Direction.Right : Direction.Down;
      _linear.Alignment = Direction == Direction.Horizontal ? Direction.Up : Direction.Left;

      owner.Events.ScrollDown += OnWheelUp;
      owner.Events.ScrollUp += OnWheelDown;
      Unhook(() =>
      {
        owner.Events.ScrollDown -= OnWheelUp;
        owner.Events.ScrollUp -= OnWheelDown;
      });
      _content.LayoutScripted = true;
      _slider.LayoutScripted = true;
      if (_sliderScript.Block is not null)
        _sliderScript.Block.LayoutScripted = true;
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

    /// <summary>滚动到指定偏移 (自动钳制); 滚轮与程序化滚动共用同一平滑逼近.</summary>
    public void ScrollTo(float position) => _target = position;

    /// <summary>滚动视口, 使内容坐标区间 [begin, end] 完整可见.</summary>
    public void EnsureVisible(float begin, float end)
    {
      if (end > _scroll + ViewportLength)
        ScrollTo(end - ViewportLength);
      else if (begin < _scroll)
        ScrollTo(begin);
    }

    public override void ExportValues(List<UIDesignValue> values)
    {
      values.Add(UIDesignValue.Field(nameof(Direction), Direction));
      values.Add(UIDesignValue.Field(nameof(ItemInterval), ItemInterval));
      values.Add(UIDesignValue.Field(nameof(SliderThickness), SliderThickness));
      values.Add(UIDesignValue.Field(nameof(ShowSlider), ShowSlider));
      values.Add(UIDesignValue.Field(nameof(CullMargin), CullMargin));
      base.ExportValues(values);
    }

    protected override void OnLayoutCalculate(ref DivLayout layout)
    {
      Div owner = Owner;
      if (owner is null || _content is null || _linear is null)
        return;
      owner.Layout.ScissorWidth = (int)owner.Layout.Width;
      owner.Layout.ScissorHeight = (int)owner.Layout.Height;

      bool vertical = Direction == Direction.Vertical;
      Div block = _sliderScript?.Block;
      bool sliderShown = _slider is not null && _slider.IsCulled is false;
      float paddingLeft = owner.Layout.PaddingLeft + (vertical && sliderShown ? SliderThickness : 0f);
      float paddingTop = owner.Layout.PaddingTop + (vertical ? 0f : SliderThickness);
      float viewportX = owner.Layout.Width - paddingLeft - owner.Layout.PaddingRight;
      float viewportY = owner.Layout.Height - paddingTop - owner.Layout.PaddingBottom;
      ViewportLength = vertical ? viewportY : viewportX;

      //内容总长与最大偏移; 控制器上一帧刚排完, 取值滞后一帧, 对滚动无感.
      float contentLength = vertical ? _linear.TotalSize.Y : _linear.TotalSize.X;
      MaxScroll = MathF.Max(0f, contentLength - ViewportLength);

      //剔除只看"有没有滚动条", 不依赖滚动条脚本是否已绑定 (否则未初始化时永远不剔除).
      if (_slider is not null)
        _slider.IsCulled = ShowSlider is false || MaxScroll <= 0.5f;
      if (_slider is not null && block is not null)
      {
        if (block.Events.DraggingState && MaxScroll > 0f)
          _target = (vertical ? _sliderScript.Precent.Y : _sliderScript.Precent.X) * MaxScroll;
        float percent = MaxScroll > 0f ? _scroll / MaxScroll : 0f;
        if (vertical)
        {
          float track = owner.Layout.Height - owner.Layout.PaddingTop - owner.Layout.PaddingBottom - block.Layout.Height;
          block.Layout.Top = owner.Layout.PaddingTop + percent * MathF.Max(0f, track);
        }
        else
        {
          float track = owner.Layout.Width - owner.Layout.PaddingLeft - owner.Layout.PaddingRight - block.Layout.Width;
          block.Layout.Left = owner.Layout.PaddingLeft + percent * MathF.Max(0f, track);
        }
      }

      float blend = MathF.Min(1f, Time.UnscaledDeltaTime * UITheme.ScrollSmooth);
      _scroll += (_target - _scroll) * blend;
      if (MathF.Abs(_target - _scroll) < 0.5f)
        _scroll = _target;
      _scroll = Math.Clamp(_scroll, 0f, MaxScroll);
      _target = Math.Clamp(_target, 0f, MaxScroll);

      if (vertical)
        _content.Layout.SetLocation(paddingLeft, paddingTop - _scroll);
      else
        _content.Layout.SetLocation(paddingLeft - _scroll, paddingTop);

      List<Div> items = _content.Children;
      for (int index = 0; index < items.Count; index++)
      {
        Div item = items[index];
        float begin = vertical ? item.Layout.Top : item.Layout.Left;
        float length = vertical ? item.Layout.Height : item.Layout.Width;
        item.IsCulled = begin + length < _scroll - CullMargin || begin > _scroll + ViewportLength + CullMargin;
      }
    }
  }
}
