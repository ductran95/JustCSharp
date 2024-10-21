using System;
using JetBrains.Annotations;

namespace JustCSharp.Uow
{
    public class InfrastructureException: Exception
    {
        public InfrastructureException()
        {
        }

        public InfrastructureException([CanBeNull] string message) : base(message)
        {
        }

        public InfrastructureException([CanBeNull] string message, [CanBeNull] Exception innerException) : base(message, innerException)
        {
        }
    }
}