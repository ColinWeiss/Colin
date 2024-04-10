namespace Colin.Core.Extensions
{
  public static class EventExt
  {
    public static void EventRaise<TEventArgs>(this object sender, EventHandler<TEventArgs> handler, TEventArgs e)
    {
      handler?.Invoke(sender, e);
    }
    public static void EventRaise(this object sender, EventHandler handler, EventArgs e)
    {
      handler?.Invoke(sender, e);
    }
  }
}