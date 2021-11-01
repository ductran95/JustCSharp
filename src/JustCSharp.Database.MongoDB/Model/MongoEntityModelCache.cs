using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JustCSharp.Database.MongoDB.Model
{
    public class MongoEntityModelCache : IMongoEntityModelCache
    {
        protected readonly ConcurrentDictionary<Type, Dictionary<Type, IMongoEntityModel>> _dbModelCache = new ConcurrentDictionary<Type, Dictionary<Type, IMongoEntityModel>>();
        
        public ConcurrentDictionary<Type, Dictionary<Type, IMongoEntityModel>> DbModelCache => _dbModelCache;
    }
}