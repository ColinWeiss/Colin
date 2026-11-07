using Colin.Core.Modulars.UserInterfaces.Controllers;
using Colin.Core.Modulars.UserInterfaces.Renderers;

namespace Colin.Core.Modulars.UserInterfaces.Scripts
{
  /// <summary>
  /// 列表容器行为: 一个内容容器的裁剪视口 + 线性排列 + 可选外部滚动条.
  /// <br>节点形状: 子级 <c>"List"</c> 是内容容器.</br>
  /// </summary>
  public class DivListScript : DivScript
  {
    /// <summary>内容容器子节点的约定名字.</summary>
    public const string ListNodeName = "List";

    /// <summary>外部绑定的滚动条 (可选; 由 <see cref="BindSlider"/> 建立).</summary>
    public Div Slider;

    private Div _list;
    private SliderScript _sliderScript;

    /// <summary>内容容器; 项挂到它上面, 由线性排列控制器逐帧排列.</summary>
    public Div List => _list ??= Owner?.GetChild(ListNodeName);

    /// <summary>内容容器的线性排列控制器 (没有就补一个).</summary>
    public LinearMenuController Linear
    {
      get
      {
        if (_list is null)
          return null;
        if (_list.Controller is LinearMenuController linear)
          return linear;
        linear = _list.BindController<LinearMenuController>();
        linear.AutoSetSize = true;
        linear.DivInterval = 4;
        return linear;
      }
    }

    protected override void OnBind()
    {
      Div owner = Owner;
      if (owner is null)
        return;
      owner.Layout.ScissorEnable = true;
      owner.Layout.ScissorWidth = (int)owner.Layout.Width;
      owner.Layout.ScissorHeight = (int)owner.Layout.Height;

      _list = owner.GetChild(ListNodeName);
      if (_list is null)
      {
        _list = new Div(ListNodeName);
        owner.Attach(_list);
      }
      if (_list.Controller is null)
      {
        LinearMenuController controller = _list.BindController<LinearMenuController>();
        controller.AutoSetSize = true;
        controller.DivInterval = 4;
      }
      _list.LayoutScripted = true;
    }

    /// <summary>绑定外部滚动条 (滚动条自己可以是任意节点, 只要挂 <see cref="SliderScript"/>).</summary>
    public void BindSlider(Div slider)
    {
      Slider = slider;
      _sliderScript = slider?.GetScript<SliderScript>();
      if (_sliderScript is null && slider is not null)
        _sliderScript = slider.BindScript(new SliderScript());
      _sliderScript?.Bind(_list, Owner);
    }

    protected override void OnLayoutCalculate(ref DivLayout layout)
    {
      Div owner = Owner;
      if (owner is null)
        return;
      owner.Layout.ScissorWidth = (int)owner.Layout.Width;
      owner.Layout.ScissorHeight = (int)owner.Layout.Height;
      if (_list is null || _sliderScript is null)
        return;

      SliderScript slider = _sliderScript;
      //[!] 内容容器的 Left/Top 是"相对本容器"的坐标, 不能叠加本容器自己的 Layout.Left/Top.
      if (_list.Layout.Width > owner.Layout.Width)
        _list.Layout.Left = (int)-(slider.Precent.X * (_list.Layout.Width - owner.Layout.Width));
      else
        _list.Layout.Left = 0;

      if (_list.Layout.Height > owner.Layout.Height)
        _list.Layout.Top = (int)-(slider.Precent.Y * (_list.Layout.Height - owner.Layout.Height));
      else
        _list.Layout.Top = 0;

      LinearMenuController linear = Linear;
      if (linear is null || _list.Layout.Height <= 0 || linear.TotalSize.Y < owner.Layout.Height)
        return;
      if (slider.Block is not null)
        slider.Block.Layout.Height = (owner.Layout.Height / linear.TotalSize.Y) * Slider.Layout.Height;
    }
  }
}
