namespace Colin.Core.Modulars.UserInterfaces.Controllers
{
  public class DivGradientController : DivController
  {
    private bool _openState = false;
    private bool _closeState = false;

    public ColorTween OpenColor;
    public ColorTween CloseColor;

    public VectorTween OpenScale;
    public VectorTween CloseScale;

    public event Action OnClosed;

    public override void OnBinded()
    {
      OpenColor = new ColorTween();
      OpenColor.Set(Color.Transparent);
      OpenColor.Target = new Color(255, 255, 255, 255);
      OpenColor.Time = 0.08f;

      CloseColor = new ColorTween();
      CloseColor.Set(Color.White);
      CloseColor.Target = Color.Transparent;
      CloseColor.Time = 0.12f;

      OpenScale = new VectorTween();
      OpenScale.GradientStyle = GradientStyle.EaseOutExpo;
      OpenScale.Set(Vector2.One * 0.7f);
      OpenScale.Target = Vector2.One;
      OpenScale.Time = 0.4f;

      CloseScale = new VectorTween();
      CloseScale.GradientStyle = GradientStyle.EaseOutExpo;
      CloseScale.Set(Vector2.One);
      CloseScale.Target = Vector2.One * 0.7f;
      CloseScale.Time = 2f;
      base.OnBinded();
    }
    public override void OnDivInitialize()
    {

      base.OnDivInitialize();
    }
    public override void Layout(ref DivLayout layout)
    {
      if (_openState)
        layout.Scale = OpenScale.Update();
      if (_closeState)
        layout.Scale = CloseScale.Update();
      base.Layout(ref layout);
    }
    public override void Design(ref DivDesign design)
    {
      if (_openState)
        design.Color = OpenColor.Update();
      if (_closeState)
      {
        design.Color = CloseColor.Update();
        if (design.Color.A <= 0)
        {
          Div.IsVisible = false;
          OnClosed?.Invoke();
        }
      }
      base.Design(ref design);
    }
    public void Open()
    {
      if (!Div.IsVisible)
      {
        OpenColor.Start();
        OpenScale.Start();
        _openState = true;
        _closeState = false;
        Div.IsVisible = true;
      }
    }
    public void Close()
    {
      CloseColor.Start();
      CloseScale.Start();
      _closeState = true;
      _openState = false;
    }
  }
}