﻿namespace Colin.Common.Code.Physics.Shared.Contracts
{
    public class EnsuresException : Exception
    {
        public EnsuresException( string message ) : base(message) { }
    }
}