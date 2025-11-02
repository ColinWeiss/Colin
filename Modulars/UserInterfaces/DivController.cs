namespace Colin.Core.Modulars.UserInterfaces
{
  /// <summary>
  /// 划分元素控制器.
  /// </summary>
  public class DivController
  {
    protected bool _waking;
    protected bool _hibernating;

    /// <summary>
    /// 指示划分元素处于唤醒过程中.
    /// </summary>
    public bool Waking => _waking;

    /// <summary>
    /// 指示划分元素处于休眠过程中.
    /// </summary>
    public bool Hibernating => _hibernating;

    public virtual void OnBinded(Div div) { }
    public virtual void OnDivInitialize(Div div) { }

    public virtual void DoWakeUp(Div div) { }
    public virtual void DoHibernate(Div div) { }

    public virtual void Layout(Div div, ref DivLayout layout) { }
    public virtual void Interact(Div div, ref InteractStyle interact) { }
    public virtual void Design(Div div, ref DivDesign design) { }
  }
}