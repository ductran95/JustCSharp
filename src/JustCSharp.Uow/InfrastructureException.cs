using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace JustCSharp.Uow
{
    public class InfrastructureException: Exception
    {
        public InfrastructureException()
        {
        }

        protected InfrastructureException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
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