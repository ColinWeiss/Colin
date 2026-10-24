using Colin.Core.Events;
using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
  public class Slider : Div
  {
    public Slider(string name) : base(name) { }

    public Div Block;

    /// <summary>
    /// 指示滑动条的方向.
    /// <br>仅判断 <see cref="Direction.Horizontal"/> 与 <see cref="Direction.Vertical"/>.</br>
    /// </summary>
    public Direction Direction = Direction.Vertical;

    public Vector2 Precent;

    private Div List;
    private Div View;

    private float WheelVelocity = 0;

    public void Bind(Div list, Div view)
    {
      if (View is not null)
      {
        View.Events.ScrollDown -= WheelEvent;
        View.Events.ScrollUp -= WheelEvent;
      }
      List = list;
      View = view;
      View.Events.ScrollDown += WheelEvent;
      View.Events.ScrollUp += WheelEvent;
    }
    private void WheelEvent(object sender, MouseArgs args)
    {
      if (MouseResponder.ScrollDown)
      {
        WheelVelocity = -6f;
      }
      else if (MouseResponder.ScrollUp)
      {
        WheelVelocity = 6f;
      }
    }
    public override void DivInit()
    {
      if (Block is null)
      {
        Block = new Div("Block");
        Block.BindRenderer<DivPixelRenderer>();
        Block.Design.Color = UITheme.SliderBlock;
        if (Direction is Direction.Vertical)
        {
          Block.Layout.Width = Layout.Width;
          if (Block.Layout.Height == 0)
            Block.Layout.Height = 24;
        }
        if (Direction is Direction.Horizontal)
        {
          if (Block.Layout.Width == 0)
            Block.Layout.Width = 24;
          Block.Layout.Height = Layout.Height;
        }
      }
      if (Renderer is null)
      {
        BindRenderer<DivPixelRenderer>();
      }
      Block.Interact.IsDraggable = true;
      Register(Block);
      base.DivInit();
      _blockBase = Block.Design.Color;
    }

    private Color _blockBase;

    public override void OnUpdate(GameTime time)
    {
      //容器尺寸不足以容纳滑块时钳制区间退化为空, 收拢到下界, 避免越界与 Precent 出现 NaN.
      float minX = Layout.PaddingLeft;
      float minY = Layout.PaddingTop;
      float maxX = Math.Max(minX, Layout.Width - Layout.PaddingRight - Block.Layout.Width);
      float maxY = Math.Max(minY, Layout.Height - Layout.PaddingBottom - Block.Layout.Height);

      Block.Layout.Left = Math.Clamp(Block.Layout.Left, minX, maxX);
      Block.Layout.Top = Math.Clamp(Block.Layout.Top, minY, maxY);

      Block.Layout.Top += WheelVelocity;
      WheelVelocity *= 0.9f;

      float rangeX = maxX - minX;
      float rangeY = maxY - minY;
      Precent = new Vector2(
        rangeX > 0f ? (Block.Layout.Left - minX) / rangeX : 0f,
        rangeY > 0f ? (Block.Layout.Top - minY) / rangeY : 0f);

      //状态反馈: 拖拽中最亮, 悬停次之, 以 DivInit 捕获的底色乘算, 不吃掉外部改色.
      float tint = Block.Events.DraggingState ? 1.2f : Block.Interact.Interaction ? 1.08f : 1f;
      Block.Design.Color = new Color(
        (int)Math.Clamp(_blockBase.R * tint, 0, 255),
        (int)Math.Clamp(_blockBase.G * tint, 0, 255),
        (int)Math.Clamp(_blockBase.B * tint, 0, 255),
        _blockBase.A);

      base.OnUpdate(time);
    }
    public override void LayoutCalculate(ref DivLayout layout)
    {
      base.LayoutCalculate(ref layout);
    }
  }
}