using System;
using System.Collections.Concurrent;
using MongoDB.Driver;

namespace JustCSharp.MongoDB.Model
{
    public class MongoDbContextOptions
    {
        public ConcurrentDictionary<Type, MongoDbContextConfig> DbContextConfigs { get; set; } = new ConcurrentDictionary<Type, MongoDbContextConfig>();
    }

    public class MongoDbContextConfig
    {
        public Type DbContextType { get; set; }
        public string ConnectionStringName { get; set; }
        public string ConnectionString { get; set; }
        public int? Timeout { get; set; }
        public MongoClientSettings Settings { get; set; }
    }
}