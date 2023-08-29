namespace Colin.Core.Extensions
{
    public static class EventExt
    {
        public static void EventRaise<TEventArgs>( this ITraceable sender, EventHandler<TEventArgs> handler, TEventArgs e )
        {
            handler?.Invoke( sender, e );
        }
        public static void EventRaise( this ITraceable sender, EventHandler handler, EventArgs e )
        {
            handler?.Invoke( sender, e );
        }
    }
}