namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// [过渡] 界面根: 改用 <c>new Div(name)</c> + <see cref="Div.MarkAsRoot"/> (或 <see cref="DivFactory.Root"/>).
  /// <br>保留它只是为了让还没迁完的页面类继续编译; 新代码不要用.</br>
  /// </summary>
  [Obsolete("改用 Div + MarkAsRoot() / DivFactory.Root(name)")]
  public class DivRoot : Div
  {
    public DivRoot(string name) : base(name) => MarkAsRoot();

    public override sealed void DivInit()
    {
      Layout.Width = CoreInfo.ViewWidth;
      Layout.Height = CoreInfo.ViewHeight;
      RootInitialize();
      base.DivInit();
    }

    /// <summary>在此处进行容器初始化操作.</summary>
    public virtual void RootInitialize() { }
  }
}
