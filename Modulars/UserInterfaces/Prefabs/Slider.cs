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

    private Div Content;
    private Div ContentContainer;
    private Div Response;
    public void Bind(Div content, Div contentContainer, Div response)
    {
      if (Response is not null)
        Response.Events.UnconditionalHover -= WheelEvent;
      Content = content;
      ContentContainer = contentContainer;
      Response = response;
      Response.Events.UnconditionalHover += WheelEvent;
    }
    private void WheelEvent(MouseEventArgs args)
    {
      if (MouseResponder.ScrollDown)
        Block.Layout.Top -= 2;
      else if (MouseResponder.ScrollUp)
        Block.Layout.Top += 2;
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
          Block.Layout.Height = 24;
        }
        if (Direction is Direction.Horizontal)
        {
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
      Block.Layout.Left = Math.Clamp(Block.Layout.Left, 0, Layout.Width - Block.Layout.Width);
      Block.Layout.Top = Math.Clamp(Block.Layout.Top, 0, Layout.Height - Block.Layout.Height);

      Precent = Block.Layout.Location / (Layout.Size - Block.Layout.Size);

      if (Content is not null && ContentContainer is not null)
      {
        if (Content.Layout.Width > ContentContainer.Layout.Width)
          Content.Layout.Left = (int)-(Precent.X * (Content.Layout.Width - ContentContainer.Layout.Width));
        else
          Content.Layout.Left = 0;

        if (Content.Layout.Height > ContentContainer.Layout.Height)
          Content.Layout.Top = (int)-(Precent.Y * (Content.Layout.Height - ContentContainer.Layout.Height));
        else
          Content.Layout.Top = 0;
      }
      base.OnUpdate(time);
    }
  }
}