using Colin.Core.Graphics.Tweens;

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

    public override void OnBinded(Div div)
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
      base.OnBinded(div);
    }
    public override void Layout(Div div, ref DivLayout layout)
    {
      if (_openState)
        layout.Scale = OpenScale.DoUpdate();
      if (_closeState)
        layout.Scale = CloseScale.DoUpdate();
      base.Layout(div, ref layout);
    }
    public override void Design(Div div, ref DivDesign design)
    {
      if (_openState)
        design.Color = OpenColor.DoUpdate();
      if (_closeState)
      {
        design.Color = CloseColor.DoUpdate();
        if (design.Color.A <= 0)
        {
          div.IsVisible = false;
          OnClosed?.Invoke();
        }
      }
      base.Design(div, ref design);
    }
    protected override void OnWakeUp(Div div)
    {
      if (!div.IsVisible)
      {
        OpenColor.Play();
        OpenScale.Play();
        _openState = true;
        _closeState = false;
        div.IsVisible = true;
      }
      base.OnWakeUp(div);
    }
    protected override void OnHibernate(Div div)
    {
      CloseColor.Play();
      CloseScale.Play();
      _closeState = true;
      _openState = false;
      base.OnHibernate(div);
    }
  }
}