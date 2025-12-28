namespace Colin.Core.Modulars.UserInterfaces.Controllers
{
  public class LinearMenuController : DivController
  {
    /// <summary>
    /// 指示项间隔.
    /// </summary>
    public int DivInterval = 0;
    /// <summary>
    /// 方向.
    /// </summary>
    public Direction Direction = Direction.Vertical;
    /// <summary>
    /// 对齐方式.
    /// </summary>
    public Direction Alignment = Direction.Center;
    /// <summary>
    /// 朝向.
    /// </summary>
    public Direction Toward = Direction.Down;
    public Vector2 Scroll;
    public Vector2 TotalSize;
    public bool AutoSetSize;
    private Div lastDiv;

    public override void Layout(Div div, ref DivLayout layout)
    {
      TotalSize = Vector2.Zero;
      div.ForEach(CalculateLayout);
      if (Direction == Direction.Vertical)
        div.ForEach(Portrait);
      else if (Direction == Direction.Horizontal)
        div.ForEach(Transverse);
      if (Direction == Direction.Vertical)
        TotalSize.Y -= DivInterval;
      else if (Direction == Direction.Horizontal)
        TotalSize.X -= DivInterval;
      lastDiv = null;
      if (AutoSetSize)
        div.Layout.SetSize(TotalSize);
      base.Layout(div, ref layout);
    }
    public void CalculateLayout(Div division)
    {
      switch (Direction)
      {
        case Direction.Vertical:
          if (TotalSize.X < division.Layout.Width)
            TotalSize.X = division.Layout.Width;
          TotalSize.Y += division.Layout.Height + DivInterval;
          break;
        case Direction.Horizontal:
          if (TotalSize.Y < division.Layout.Height)
            TotalSize.Y = division.Layout.Height;
          TotalSize.X += division.Layout.Width + DivInterval;
          break;
      }
    }
    private void Portrait(Div division)
    {
      if (lastDiv != null)
      {
        switch (Toward)
        {
          case Direction.Down:
            division.Layout.Top = lastDiv.Layout.Top + lastDiv.Layout.Height + DivInterval;
            break;
          case Direction.Up:
            division.Layout.Top = lastDiv.Layout.Top - division.Layout.Height - DivInterval;
            break;
        }
      }
      else if (Toward == Direction.Up)
        division.Layout.Top = (int)Scroll.Y + TotalSize.Y - division.Layout.Height;
      else if (Toward == Direction.Down)
        division.Layout.Top = (int)Scroll.Y;
      switch (Alignment)
      {
        case Direction.Left:
          division.Layout.Left = (int)Scroll.X;
          break;
        case Direction.Right:
          division.Layout.Left = (int)Scroll.X + division.Parent.Layout.Width - division.Layout.Width;
          break;
        case Direction.Center:
          division.Layout.Left = (int)Scroll.X + TotalSize.X / 2 - division.Layout.Width / 2;
          break;
      }
      lastDiv = division;
    }
    private void Transverse(Div div)
    {
      if (lastDiv != null)
      {
        switch (Toward)
        {
          case Direction.Right:
            div.Layout.Left = lastDiv.Layout.Left + lastDiv.Layout.Width + DivInterval;
            break;
          case Direction.Left:
            div.Layout.Left = lastDiv.Layout.Left - div.Layout.Width - DivInterval;
            break;
        }
      }
      else if (Toward == Direction.Left)
        div.Layout.Left = (int)Scroll.X + div.Layout.Width - div.Layout.Width;
      else if (Toward == Direction.Right)
        div.Layout.Left = (int)Scroll.X;
      switch (Alignment)
      {
        case Direction.Up:
          div.Layout.Top = (int)Scroll.Y;
          break;
        case Direction.Down:
          div.Layout.Top = (int)Scroll.Y + div.Parent.Layout.Height - div.Layout.Height;
          break;
        case Direction.Center:
          div.Layout.Top = (int)Scroll.Y + TotalSize.Y / 2 - div.Layout.Height / 2;
          break;
      }
      lastDiv = div;
    }
  }
}