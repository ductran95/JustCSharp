using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JustCSharp.Database.MongoDB.Model
{
    public interface IMongoEntityModelCache
    {
        ConcurrentDictionary<Type, Dictionary<Type, IMongoEntityModel>> DbModelCache { get; }
    }
}