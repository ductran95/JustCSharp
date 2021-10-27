using JustCSharp.Uow;

namespace JustCSharp.MongoDB.Context
{
    public class MongoDbDatabase: IDatabase
    {
        public IMongoDbContext DbContext { get; }

        public MongoDbDatabase(IMongoDbContext dbContext)
        {
            DbContext = dbContext;
        }
    }
}