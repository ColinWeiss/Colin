
namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 用户交互界面中的根划分元素.
  /// <br>"万物皆有「阈点」..."</br>
  /// </summary>
  public class DivRoot : Div
  {
    public DivRoot(string name) : base(name) => _root = this;
    public override sealed void DivInit()
    {
      Interact.IsSelectable = false;
      Layout.Width = CoreInfo.ViewWidth;
      Layout.Height = CoreInfo.ViewHeight;
      RootInitialize();
      base.DivInit();
    }

    /// <summary>
    /// 在此处进行容器初始化操作.
    /// </summary>
    public virtual void RootInitialize() { }

    /// <summary>
    /// 于每帧布局计算前同步视口尺寸, 确保本帧布局与渲染使用的是最新尺寸.
    /// </summary>
    public override void LayoutCalculate(ref DivLayout layout)
    {
      Layout.Width = CoreInfo.ViewWidth;
      Layout.Height = CoreInfo.ViewHeight;
      base.LayoutCalculate(ref layout);
    }
  }
}