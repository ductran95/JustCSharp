using System;
using JustCSharp.Database.MongoDB.Context;
using MongoDB.Driver;

namespace JustCSharp.Database.MongoDB.Model
{
    public class MongoDbContextOptions
    {
        public Type DbContextType { get; set; } = null!;
        public string? ConnectionStringName { get; set; }
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
        public int? Timeout { get; set; }
        public bool LazyConnect { get; set; } = true;
        public bool ConcurrentInit { get; set; } = true;
        public bool AutoGeneratedKey { get; set; } = true;
        public MongoClientSettings Settings { get; set; } = new MongoClientSettings();
    }
    
    public class MongoDbContextOptions<TDbContext>: MongoDbContextOptions where TDbContext: IMongoDbContext
    {
        public MongoDbContextOptions()
        {
            DbContextType = typeof(TDbContext);
        }
    }
}