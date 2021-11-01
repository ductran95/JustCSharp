using System;
using JustCSharp.Database.MongoDB.Context;
using MongoDB.Driver;

namespace JustCSharp.Database.MongoDB.Model
{
    public class MongoDbContextOptions
    {
        public Type DbContextType { get; set; }
        public string ConnectionStringName { get; set; }
        public string ConnectionString { get; set; }
        public int? Timeout { get; set; }
        public bool LazyConnect { get; set; } = true;
        public MongoClientSettings Settings { get; set; }
    }
    
    public class MongoDbContextOptions<TDbContext>: MongoDbContextOptions where TDbContext: IMongoDbContext
    {
        public MongoDbContextOptions()
        {
            DbContextType = typeof(TDbContext);
        }
    }
}