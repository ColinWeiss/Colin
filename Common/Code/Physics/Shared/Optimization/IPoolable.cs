namespace Colin.Common.Code.Physics.Shared.Optimization
{
    public interface IPoolable<T> : IDisposable where T : IPoolable<T>
    {
        void Reset( );
    }
}