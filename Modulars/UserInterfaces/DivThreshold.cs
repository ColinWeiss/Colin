namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 用户交互界面中的根划分元素.
  /// <br>"万物皆有「阈点」..."</br>
  /// </summary>
  public class DivThreshold : Div
  {
    public DivThreshold(string name) : base(name) => threshold = this;
    public override sealed void DivInit()
    {
      Interact.IsInteractive = false;
      Interact.IsSelectable = false;
      Layout.Width = EngineInfo.ViewWidth;
      Layout.Height = EngineInfo.ViewHeight;
      ContainerInitialize();
      UserInterface.Scene.Events.ClientSizeChanged += Events_ClientSizeChanged;
      base.DivInit();
    }
    private void Events_ClientSizeChanged(object sender, EventArgs e)
    {
      Layout.Width = EngineInfo.ViewWidth;
      Layout.Height = EngineInfo.ViewHeight;
    }

    /// <summary>
    /// 在此处进行容器初始化操作.
    /// </summary>
    public virtual void ContainerInitialize() { }
    public void SetTop(Div division)
    {
      Remove(division);
      Register(division, true);
    }
    public override bool Register(Div division, bool doInit = false)
    {
      if (base.Register(division, doInit))
      {
        return true;
      }
      else
        return false;
    }
  }
}