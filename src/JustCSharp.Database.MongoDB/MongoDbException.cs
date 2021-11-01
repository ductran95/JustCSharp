using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using JustCSharp.Uow;

namespace JustCSharp.Database.MongoDB
{
    public class MongoDbException: InfrastructureException
    {
        public MongoDbException()
        {
        }

        protected MongoDbException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MongoDbException([CanBeNull] string message) : base(message)
        {
        }

        public MongoDbException([CanBeNull] string message, [CanBeNull] Exception innerException) : base(message, innerException)
        {
        }
    }
}