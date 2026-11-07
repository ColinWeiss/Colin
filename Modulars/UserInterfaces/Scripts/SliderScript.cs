using Colin.Core.Events;
using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Scripts
{
  /// <summary>
  /// 滑动条行为: 拖拽滑块 / 滚轮微调 / 回读百分比.
  /// <br>节点形状: 子级 <c>"Block"</c> 是滑块, 自身绑像素渲染器.</br>
  /// </summary>
  public sealed class SliderScript : DivScript
  {
    /// <summary>滑块子节点的约定名字.</summary>
    public const string BlockNodeName = "Block";

    /// <summary>指示滑动条的方向; 仅判断 Horizontal / Vertical.</summary>
    public Direction Direction = Direction.Vertical;

    /// <summary>滑块当前所在比例 (0~1).</summary>
    public Vector2 Precent;

    private Div _block;
    private Div _list;
    private Div _view;
    private float _wheelVelocity;
    private Color _blockBase;

    /// <summary>滑块节点 (结构由生成代码/工厂给出, 缺失时补建).</summary>
    public Div Block => _block ??= Owner?.GetChild(BlockNodeName);

    protected override void OnBind()
    {
      Div owner = Owner;
      if (owner is null)
        return;
      _block = owner.GetChild(BlockNodeName);
      if (_block is null)
      {
        _block = new Div(BlockNodeName);
        owner.Attach(_block);
      }
      if (_block.Renderer is null)
        _block.BindRenderer<DivPixelRenderer>();
      _block.Design.Color = UITheme.SliderBlock;
      if (owner.Renderer is null)
        owner.BindRenderer<DivPixelRenderer>();
      _block.Interact.IsDraggable = true;
      ApplyBlockDefaultSize();
      _blockBase = _block.Design.Color;
    }

    /// <summary>按方向给滑块一个合理的初始长宽 (改方向后也要跟着换).</summary>
    public void ApplyBlockDefaultSize()
    {
      Div owner = Owner;
      if (owner is null || _block is null)
        return;
      if (Direction is Direction.Vertical)
      {
        _block.Layout.Width = owner.Layout.Width;
        if (_block.Layout.Height == 0)
          _block.Layout.Height = 24;
      }
      if (Direction is Direction.Horizontal)
      {
        if (_block.Layout.Width == 0)
          _block.Layout.Width = 24;
        _block.Layout.Height = owner.Layout.Height;
      }
    }

    /// <summary>把滚轮事件接到指定内容视口上 (list 为被滚动的内容, view 为接收滚轮的视口).</summary>
    public void Bind(Div list, Div view)
    {
      if (_view is not null)
      {
        _view.Events.ScrollDown -= WheelEvent;
        _view.Events.ScrollUp -= WheelEvent;
      }
      _list = list;
      _view = view;
      if (_view is not null)
      {
        _view.Events.ScrollDown += WheelEvent;
        _view.Events.ScrollUp += WheelEvent;
      }
    }

    private void WheelEvent(object sender, MouseArgs args)
    {
      if (MouseResponder.ScrollDown)
        _wheelVelocity = -6f;
      else if (MouseResponder.ScrollUp)
        _wheelVelocity = 6f;
    }

    public override void ExportValues(List<UIDesignValue> values)
    {
      values.Add(UIDesignValue.Field(nameof(Direction), Direction));
      base.ExportValues(values);
    }

    protected override void OnUpdate(GameTime time)
    {
      Div owner = Owner;
      if (owner is null || _block is null)
        return;
      //容器尺寸不足以容纳滑块时钳制区间退化为空, 收拢到下界, 避免越界与 Precent 出现 NaN.
      float minX = owner.Layout.PaddingLeft;
      float minY = owner.Layout.PaddingTop;
      float maxX = Math.Max(minX, owner.Layout.Width - owner.Layout.PaddingRight - _block.Layout.Width);
      float maxY = Math.Max(minY, owner.Layout.Height - owner.Layout.PaddingBottom - _block.Layout.Height);

      _block.Layout.Left = Math.Clamp(_block.Layout.Left, minX, maxX);
      _block.Layout.Top = Math.Clamp(_block.Layout.Top, minY, maxY);

      _block.Layout.Top += _wheelVelocity;
      _wheelVelocity *= 0.9f;

      float rangeX = maxX - minX;
      float rangeY = maxY - minY;
      Precent = new Vector2(
        rangeX > 0f ? (_block.Layout.Left - minX) / rangeX : 0f,
        rangeY > 0f ? (_block.Layout.Top - minY) / rangeY : 0f);

      //状态反馈: 拖拽中最亮, 悬停次之, 以绑定时刻的底色乘算, 不吃掉外部改色.
      float tint = _block.Events.DraggingState ? 1.2f : _block.Interact.Interaction ? 1.08f : 1f;
      _block.Design.Color = new Color(
        (int)Math.Clamp(_blockBase.R * tint, 0, 255),
        (int)Math.Clamp(_blockBase.G * tint, 0, 255),
        (int)Math.Clamp(_blockBase.B * tint, 0, 255),
        _blockBase.A);
    }
  }
}
