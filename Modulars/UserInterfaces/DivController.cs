namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 划分元素控制器.
  /// </summary>
  public class DivController
  {
    internal Div div;
    public Div Div => div;
    public virtual void OnBinded() { }
    public virtual void OnDivInitialize() { }
    public virtual void Layout(ref DivLayout layout) { }
    public virtual void Interact(ref InteractStyle interact) { }
    public virtual void Design(ref DivDesign design) { }
  }
}