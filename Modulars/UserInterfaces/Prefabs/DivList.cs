using Colin.Core.Modulars.UserInterfaces.Controllers;

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
      base.LayoutCalculate(ref layout);
    }
    public void GetSlider(Slider slider)
    {
      slider.Bind(List, this);
    }
    public override bool Register(Div div, bool doInit = false)
    {
      return List.Register(div, doInit);
    }
  }
}