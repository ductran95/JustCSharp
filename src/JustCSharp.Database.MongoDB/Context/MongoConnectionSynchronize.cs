using System;
using System.Collections.Concurrent;
using System.Threading;
using JustCSharp.Utility.Extensions;

namespace JustCSharp.Database.MongoDB.Context
{
    internal static class MongoConnectionSynchronization
    {
        private static readonly ConcurrentDictionary<Type, MongoConnectionSynchronize> ConnectionSync =
            new ConcurrentDictionary<Type, MongoConnectionSynchronize>();

        internal static MongoConnectionSynchronize GetForType(Type type)
        {
            return ConnectionSync.GetOrAdd(type, () => new MongoConnectionSynchronize()
            {
                Mutex = new SemaphoreSlim(1)
            });
        }
    }

    internal class MongoConnectionSynchronize
    {
        internal bool IsInitConnection { get; set; }
        internal SemaphoreSlim? Mutex { get; set; }
    }
}