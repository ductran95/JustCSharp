using JustCSharp.Uow;

namespace JustCSharp.Database.MongoDB.Context
{
    public class MongoDbDatabase: IDatabase
    {
        public IMongoDbContext DbContext { get; private set; }

        public MongoDbDatabase(IMongoDbContext dbContext)
        {
            DbContext = dbContext;
        }
    }
}