using Colin.Core.Modulars.UserInterfaces.Controllers;
using SharpDX.Direct3D9;

namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
  public class DivList : Div
  {
    public DivList(string name, bool isCanvas = false) : base(name, isCanvas)
    {
      List = new Div("List");
    }
    public Div List;

    public LinearMenuController Linear
    {
      get
      {
        if (List.Controller is null)
        {
          var linear = List.BindController<LinearMenuController>();
          linear.AutoSetSize = true;
          linear.DivInterval = 4;
          return linear;
        }
        else
          return List.GetController<LinearMenuController>();
      }
    }

    public Slider Slider;

    public override void DivInit()
    {
      Layout.ScissorEnable = true;
      Layout.ScissorWidth = (int)Layout.Width;
      Layout.ScissorHeight = (int)Layout.Height;

      if (List.Controller is null)
      {
        var linear = List.BindController<LinearMenuController>();
        linear.AutoSetSize = true;
        linear.DivInterval = 4;
      }
      base.Register(List);
      base.DivInit();
    }
    public override void LayoutCalculate(ref DivLayout layout)
    {
      Layout.ScissorWidth = (int)Layout.Width;
      Layout.ScissorHeight = (int)Layout.Height;

      if (Slider is not null)
      {
        if (List.Layout.Width > Layout.Width)
          List.Layout.Left = (int)-(Slider.Precent.X * (List.Layout.Width - Layout.Width)) + Layout.Left;
        else
          List.Layout.Left = 0;

        if (List.Layout.Height > Layout.Height)
          List.Layout.Top = (int)-(Slider.Precent.Y * (List.Layout.Height - Layout.Height)) + Layout.Top;
        else
          List.Layout.Top = 0;

        if (List.Layout.Height <= 0 || Linear.TotalSize.Y < Layout.Height)
          return;
        else
          Slider.Block.Layout.Height = (Layout.Height / Linear.TotalSize.Y) * Slider.Layout.Height;
      }
      base.LayoutCalculate(ref layout);
    }

    public void BindSlider(Slider slider)
    {
      Slider = slider;
      Slider.Bind(List, this);
    }
    public override bool Register(Div div, bool doInit = false)
    {
      return List.Register(div, doInit);
    }
  }
}