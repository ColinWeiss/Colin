namespace Colin.Core.Common
{
  public interface IBehavior
  {
    public void DoInitialize();
    public void DoUpdate(GameTime time);
    public void DoRender();
  }
}