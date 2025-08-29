using Colin.Core.Events;
using Colin.Core.Modulars.UserInterfaces.Controllers;
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
        View.Events.LeftUp.Event -= WheelEvent;
      List = list;
      View = view;
      View.Events.LeftUp.Event += WheelEvent;
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
        Block.Design.Color = new Color(255, 223, 135);
        if (Direction is Direction.Vertical)
        {
          Block.Layout.Width = Layout.Width;
          if(Block.Layout.Height == 0)
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
    }
    public override void OnUpdate(GameTime time)
    {
      Block.Layout.Left = Math.Clamp(Block.Layout.Left, Layout.PaddingLeft, Layout.Width - Layout.PaddingRight - Block.Layout.Width);
      Block.Layout.Top = Math.Clamp(Block.Layout.Top, Layout.PaddingTop, Layout.Height - Layout.PaddingBottom - Block.Layout.Height);

      Block.Layout.Top += WheelVelocity;
      WheelVelocity *= 0.9f;

      Precent =
        (Block.Layout.Location - new Vector2(Layout.PaddingLeft, Layout.PaddingTop))
        /
        (Layout.Size - Block.Layout.Size - new Vector2(Layout.PaddingLeft + Layout.PaddingRight, Layout.PaddingTop + Layout.PaddingBottom));

      base.OnUpdate(time);
    }
    public override void LayoutCalculate(ref DivLayout layout)
    {
      base.LayoutCalculate(ref layout);
    }
  }
}