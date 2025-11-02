using Colin.Core.Modulars.UserInterfaces.Controllers;
using Colin.Core.Modulars.UserInterfaces.Prefabs;
using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Forms
{
  public class Popup : Canvas
  {
    private int _titleHeight;

    public Popup(string name, int width, int height, int titleHeight) : base(name)
    {
      Layout.Width = width;
      Layout.Height = height;
      _titleHeight = titleHeight;
    }

    public Div Substrate;

    public Div TitleColumn;

    public Div Icon;

    public Div CloseButton;

    public Div Block;

    public override sealed void DivInit()
    {
      Layout.Scale = Vector2.One;
      Design.Color = Color.White;

      BindController<DivGradientController>();
      Interact.IsDraggable = true;
      Interact.IsSelectable = false;

      Substrate = new Div("Substrate");
      Substrate.Layout.Left = -4;
      Substrate.Layout.Top = -4;
      Substrate.Layout.Width = Layout.Width + 8;
      Substrate.Layout.Height = Layout.Height + _titleHeight + 8;
      base.Register(Substrate);

      Block = new Div("Behavior");
      Block.BindRenderer<DivPixelRenderer>();
      Block.Design.Color = new Color(17, 18, 20);
      Block.Layout.Top = _titleHeight;
      Block.Layout.Width = Layout.Width;
      Block.Layout.Height = Layout.Height;
      base.Register(Block);

      TitleColumn = new Div("TitleColumn");
      TitleColumn.Layout.Width = Layout.Width;
      TitleColumn.Layout.Height = _titleHeight;
      {
        Icon = new Div("Icon");
        Icon.Layout.Left = 4;
        Icon.Layout.Top = 4;
        Icon.Layout.Width = 8;
        Icon.Layout.Height = 8;
        TitleColumn.Register(Icon);

        CloseButton = new Div("CloseButton");
        CloseButton.Layout.Left = TitleColumn.Layout.Width - 16;
        CloseButton.Layout.Top = 2;
        CloseButton.Layout.Width = 14;
        CloseButton.Layout.Height = 12;
        TitleColumn.Register(CloseButton);
      }
      base.Register(TitleColumn);

      PopupInit();

      Layout.Width += 8;
      Layout.Height += _titleHeight + 8;

      Events.KeysClicked += (s, e) =>
      {
        if (e.Keys == Keys.Escape && base.IsVisible)
        {
          e.StopBubbling = true;
          DoHibernate();
        }
      };
      base.DivInit();
    }
    public virtual void PopupInit() { }
    public override bool Register(Div division, bool doInit = false) => Block.Register(division, doInit);
  }
}