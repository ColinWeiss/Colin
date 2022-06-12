namespace Colin.Common.Code.Physics.Shared.Contracts
{
    public class RequiredException : Exception
    {
        public RequiredException( string message ) : base( message ) { }
    }
}