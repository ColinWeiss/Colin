using Colin.Core.Graphics.Tweens;
using Microsoft.Xna.Framework;
using System;

namespace Colin.Core.Modulars.UserInterfaces.Controllers
{
  public class DivGradientController : DivController
  {
    private bool _openState = false;
    private bool _closeState = false;

    public ColorTween OpenColor;
    public ColorTween CloseColor;

    public Vector2Tween OpenScale;
    public Vector2Tween CloseScale;

    public event Action OnClosed;

    public override void OnBinded(Div div)
    {
      OpenColor = new ColorTween()
        .SetFrom(Color.Transparent).SetTo(Color.White).SetDuration(0.08f);

      CloseColor = new ColorTween()
        .SetFrom(Color.White).SetTo(Color.Transparent).SetDuration(0.12f);

      OpenScale = new Vector2Tween()
        .SetFrom(Vector2.One * 0.7f).SetTo(Vector2.One).SetDuration(0.4f).SetEase(Ease.ExpoOut);

      CloseScale = new Vector2Tween()
        .SetFrom(Vector2.One).SetTo(Vector2.One * 0.7f).SetDuration(2f).SetEase(Ease.ExpoOut);

      base.OnBinded(div);
    }

    public override void Layout(Div div, ref DivLayout layout)
    {
      if (_openState)
        layout.Scale = OpenScale.CurrentValue;
      if (_closeState)
        layout.Scale = CloseScale.CurrentValue;
      base.Layout(div, ref layout);
    }

    public override void Design(Div div, ref DivDesign design)
    {
      if (_openState)
        design.Color = OpenColor.CurrentValue;
      if (_closeState)
      {
        design.Color = CloseColor.CurrentValue;
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
